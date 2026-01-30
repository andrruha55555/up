using AdminUP.Models;
using AdminUP.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class ModelPage : Page
    {
        private readonly ModelPageViewModel _viewModel;

        public ModelPage()
        {
            InitializeComponent();
            _viewModel = new ModelPageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
            => await _viewModel.LoadDataAsync();

        private void AddButton_Click(object sender, RoutedEventArgs e)
            => ShowEditDialog(new EquipmentModel(), "Добавление модели");

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedModel == null)
            {
                MessageBox.Show("Выберите модель для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ShowEditDialog(_viewModel.SelectedModel, "Редактирование модели");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedModel == null)
            {
                MessageBox.Show("Выберите модель для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await _viewModel.DeleteModelAsync(_viewModel.SelectedModel.Id);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
            => _viewModel.FilterModels();

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
            => EditButton_Click(sender, e);

        private async void ShowEditDialog(EquipmentModel model, string title)
        {
            var dlg = new EditDialog(model, title) { Owner = Window.GetWindow(this) };

            if (dlg.ShowDialog() == true && dlg.GetEditedItem() is EquipmentModel edited)
            {
                if (edited.Id == 0)
                    await _viewModel.AddModelAsync(edited);
                else
                    await _viewModel.UpdateModelAsync(edited.Id, edited);
            }
        }
    }
}
