using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AdminUP.ViewModels
{
    public class SoftwarePageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _api;
        private readonly CacheService _cache;

        public ObservableCollection<SoftwareEntity> SoftwareList { get; } = new();
        public ObservableCollection<SoftwareRow> FilteredSoftwareList { get; } = new();
        public ObservableCollection<Developer> DeveloperList { get; } = new();

        private List<SoftwareRow> _allRows = new();
        private Dictionary<int, string> _developerNames = new();

        private object? _selectedSoftware;
        public object? SelectedSoftware
        {
            get => _selectedSoftware;
            set { _selectedSoftware = value; OnPropertyChanged(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value ?? ""; OnPropertyChanged(); FilterSoftware(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public SoftwarePageViewModel(ApiService api, CacheService cache)
        {
            _api = api;
            _cache = cache;
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                var developersTask = _cache.GetOrSetAsync("developers_list",
                    async () => await _api.GetListAsync<Developer>("DevelopersController"));
                var softwareTask = _cache.GetOrSetAsync("software_list",
                    async () => await _api.GetListAsync<SoftwareEntity>("SoftwareController"));

                await Task.WhenAll(developersTask, softwareTask);

                var developers = developersTask.Result ?? new();
                _developerNames = developers.ToDictionary(d => d.id, d => d.name ?? d.id.ToString());

                DeveloperList.Clear();
                foreach (var d in developers) DeveloperList.Add(d);
                OnPropertyChanged(nameof(DeveloperList));

                SoftwareList.Clear();
                _allRows.Clear();
                var software = softwareTask.Result ?? new();
                foreach (var s in software)
                {
                    SoftwareList.Add(s);
                    _allRows.Add(new SoftwareRow(s, _developerNames));
                }

                FilterSoftware();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки ПО/разработчиков:\n{ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterSoftware()
        {
            FilteredSoftwareList.Clear();

            var q = (SearchText ?? "").Trim().ToLowerInvariant();
            IEnumerable<SoftwareRow> source = _allRows;

            if (!string.IsNullOrWhiteSpace(q))
            {
                source = source.Where(r =>
                    (r.Software.name ?? "").ToLowerInvariant().Contains(q) ||
                    (r.Software.version ?? "").ToLowerInvariant().Contains(q) ||
                    (r.DeveloperName ?? "").ToLowerInvariant().Contains(q));
            }

            foreach (var r in source) FilteredSoftwareList.Add(r);
            OnPropertyChanged(nameof(FilteredSoftwareList));
        }

        public async Task<bool> AddSoftwareAsync(SoftwareEntity item)
        {
            var ok = await _api.AddItemAsync("SoftwareController", item);
            _cache.Remove("software_list");
            await LoadDataAsync();
            return ok;
        }

        public async Task<bool> UpdateSoftwareAsync(int id, SoftwareEntity item)
        {
            var ok = await _api.UpdateItemAsync("SoftwareController", id, item);
            _cache.Remove("software_list");
            await LoadDataAsync();
            return ok;
        }

        public async Task<bool> DeleteSoftwareAsync(int id)
        {
            var ok = await _api.DeleteItemAsync("SoftwareController", id);
            _cache.Remove("software_list");
            await LoadDataAsync();
            return ok;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
          => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class SoftwareRow
    {
        public SoftwareEntity Software { get; }
        public int Id => Software.id;
        public string Name => Software.name;
        public string DeveloperName { get; }
        public string Version => Software.version;

        public SoftwareRow(SoftwareEntity s, Dictionary<int, string> developers)
        {
            Software = s;
            DeveloperName = s.developer_id.HasValue && developers.TryGetValue(s.developer_id.Value, out var d) ? d : "—";
        }
    }
}
