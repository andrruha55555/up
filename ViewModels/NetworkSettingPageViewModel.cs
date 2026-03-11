using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.ViewModels
{
    /// <summary>
    /// Строка таблицы сетевых настроек с именем оборудования и ICMP-статусом.
    /// </summary>
    public class NetworkRow : INotifyPropertyChanged
    {
        /// <summary>Исходная модель сетевой настройки</summary>
        public NetworkSetting Setting { get; }

        /// <summary>Название оборудования (из справочника)</summary>
        public string EquipmentName { get; }

        public int Id => Setting.id;
        public string IpAddress => Setting.ip_address ?? "—";
        public string SubnetMask => Setting.subnet_mask ?? "—";
        public string Gateway => Setting.gateway ?? "—";
        public string Dns1 => Setting.dns1 ?? "—";
        public string Dns2 => Setting.dns2 ?? "—";
        public string CreatedAt => Setting.created_at?.ToString("dd.MM.yyyy HH:mm") ?? "—";

        private string _statusText = "—";
        /// <summary>Текстовый результат ICMP-проверки</summary>
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        public NetworkRow(NetworkSetting s, Dictionary<int, string> equipmentMap)
        {
            Setting = s;
            EquipmentName = equipmentMap.TryGetValue(s.equipment_id, out var n) ? n : $"ID {s.equipment_id}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// ViewModel страницы сетевых настроек.
    /// Поддерживает загрузку, фильтрацию и ICMP-проверку через RAW-сокет.
    /// </summary>
    public class NetworkSettingPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;
        private readonly IcmpService _icmpService = new();
        private CancellationTokenSource? _checkCts;
        private List<NetworkRow> _allRows = new();

        public ObservableCollection<NetworkSetting> NetworkSettings { get; } = new();
        public ObservableCollection<NetworkRow> FilteredNetworkSettings { get; } = new();

        private NetworkSetting? _selectedNetworkSetting;
        /// <summary>Выбранная настройка в DataGrid</summary>
        public NetworkSetting? SelectedNetworkSetting
        {
            get => _selectedNetworkSetting;
            set { _selectedNetworkSetting = value; OnPropertyChanged(); }
        }

        private string _searchText = "";
        /// <summary>Текст поиска</summary>
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value ?? ""; OnPropertyChanged(); FilterNetwork(); }
        }

        private bool _isChecking;
        /// <summary>True пока идёт ICMP-проверка</summary>
        public bool IsChecking
        {
            get => _isChecking;
            set { _isChecking = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanCheck)); }
        }

        private string _checkProgress = "";
        /// <summary>Прогресс проверки, например "5 / 12"</summary>
        public string CheckProgress
        {
            get => _checkProgress;
            set { _checkProgress = value; OnPropertyChanged(); }
        }

        /// <summary>Можно запустить проверку (не выполняется уже)</summary>
        public bool CanCheck => !IsChecking;

        public NetworkSettingPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;
        }

        // ─── Загрузка ─────────────────────────────────────────────────────────

        /// <summary>Загружает настройки и строит строки таблицы с именами оборудования.</summary>
        public async Task LoadDataAsync()
        {
            var settingsTask = _cacheService.GetOrAddAsync("network_settings",
                async () => await _apiService.GetListAsync<NetworkSetting>("NetworkSettingsController"));
            var equipmentTask = _apiService.GetListAsync<Equipment>("EquipmentController");

            await Task.WhenAll(settingsTask, equipmentTask);

            var settings = settingsTask.Result ?? new();
            var equipment = equipmentTask.Result ?? new();
            var eqMap = equipment.ToDictionary(e => e.id, e => e.name ?? $"ID {e.id}");

            NetworkSettings.Clear();
            _allRows.Clear();

            foreach (var s in settings)
            {
                NetworkSettings.Add(s);
                _allRows.Add(new NetworkRow(s, eqMap));
            }

            FilterNetwork();
        }

        // ─── Фильтрация ───────────────────────────────────────────────────────

        /// <summary>Фильтрует строки по SearchText без аргумента (для code-behind).</summary>
        public void FilterNetwork() => FilterNetwork(_searchText);

        /// <summary>Фильтрует строки по заданному тексту.</summary>
        public void FilterNetwork(string search)
        {
            var q = (search ?? "").Trim().ToLowerInvariant();
            IEnumerable<NetworkRow> items = _allRows;

            if (!string.IsNullOrWhiteSpace(q))
                items = items.Where(x =>
                    x.IpAddress.ToLowerInvariant().Contains(q) ||
                    x.Gateway.ToLowerInvariant().Contains(q) ||
                    x.Dns1.ToLowerInvariant().Contains(q) ||
                    x.EquipmentName.ToLowerInvariant().Contains(q));

            FilteredNetworkSettings.Clear();
            foreach (var x in items) FilteredNetworkSettings.Add(x);
        }

        // ─── ICMP-проверка ────────────────────────────────────────────────────

        /// <summary>
        /// Запускает параллельную ICMP-проверку всех видимых устройств.
        /// Использует RAW ICMP-сокет (не ping).
        /// </summary>
        public async Task CheckAllDevicesAsync()
        {
            if (IsChecking) return;

            _checkCts?.Cancel();
            _checkCts = new CancellationTokenSource();
            var token = _checkCts.Token;

            IsChecking = true;
            CheckProgress = "";

            var rows = FilteredNetworkSettings.ToList();

            if (rows.Count == 0)
            {
                MessageBox.Show("Нет устройств для проверки.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                IsChecking = false;
                return;
            }

            foreach (var row in rows)
                row.StatusText = "⏳ Ожидание...";

            int done = 0;
            int total = rows.Count;

            // Ограничиваем параллелизм — 8 одновременных проверок
            var semaphore = new SemaphoreSlim(8, 8);

            var tasks = rows.Select(async row =>
            {
                await semaphore.WaitAsync(token);
                try
                {
                    if (token.IsCancellationRequested) return;
                    row.StatusText = "🔄 Проверка...";
                    var result = await _icmpService.CheckHostAsync(row.IpAddress, token);
                    row.StatusText = result.StatusText;
                }
                finally
                {
                    semaphore.Release();
                    CheckProgress = $"{Interlocked.Increment(ref done)} / {total}";
                }
            });

            try
            {
                await Task.WhenAll(tasks);
                CheckProgress = $"✅ Проверено: {total} устройств";
            }
            catch (OperationCanceledException)
            {
                CheckProgress = "Проверка отменена";
            }
            finally
            {
                IsChecking = false;
            }
        }

        /// <summary>Отменяет текущую ICMP-проверку.</summary>
        public void CancelCheck() => _checkCts?.Cancel();

        // ─── CRUD ─────────────────────────────────────────────────────────────

        public async Task AddNetworkSettingAsync(NetworkSetting item)
        {
            await _apiService.AddItemAsync("NetworkSettingsController", item);
            _cacheService.Remove("network_settings");
            await LoadDataAsync();
        }

        public async Task UpdateNetworkSettingAsync(int id, NetworkSetting item)
        {
            await _apiService.UpdateItemAsync("NetworkSettingsController", id, item);
            _cacheService.Remove("network_settings");
            await LoadDataAsync();
        }

        public async Task DeleteNetworkSettingAsync(int id)
        {
            await _apiService.DeleteItemAsync("NetworkSettingsController", id);
            _cacheService.Remove("network_settings");
            await LoadDataAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
