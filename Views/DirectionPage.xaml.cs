using AdminUP.Models;
using AdminUP.ViewModels;
using AdminUP.Views.Controls;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class DirectionPage : Page
    {
        private DirectionPageViewModel _viewModel;

        public DirectionPage()
        {
            InitializeComponent();

            _viewModel = new DirectionPageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDirectionsAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newDirection = new Direction();
            ShowEditDialog(newDirection, "Добавление направления");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedDirection != null)
            {
                ShowEditDialog(_viewModel.SelectedDirection, "Редактирование направления");
            }
            else
            {
                MessageBox.Show("Выберите направление для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedDirection != null)
            {
                await _viewModel.DeleteDirectionAsync(_viewModel.SelectedDirection.id);
            }
            else
            {
                MessageBox.Show("Выберите направление для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.FilterDirections();
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

        private async void ShowEditDialog(Direction direction, string title)
        {
            var control = new DirectionEditControl(direction);

            var editDialog = new EditDialog(control, title);
            editDialog.Owner = Window.GetWindow(this);

            if (editDialog.ShowDialog() == true)
            {
                var editedDirection = control.GetDirection();

                if (editedDirection != null)
                {
                    if (editedDirection.id == 0)
                        await _viewModel.AddDirectionAsync(editedDirection);
                    else
                        await _viewModel.UpdateDirectionAsync(editedDirection.id, editedDirection);
                }
            }
        }
    }
}
