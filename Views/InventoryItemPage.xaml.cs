
using AdminUP.Models;
using AdminUP.ViewModels;
using AdminUP.Views.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AdminUP.Views
{
    public partial class InventoryItemPage : Page
    {
        private readonly InventoryItemPageViewModel _viewModel;
        private bool _loaded;

        public InventoryItemPage()
        {
            InitializeComponent();
            _viewModel = new InventoryItemPageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
            Loaded += Page_Loaded;
            AddBtn.Click += AddButton_Click;
            EditBtn.Click += EditButton_Click;
            DeleteBtn.Click += DeleteButton_Click;
            SearchBtn.Click += SearchButton_Click;
            ClearBtn.Click += ClearButton_Click;
            InventoryComboBox.SelectionChanged += InventoryComboBox_SelectionChanged;
            ItemsGrid.MouseDoubleClick += DataGrid_MouseDoubleClick;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_loaded) return;
            _loaded = true;
            try
            {
                await _viewModel.LoadDataAsync();
                InventoryComboBox.ItemsSource = _viewModel.InventoryList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки:\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new InventoryItem
            {
                checked_by_user_id = App.AuthService.CurrentUserId,
                checked_at = DateTime.Now
            };
            if (InventoryComboBox.SelectedValue is int invId && invId != 0)
                newItem.inventory_id = invId;
            ShowEditDialog(newItem, "Добавить позицию инвентаризации");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var row = _viewModel.SelectedInventoryItem;
            if (row == null)
            {
                MessageBox.Show("Выберите позицию для изменения.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ShowEditDialog(row.Item, "Изменить позицию инвентаризации");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var row = _viewModel.SelectedInventoryItem;
            if (row == null)
            {
                MessageBox.Show("Выберите позицию для удаления.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var res = MessageBox.Show(
                $"Удалить позицию «{row.EquipmentName}»?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;
            try
            {
                await _viewModel.DeleteInventoryItemAsync(row.id);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления:\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
            => _viewModel.FilterInventoryItems();

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
            _viewModel.FilterInventoryItems();
        }

        private void InventoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) return;
            _viewModel.SelectedInventoryId = InventoryComboBox.SelectedValue is int id && id != 0
                ? id : (int?)null;
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
            => EditButton_Click(sender, e);

        private async void ShowEditDialog(InventoryItem item, string title)
        {
            var control = new InventoryItemEditControl(item);
            var dialog = new EditDialog(control, title) { Owner = Window.GetWindow(this) };
            if (dialog.ShowDialog() == true)
            {
                var edited = control.GetInventoryItem();
                if (edited == null) return;
                try
                {
                    if (edited.id == 0)
                        await _viewModel.AddInventoryItemAsync(edited);
                    else
                        await _viewModel.UpdateInventoryItemAsync(edited.id, edited);
                    InventoryComboBox.ItemsSource = _viewModel.InventoryList;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения:\n" + ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
