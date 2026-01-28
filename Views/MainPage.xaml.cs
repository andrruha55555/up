using AdminUP.Models;
using AdminUP.ViewModels;
using AdminUP.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AdminUP
{
    public partial class MainPage : Window
    {
        private MainViewModel _viewModel;
        public MainPage()
        {
            InitializeComponent();
            _viewModel = App.MainViewModel;
            DataContext = _viewModel;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadAllDataAsync();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Подтверждение выхода
            var result = MessageBox.Show("Вы уверены, что хотите выйти?", "Подтверждение выхода",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadAllDataAsync();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            App.AuthService.Logout();

            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            object newItem = null;
            string title = "Добавление";

            // Определяем тип нового элемента в зависимости от активной вкладки
            switch (MainTabControl.SelectedIndex)
            {
                case 0: // Оборудование
                    newItem = new Equipment();
                    title = "Добавление оборудования";
                    break;
                case 1: // Пользователи
                    newItem = new User();
                    title = "Добавление пользователя";
                    break;
                case 2: // Аудитории
                    newItem = new Classroom();
                    title = "Добавление аудитории";
                    break;
                    // Добавить остальные случаи...
            }

            if (newItem != null)
            {
                ShowEditDialog(newItem, title);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            object selectedItem = GetSelectedItem();

            if (selectedItem != null)
            {
                string title = "Редактирование";

                // Определяем тип для заголовка
                if (selectedItem is Equipment) title = "Редактирование оборудования";
                else if (selectedItem is User) title = "Редактирование пользователя";
                else if (selectedItem is Classroom) title = "Редактирование аудитории";
                // Добавить остальные...

                ShowEditDialog(selectedItem, title);
            }
            else
            {
                MessageBox.Show("Выберите элемент для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItem = GetSelectedItem();

            if (selectedItem != null)
            {
                bool success = false;

                // Удаление в зависимости от типа
                if (selectedItem is Equipment equipment)
                {
                    success = await _viewModel.DeleteEquipmentAsync(equipment.Id);
                }
                else if (selectedItem is User user)
                {
                    // Реализовать удаление пользователя
                    MessageBox.Show("Удаление пользователей пока не реализовано", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                // Добавить остальные...

                if (success)
                {
                    await _viewModel.LoadAllDataAsync();
                }
            }
            else
            {
                MessageBox.Show("Выберите элемент для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            switch (MainTabControl.SelectedIndex)
            {
                case 0: // Оборудование
                    await _viewModel.ExportEquipmentToExcel();
                    break;
                case 1: // Пользователи
                    await _viewModel.ExportUsersToExcel();
                    break;
                    // Добавить остальные...
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Печать пока не реализована", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
            }
        }

        private void ClearSearchButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }

        private void EquipmentGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditButton_Click(sender, e);
        }

        private object GetSelectedItem()
        {
            switch (MainTabControl.SelectedIndex)
            {
                case 0: return EquipmentGrid.SelectedItem;
                case 1: return UserGrid.SelectedItem;
                // Добавить для остальных DataGrid
                default: return null;
            }
        }

        private void PerformSearch()
        {
            var searchText = _viewModel.SearchText;
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                switch (MainTabControl.SelectedIndex)
                {
                    case 0: // Оборудование
                        _viewModel.SearchEquipment(searchText);
                        break;
                        // Добавить для остальных коллекций
                }
            }
        }

        private async void ShowEditDialog(object item, string title)
        {
            var editDialog = new EditDialog(item, title);
            editDialog.Owner = this;

            if (editDialog.ShowDialog() == true)
            {
                var editedItem = editDialog.GetEditedItem();

                // Сохраняем изменения
                if (editedItem != null)
                {
                    bool success = false;

                    if (editedItem is Equipment equipment)
                    {
                        if (equipment.Id == 0)
                            success = await _viewModel.AddEquipmentAsync(equipment);
                        else
                            success = await _viewModel.UpdateEquipmentAsync(equipment);
                    }
                    // Добавить обработку других типов...

                    if (success)
                    {
                        await _viewModel.LoadAllDataAsync();
                    }
                }
            }
        }
    }
}