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
    public class ModelPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _api;
        private readonly CacheService _cache;

        public ObservableCollection<EquipmentModel> ModelList { get; } = new();
        public ObservableCollection<EquipmentModel> FilteredModelList { get; } = new();
        public ObservableCollection<EquipmentType> EquipmentTypeList { get; } = new();

        private EquipmentModel? _selectedModel;
        public EquipmentModel? SelectedModel
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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ModelPageViewModel(ApiService api, CacheService cache)
        {
            _api = api;
            _cache = cache;
        }

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                var types = await _cache.GetOrAddAsync("equipment_types", async () =>
                    await _api.GetListAsync<EquipmentType>("EquipmentTypesController"));

                EquipmentTypeList.Clear();
                foreach (var t in types) EquipmentTypeList.Add(t);

                var models = await _cache.GetOrAddAsync("models", async () =>
                    await _api.GetListAsync<EquipmentModel>("ModelsController"));

                ModelList.Clear();
                foreach (var m in models)
                {
                    m.EquipmentTypeName = EquipmentTypeList.FirstOrDefault(x => x.Id == m.EquipmentTypeId)?.Name ?? "";
                    ModelList.Add(m);
                }

                FilterModels();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterModels()
        {
            FilteredModelList.Clear();
            var q = (SearchText ?? "").Trim().ToLowerInvariant();

            var items = ModelList.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(q))
                items = items.Where(x =>
                    (x.Name ?? "").ToLowerInvariant().Contains(q) ||
                    (x.EquipmentTypeName ?? "").ToLowerInvariant().Contains(q));

            foreach (var i in items) FilteredModelList.Add(i);
        }

        public async Task<bool> AddModelAsync(EquipmentModel item)
        {
            var ok = await _api.AddItemAsync("ModelsController", item);
            _cache.Remove("models");
            await LoadDataAsync();
            return ok;
        }

        public async Task<bool> UpdateModelAsync(int id, EquipmentModel item)
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
