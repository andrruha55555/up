using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Collections.Generic;
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
        private ClassroomRow _selectedClassroom;
        private bool _isLoading;
        private string _searchText;
        private Dictionary<int, string> _userNames = new();

        public ObservableCollection<Classroom> ClassroomList
        {
            get => _classroomList;
            set
            {
                _classroomList = value;
                OnPropertyChanged();
            }
        }

        public ClassroomRow SelectedClassroom
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

        public ObservableCollection<ClassroomRow> FilteredClassroomList { get; set; }
        private List<ClassroomRow> _allRows = new();

        public ClassroomPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            ClassroomList = new ObservableCollection<Classroom>();
            FilteredClassroomList = new ObservableCollection<ClassroomRow>();
        }

        public async Task LoadClassroomsAsync()
        {
            IsLoading = true;
            try
            {
                var usersTask = _apiService.GetListAsync<User>("UsersController");
                var classroomsTask = _cacheService.GetOrSetAsync("classrooms_page_list",
                    async () => await _apiService.GetListAsync<Classroom>("ClassroomsController"));

                await Task.WhenAll(usersTask, classroomsTask);

                _userNames = (usersTask.Result ?? new())
                    .ToDictionary(u => u.id, u => u.FullName);

                var classrooms = classroomsTask.Result;
                ClassroomList.Clear();
                _allRows.Clear();

                if (classrooms != null)
                {
                    foreach (var item in classrooms)
                    {
                        ClassroomList.Add(item);
                        _allRows.Add(new ClassroomRow(item, _userNames));
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

            IEnumerable<ClassroomRow> source = _allRows;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var s = SearchText.ToLower();
                source = source.Where(r =>
                    (r.Classroom.name?.ToLower().Contains(s) ?? false) ||
                    (r.Classroom.short_name?.ToLower().Contains(s) ?? false) ||
                    (r.ResponsibleUser?.ToLower().Contains(s) ?? false));
            }

            foreach (var row in source)
                FilteredClassroomList.Add(row);
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

    public class ClassroomRow
    {
        public Classroom Classroom { get; }
        public int Id => Classroom.id;
        public string Name => Classroom.name;
        public string ShortName => Classroom.short_name;
        public string ResponsibleUser { get; }
        public string TempResponsibleUser { get; }

        public ClassroomRow(Classroom c, Dictionary<int, string> users)
        {
            Classroom = c;
            ResponsibleUser = c.responsible_user_id.HasValue && users.TryGetValue(c.responsible_user_id.Value, out var u) ? u : "—";
            TempResponsibleUser = c.temp_responsible_user_id.HasValue && users.TryGetValue(c.temp_responsible_user_id.Value, out var t) ? t : "—";
        }
    }
}
