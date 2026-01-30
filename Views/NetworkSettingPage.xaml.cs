using AdminUP.Models;
using AdminUP.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class NetworkSettingPage : Page
    {
        private readonly NetworkSettingPageViewModel _viewModel;

        public NetworkSettingPage()
        {
            InitializeComponent();
            _viewModel = new NetworkSettingPageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
            => await _viewModel.LoadDataAsync();

        private void AddButton_Click(object sender, RoutedEventArgs e)
            => ShowEditDialog(new NetworkSetting(), "Добавление сетевых настроек");

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedNetworkSetting == null)
            {
                MessageBox.Show("Выберите запись для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ShowEditDialog(_viewModel.SelectedNetworkSetting, "Редактирование сетевых настроек");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedNetworkSetting == null)
            {
                MessageBox.Show("Выберите запись для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            await _viewModel.DeleteNetworkSettingAsync(_viewModel.SelectedNetworkSetting.Id);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
            => _viewModel.FilterNetwork();

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
            => EditButton_Click(sender, e);

        private void CheckNetworkButton_Click(object sender, RoutedEventArgs e)
        {
            // Заглушка: можно потом сделать ping/проверку доступности.
            MessageBox.Show("Проверка сети пока не реализована.\n(Можно добавить Ping по IP)", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void ShowEditDialog(NetworkSetting item, string title)
        {
            var dlg = new EditDialog(item, title) { Owner = Window.GetWindow(this) };

            if (dlg.ShowDialog() == true && dlg.GetEditedItem() is NetworkSetting edited)
            {
                if (edited.Id == 0)
                    await _viewModel.AddNetworkSettingAsync(edited);
                else
                    await _viewModel.UpdateNetworkSettingAsync(edited.Id, edited);
            }
        }
    }
}
