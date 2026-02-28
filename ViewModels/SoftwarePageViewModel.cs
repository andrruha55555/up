using AdminUP.Models;
using AdminUP.Services;
using System;
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
        public ObservableCollection<SoftwareEntity> FilteredSoftwareList { get; } = new();

        // ✅ разработчики
        public ObservableCollection<Developer> DeveloperList { get; } = new();

        private SoftwareEntity? _selectedSoftware;
        public SoftwareEntity? SelectedSoftware
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
                // ✅ Developers
                var developers = await _cache.GetOrSetAsync("developers_list",
                    async () => await _api.GetListAsync<Developer>("DevelopersController"));

                DeveloperList.Clear();
                if (developers != null)
                    foreach (var d in developers)
                        DeveloperList.Add(d);

                OnPropertyChanged(nameof(DeveloperList));

                // ✅ Software
                var software = await _cache.GetOrSetAsync("software_list",
                    async () => await _api.GetListAsync<SoftwareEntity>("SoftwareController"));

                SoftwareList.Clear();
                if (software != null)
                    foreach (var s in software)
                        SoftwareList.Add(s);

                FilterSoftware();
            }
            catch (Exception ex)
            {
                // важно чтобы ты видел ошибку API/DB
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
            var items = SoftwareList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                items = items.Where(x =>
                    (x.name ?? "").ToLowerInvariant().Contains(q) ||
                    (x.version ?? "").ToLowerInvariant().Contains(q));
            }

            foreach (var i in items)
                FilteredSoftwareList.Add(i);

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
}