using AdminUP.Models;
using AdminUP.ViewModels;
using AdminUP.Views.Controls;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class SoftwarePage : Page
    {
        private readonly SoftwarePageViewModel _viewModel;

        public SoftwarePage()
        {
            InitializeComponent();
            _viewModel = new SoftwarePageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
            => await _viewModel.LoadDataAsync();

        private void AddButton_Click(object sender, RoutedEventArgs e)
            => ShowEditDialog(new Software(), "Добавление ПО");

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedSoftware == null)
            {
                MessageBox.Show("Выберите ПО для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ShowEditDialog(_viewModel.SelectedSoftware, "Редактирование ПО");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedSoftware == null)
            {
                MessageBox.Show("Выберите ПО для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await _viewModel.DeleteSoftwareAsync(_viewModel.SelectedSoftware.Id);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
            => _viewModel.FilterSoftware();

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
            => EditButton_Click(sender, e);

        private async void ShowEditDialog(Software software, string title)
        {
            var control = new SoftwareEditControl(software);

            var dlg = new EditDialog(control, title)
            {
                Owner = Window.GetWindow(this)
            };

            if (dlg.ShowDialog() == true)
            {
                var edited = control.GetSoftware();

                if (edited.Id == 0)
                    await _viewModel.AddSoftwareAsync(edited);
                else
                    await _viewModel.UpdateSoftwareAsync(edited.Id, edited);
            }
        }
    }
}
