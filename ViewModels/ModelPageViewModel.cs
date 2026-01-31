using AdminUP.Models;
using AdminUP.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AdminUP.ViewModels
{
    public class ModelPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _api;
        private readonly CacheService _cache;

        public ObservableCollection<ModelEntity> ModelList { get; } = new();
        public ObservableCollection<ModelEntity> FilteredModelList { get; } = new();
        public ObservableCollection<EquipmentType> EquipmentTypeList { get; } = new();

        private ModelEntity? _selectedModel;
        public ModelEntity? SelectedModel
        {
            get => _selectedModel;
            set { _selectedModel = value; OnPropertyChanged(); }
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); FilterModels(); }
        }

        public ModelPageViewModel(ApiService api, CacheService cache)
        {
            _api = api;
            _cache = cache;
        }

        public async Task LoadDataAsync()
        {
            var types = await _cache.GetOrSetAsync("equipment_types",
                async () => await _api.GetListAsync<EquipmentType>("EquipmentTypesController"));

            EquipmentTypeList.Clear();
            if (types != null)
                foreach (var t in types) EquipmentTypeList.Add(t);

            var models = await _cache.GetOrSetAsync("models",
                async () => await _api.GetListAsync<ModelEntity>("ModelsController"));

            ModelList.Clear();
            if (models != null)
                foreach (var m in models) ModelList.Add(m);

            FilterModels();
        }

        public void FilterModels()
        {
            FilteredModelList.Clear();
            var q = (SearchText ?? "").Trim().ToLowerInvariant();

            var items = ModelList.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
                items = items.Where(x => (x.name ?? "").ToLowerInvariant().Contains(q));

            foreach (var i in items) FilteredModelList.Add(i);
        }

        public async Task<bool> AddModelAsync(ModelEntity item)
        {
            var ok = await _api.AddItemAsync("ModelsController", item);
            _cache.Remove("models");
            await LoadDataAsync();
            return ok;
        }

        public async Task<bool> UpdateModelAsync(int id, ModelEntity item)
        {
            var ok = await _api.UpdateItemAsync("ModelsController", id, item);
            _cache.Remove("models");
            await LoadDataAsync();
            return ok;
        }

        public async Task<bool> DeleteModelAsync(int id)
        {
            var ok = await _api.DeleteItemAsync("ModelsController", id);
            _cache.Remove("models");
            await LoadDataAsync();
            return ok;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
