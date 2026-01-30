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

        public ObservableCollection<Software> SoftwareList { get; } = new();
        public ObservableCollection<Software> FilteredSoftwareList { get; } = new();
        public ObservableCollection<Developer> DeveloperList { get; } = new();

        private Software? _selectedSoftware;
        public Software? SelectedSoftware
        {
            get => _selectedSoftware;
            set { _selectedSoftware = value; OnPropertyChanged(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterSoftware(); }
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
            try
            {
                IsLoading = true;

                var developers = await _cache.GetOrAddAsync("developers", async () =>
                    await _api.GetListAsync<Developer>("DevelopersController"));

                DeveloperList.Clear();
                foreach (var d in developers) DeveloperList.Add(d);

                var software = await _cache.GetOrAddAsync("software", async () =>
                    await _api.GetListAsync<Software>("SoftwareController"));

                SoftwareList.Clear();
                foreach (var s in software)
                {
                    // Подмешаем имя разработчика для отображения
                    s.DeveloperName = DeveloperList.FirstOrDefault(x => x.Id == s.DeveloperId)?.Name ?? "";
                    SoftwareList.Add(s);
                }

                FilterSoftware();
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
                items = items.Where(x =>
                    (x.Name ?? "").ToLowerInvariant().Contains(q) ||
                    (x.Version ?? "").ToLowerInvariant().Contains(q) ||
                    (x.DeveloperName ?? "").ToLowerInvariant().Contains(q));

            foreach (var i in items) FilteredSoftwareList.Add(i);
        }

        public async Task<bool> AddSoftwareAsync(Software item)
        {
            var ok = await _api.AddItemAsync("SoftwareController", item);
            _cache.Remove("software");
            await LoadDataAsync();
            return ok;
        }

        public async Task<bool> UpdateSoftwareAsync(int id, Software item)
        {
            var ok = await _api.UpdateItemAsync("SoftwareController", id, item);
            _cache.Remove("software");
            await LoadDataAsync();
            return ok;
        }

        public async Task<bool> DeleteSoftwareAsync(int id)
        {
            var ok = await _api.DeleteItemAsync("SoftwareController", id);
            _cache.Remove("software");
            await LoadDataAsync();
            return ok;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
