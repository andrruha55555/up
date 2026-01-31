using AdminUP.Models;
using AdminUP.ViewModels;
using AdminUP.Views.Controls;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class EquipmentHistoryPage : Page
    {
        private EquipmentHistoryPageViewModel _viewModel;

        public EquipmentHistoryPage()
        {
            InitializeComponent();

            _viewModel = new EquipmentHistoryPageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;

            // Привязка ComboBox
            EquipmentComboBox.ItemsSource = _viewModel.EquipmentList;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadDataAsync();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newHistory = new EquipmentHistory();
            ShowEditDialog(newHistory, "Добавление записи истории");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedHistory != null)
            {
                ShowEditDialog(_viewModel.SelectedHistory, "Редактирование записи истории");
            }
            else
            {
                MessageBox.Show("Выберите запись истории для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedHistory != null)
            {
                await _viewModel.DeleteHistoryAsync(_viewModel.SelectedHistory.id);
            }
            else
            {
                MessageBox.Show("Выберите запись истории для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.FilterHistory();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }

        private void EquipmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EquipmentComboBox.SelectedValue is int selectedId)
            {
                _viewModel.SelectedEquipmentId = selectedId;
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            EditButton_Click(sender, e);
        }

        private async void ShowEditDialog(EquipmentHistory history, string title)
        {
            var control = new EquipmentHistoryEditControl(history);

            var editDialog = new EditDialog(control, title);
            editDialog.Owner = Window.GetWindow(this);

            if (editDialog.ShowDialog() == true)
            {
                var editedHistory = control.GetEquipmentHistory();

                if (editedHistory != null)
                {
                    if (editedHistory.id == 0)
                        await _viewModel.AddHistoryAsync(editedHistory);
                    else
                        await _viewModel.UpdateHistoryAsync(editedHistory.id, editedHistory);
                }
            }
        }
    }
}
