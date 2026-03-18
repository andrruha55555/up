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
            => ShowEditDialog(new SoftwareEntity(), "Добавление ПО");

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var row = _viewModel.SelectedSoftware as SoftwareRow;
            if (row == null)
            {
                MessageBox.Show("Выберите ПО для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ShowEditDialog(row.Software, "Редактирование ПО");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var row = _viewModel.SelectedSoftware as SoftwareRow;
            if (row == null)
            {
                MessageBox.Show("Выберите ПО для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var confirm = MessageBox.Show("Вы уверены, что хотите удалить это ПО?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm == MessageBoxResult.Yes)
                await _viewModel.DeleteSoftwareAsync(row.Software.id);
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

        private async void ShowEditDialog(SoftwareEntity software, string title)
        {
            var control = new SoftwareEditControl(software);
            var dlg = new EditDialog(control, title) { Owner = Window.GetWindow(this) };

            if (dlg.ShowDialog() == true)
            {
                if (!control.Validate()) return;
                var edited = control.GetSoftware();
                if (edited.id == 0)
                    await _viewModel.AddSoftwareAsync(edited);
                else
                    await _viewModel.UpdateSoftwareAsync(edited.id, edited);
            }
        }
    }
}
