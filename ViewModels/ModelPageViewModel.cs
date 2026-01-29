using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.ViewModels
{
    public class ModelPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<Model> _modelList;
        private Model _selectedModel;
        private bool _isLoading;
        private string _searchText;

        public ObservableCollection<Model> ModelList
        {
            get => _modelList;
            set
            {
                _modelList = value;
                OnPropertyChanged();
            }
        }

        public Model SelectedModel
        {
            get => _selectedModel;
            set
            {
                _selectedModel = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsModelSelected));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterModels();
            }
        }

        public bool IsModelSelected => SelectedModel != null;

        public ObservableCollection<Model> FilteredModelList { get; set; }

        public ModelPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            ModelList = new ObservableCollection<Model>();
            FilteredModelList = new ObservableCollection<Model>();
        }

        public async Task LoadModelsAsync()
        {
            IsLoading = true;
            try
            {
                var models = await _cacheService.GetOrSetAsync("models_page_list",
                    async () => await _apiService.GetListAsync<Model>("ModelsController"));

                ModelList.Clear();
                if (models != null)
                {
                    foreach (var item in models)
                    {
                        ModelList.Add(item);
                    }
                }

                FilterModels();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки моделей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterModels()
        {
            FilteredModelList.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in ModelList)
                {
                    FilteredModelList.Add(item);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = ModelList.Where(m =>
                    (m.Name?.ToLower().Contains(searchLower) ?? false));

                foreach (var item in filtered)
                {
                    FilteredModelList.Add(item);
                }
            }
        }

        public async Task<bool> AddModelAsync(Model model)
        {
            try
            {
                var success = await _apiService.AddItemAsync("ModelsController", model);
                if (success)
                {
                    _cacheService.Remove("models_page_list");
                    await LoadModelsAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> UpdateModelAsync(int id, Model model)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("ModelsController", id, model);
                if (success)
                {
                    _cacheService.Remove("models_page_list");
                    await LoadModelsAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<bool> DeleteModelAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту модель?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("ModelsController", id);
                if (success)
                {
                    _cacheService.Remove("models_page_list");
                    await LoadModelsAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}