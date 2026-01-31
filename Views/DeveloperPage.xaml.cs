using AdminUP.Models;
using AdminUP.ViewModels;
using AdminUP.Views.Controls;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class DeveloperPage : Page
    {
        private DeveloperPageViewModel _viewModel;

        public DeveloperPage()
        {
            InitializeComponent();

            _viewModel = new DeveloperPageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDevelopersAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newDeveloper = new Developer();
            ShowEditDialog(newDeveloper, "Добавление разработчика");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedDeveloper != null)
            {
                ShowEditDialog(_viewModel.SelectedDeveloper, "Редактирование разработчика");
            }
            else
            {
                MessageBox.Show("Выберите разработчика для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedDeveloper != null)
            {
                await _viewModel.DeleteDeveloperAsync(_viewModel.SelectedDeveloper.id);
            }
            else
            {
                MessageBox.Show("Выберите разработчика для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.FilterDevelopers();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditButton_Click(sender, e);
        }

        private async void ShowEditDialog(Developer developer, string title)
        {
            var control = new DeveloperEditControl(developer);

            var editDialog = new EditDialog(control, title);
            editDialog.Owner = Window.GetWindow(this);

            if (editDialog.ShowDialog() == true)
            {
                var editedDeveloper = control.GetDeveloper();
                if (editedDeveloper != null)
                {
                    if (editedDeveloper.id == 0)
                        await _viewModel.AddDeveloperAsync(editedDeveloper);
                    else
                        await _viewModel.UpdateDeveloperAsync(editedDeveloper.id, editedDeveloper);
                }
            }
        }
    }
}
