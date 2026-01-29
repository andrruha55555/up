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
    public class EquipmentHistoryPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<EquipmentHistory> _historyList;
        private EquipmentHistory _selectedHistory;
        private bool _isLoading;
        private string _searchText;
        private int? _selectedEquipmentId;

        public ObservableCollection<EquipmentHistory> HistoryList
        {
            get => _historyList;
            set
            {
                _historyList = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Equipment> EquipmentList { get; set; }

        public EquipmentHistory SelectedHistory
        {
            get => _selectedHistory;
            set
            {
                _selectedHistory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsHistorySelected));
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
                FilterHistory();
            }
        }

        public int? SelectedEquipmentId
        {
            get => _selectedEquipmentId;
            set
            {
                _selectedEquipmentId = value;
                OnPropertyChanged();
                FilterHistory();
            }
        }

        public bool IsHistorySelected => SelectedHistory != null;

        public ObservableCollection<EquipmentHistory> FilteredHistoryList { get; set; }

        public EquipmentHistoryPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            HistoryList = new ObservableCollection<EquipmentHistory>();
            EquipmentList = new ObservableCollection<Equipment>();
            FilteredHistoryList = new ObservableCollection<EquipmentHistory>();
        }

        public async Task LoadDataAsync()
        {
            IsLoading = true;
            try
            {
                await Task.WhenAll(
                    LoadHistoryAsync(),
                    LoadEquipmentAsync()
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadHistoryAsync()
        {
            var history = await _cacheService.GetOrSetAsync("equipment_history_page_list",
                async () => await _apiService.GetListAsync<EquipmentHistory>("EquipmentHistoryController"));

            HistoryList.Clear();
            if (history != null)
            {
                foreach (var item in history)
                {
                    HistoryList.Add(item);
                }
            }

            FilterHistory();
        }

        private async Task LoadEquipmentAsync()
        {
            var equipment = await _cacheService.GetOrSetAsync("equipment_for_history",
                async () => await _apiService.GetListAsync<Equipment>("EquipmentController"));

            EquipmentList.Clear();
            if (equipment != null)
            {
                foreach (var item in equipment)
                {
                    EquipmentList.Add(item);
                }
            }
        }

        public void FilterHistory()
        {
            FilteredHistoryList.Clear();

            IEnumerable<EquipmentHistory> filtered = HistoryList;

            if (SelectedEquipmentId.HasValue && SelectedEquipmentId > 0)
            {
                filtered = filtered.Where(h => h.EquipmentId == SelectedEquipmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(h =>
                    (h.Comment?.ToLower().Contains(searchLower) ?? false));
            }

            foreach (var item in filtered)
            {
                FilteredHistoryList.Add(item);
            }
        }

        public async Task<bool> AddHistoryAsync(EquipmentHistory history)
        {
            try
            {
                var success = await _apiService.AddItemAsync("EquipmentHistoryController", history);
                if (success)
                {
                    _cacheService.Remove("equipment_history_page_list");
                    await LoadHistoryAsync();
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

        public async Task<bool> UpdateHistoryAsync(int id, EquipmentHistory history)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("EquipmentHistoryController", id, history);
                if (success)
                {
                    _cacheService.Remove("equipment_history_page_list");
                    await LoadHistoryAsync();
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

        public async Task<bool> DeleteHistoryAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту запись истории?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("EquipmentHistoryController", id);
                if (success)
                {
                    _cacheService.Remove("equipment_history_page_list");
                    await LoadHistoryAsync();
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