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
    public class ClassroomPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<Classroom> _classroomList;
        private Classroom _selectedClassroom;
        private bool _isLoading;
        private string _searchText;

        public ObservableCollection<Classroom> ClassroomList
        {
            get => _classroomList;
            set
            {
                _classroomList = value;
                OnPropertyChanged();
            }
        }

        public Classroom SelectedClassroom
        {
            get => _selectedClassroom;
            set
            {
                _selectedClassroom = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsClassroomSelected));
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
                FilterClassrooms();
            }
        }

        public bool IsClassroomSelected => SelectedClassroom != null;

        public ObservableCollection<Classroom> FilteredClassroomList { get; set; }

        public ClassroomPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            ClassroomList = new ObservableCollection<Classroom>();
            FilteredClassroomList = new ObservableCollection<Classroom>();
        }

        public async Task LoadClassroomsAsync()
        {
            IsLoading = true;
            try
            {
                var classrooms = await _cacheService.GetOrSetAsync("classrooms_page_list",
                    async () => await _apiService.GetListAsync<Classroom>("ClassroomsController"));

                ClassroomList.Clear();
                if (classrooms != null)
                {
                    foreach (var item in classrooms)
                    {
                        ClassroomList.Add(item);
                    }
                }

                FilterClassrooms();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки аудиторий: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterClassrooms()
        {
            FilteredClassroomList.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in ClassroomList)
                {
                    FilteredClassroomList.Add(item);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = ClassroomList.Where(c =>
                    (c.Name?.ToLower().Contains(searchLower) ?? false) ||
                    (c.ShortName?.ToLower().Contains(searchLower) ?? false));

                foreach (var item in filtered)
                {
                    FilteredClassroomList.Add(item);
                }
            }
        }

        public async Task<bool> AddClassroomAsync(Classroom classroom)
        {
            try
            {
                var success = await _apiService.AddItemAsync("ClassroomsController", classroom);
                if (success)
                {
                    _cacheService.Remove("classrooms_page_list");
                    await LoadClassroomsAsync();
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

        public async Task<bool> UpdateClassroomAsync(int id, Classroom classroom)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("ClassroomsController", id, classroom);
                if (success)
                {
                    _cacheService.Remove("classrooms_page_list");
                    await LoadClassroomsAsync();
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

        public async Task<bool> DeleteClassroomAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту аудиторию?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("ClassroomsController", id);
                if (success)
                {
                    _cacheService.Remove("classrooms_page_list");
                    await LoadClassroomsAsync();
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