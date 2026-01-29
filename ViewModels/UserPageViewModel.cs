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
    public class UserPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly CacheService _cacheService;

        private ObservableCollection<User> _userList;
        private User _selectedUser;
        private bool _isLoading;
        private string _searchText;

        public ObservableCollection<User> UserList
        {
            get => _userList;
            set
            {
                _userList = value;
                OnPropertyChanged();
            }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsUserSelected));
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
                FilterUsers();
            }
        }

        public bool IsUserSelected => SelectedUser != null;

        public ObservableCollection<User> FilteredUserList { get; set; }

        public UserPageViewModel(ApiService apiService, CacheService cacheService)
        {
            _apiService = apiService;
            _cacheService = cacheService;

            UserList = new ObservableCollection<User>();
            FilteredUserList = new ObservableCollection<User>();
        }

        public async Task LoadUsersAsync()
        {
            IsLoading = true;
            try
            {
                var users = await _cacheService.GetOrSetAsync("users_page_list",
                    async () => await _apiService.GetListAsync<User>("UsersController"));

                UserList.Clear();
                if (users != null)
                {
                    foreach (var user in users)
                    {
                        UserList.Add(user);
                    }
                }

                FilterUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void FilterUsers()
        {
            FilteredUserList.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var user in UserList)
                {
                    FilteredUserList.Add(user);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = UserList.Where(u =>
                    (u.Login?.ToLower().Contains(searchLower) ?? false) ||
                    (u.LastName?.ToLower().Contains(searchLower) ?? false) ||
                    (u.FirstName?.ToLower().Contains(searchLower) ?? false) ||
                    (u.Email?.ToLower().Contains(searchLower) ?? false));

                foreach (var user in filtered)
                {
                    FilteredUserList.Add(user);
                }
            }
        }

        public async Task<bool> AddUserAsync(User user)
        {
            try
            {
                var success = await _apiService.AddItemAsync("UsersController", user);
                if (success)
                {
                    _cacheService.Remove("users_page_list");
                    await LoadUsersAsync();
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

        public async Task<bool> UpdateUserAsync(int id, User user)
        {
            try
            {
                var success = await _apiService.UpdateItemAsync("UsersController", id, user);
                if (success)
                {
                    _cacheService.Remove("users_page_list");
                    await LoadUsersAsync();
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

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого пользователя?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return false;

                var success = await _apiService.DeleteItemAsync("UsersController", id);
                if (success)
                {
                    _cacheService.Remove("users_page_list");
                    await LoadUsersAsync();
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