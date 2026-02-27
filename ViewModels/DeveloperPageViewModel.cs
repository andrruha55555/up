using AdminUP.Models;
using AdminUP.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AdminUP.ViewModels
{
    public class DeveloperPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        public ObservableCollection<Developer> Developers { get; } = new();
        public ObservableCollection<Developer> FilteredDevelopers { get; } = new();

        private Developer? _selectedDeveloper;
        public Developer? SelectedDeveloper
        {
            get => _selectedDeveloper;
            set { _selectedDeveloper = value; OnPropertyChanged(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value ?? ""; OnPropertyChanged(); }
        }

        public DeveloperPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;
        }

        // ТВОЙ code-behind ждёт такой метод
        public async Task LoadDevelopersAsync()
        {
            var list = await _cacheService.GetOrAddAsync("developers", async () =>
                await _apiService.GetListAsync<Developer>("DevelopersController"));

            Developers.Clear();
            if (list != null)
                foreach (var x in list) Developers.Add(x);

            FilterDevelopers();
        }

        // ТВОЙ code-behind вызывает FilterDevelopers() без аргументов
        public void FilterDevelopers()
        {
            FilterDevelopers(SearchText);
        }

        // Оставляем и вариант с параметром (удобно)
        public void FilterDevelopers(string search)
        {
            var q = (search ?? "").Trim().ToLowerInvariant();

            IEnumerable<Developer> items = Developers;
            if (!string.IsNullOrWhiteSpace(q))
                items = items.Where(x => (x.name ?? "").ToLowerInvariant().Contains(q));

            FilteredDevelopers.Clear();
            foreach (var x in items) FilteredDevelopers.Add(x);
        }

        public async Task AddDeveloperAsync(Developer item)
        {
            await _apiService.AddItemAsync("DevelopersController", item);
            _cacheService.Remove("developers");
            await LoadDevelopersAsync();
        }

        public async Task UpdateDeveloperAsync(int id, Developer item)
        {
            await _apiService.UpdateItemAsync("DevelopersController", id, item);
            _cacheService.Remove("developers");
            await LoadDevelopersAsync();
        }

        public async Task DeleteDeveloperAsync(int id)
        {
            await _apiService.DeleteItemAsync("DevelopersController", id);
            _cacheService.Remove("developers");
            await LoadDevelopersAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}