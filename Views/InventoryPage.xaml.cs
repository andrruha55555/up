
using AdminUP.Models;
using AdminUP.ViewModels;
using AdminUP.Views.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    public partial class InventoryPage : Page
    {
        private readonly InventoryPageViewModel _viewModel;

        public InventoryPage()
        {
            InitializeComponent();
            _viewModel = new InventoryPageViewModel(App.ApiService, App.CacheService);
            DataContext = _viewModel;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
            => await _viewModel.LoadInventoriesAsync();

        private void AddButton_Click(object sender, RoutedEventArgs e)
            => ShowEditDialog(new Inventory(), "Создать инвентаризацию");

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedInventory == null)
            {
                MessageBox.Show("Выберите инвентаризацию.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            ShowEditDialog(_viewModel.SelectedInventory, "Редактировать инвентаризацию");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedInventory == null)
            {
                MessageBox.Show("Выберите инвентаризацию.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var res = MessageBox.Show(
                $"Удалить инвентаризацию «{_viewModel.SelectedInventory.name}»?\nВсе позиции также будут удалены.",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res != MessageBoxResult.Yes) return;
            await _viewModel.DeleteInventoryAsync(_viewModel.SelectedInventory.id);
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
            => _viewModel.FilterInventories();

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
        }

        private void DataGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
            => EditButton_Click(sender, e);

        private async void ShowEditDialog(Inventory item, string title)
        {
            var control = new InventoryEditControl(item);
            var dlg = new EditDialog(control, title)
            {
                Owner = Window.GetWindow(this),
                Width = 660,
                Height = 580
            };

            if (dlg.ShowDialog() != true) return;

            var edited = control.GetInventory();
            var selectedIds = control.GetSelectedEquipmentIds();
            if (edited == null) return;

            try
            {
                bool isNew = edited.id == 0;

                if (isNew)
                    await _viewModel.AddInventoryAsync(edited);
                else
                    await _viewModel.UpdateInventoryAsync(edited.id, edited);

                // Получаем актуальный id (после Add возвращается объект с id)
                int invId = edited.id;

                // Загружаем текущие позиции этой инвентаризации
                var currentItems = await App.ApiService.GetListAsync<InventoryItem>("InventoryItemsController")
                                   ?? new System.Collections.Generic.List<InventoryItem>();
                var existingForThis = currentItems.FindAll(i => i.inventory_id == invId);
                var existingIds = new System.Collections.Generic.HashSet<int>(
                    existingForThis.ConvertAll(i => i.equipment_id));

                // Добавляем новые позиции (без отметки проверки — просто план)
                foreach (var eqId in selectedIds)
                {
                    if (!existingIds.Contains(eqId))
                    {
                        await App.ApiService.AddItemAsync("InventoryItemsController", new InventoryItem
                        {
                            inventory_id = invId,
                            equipment_id = eqId
                            // checked_by_user_id = NULL (ещё не проверено)
                        });
                    }
                }

                // Удаляем снятые позиции (только если ещё не проверены)
                foreach (var existing in existingForThis)
                {
                    if (!selectedIds.Contains(existing.equipment_id) &&
                        !existing.checked_by_user_id.HasValue)
                    {
                        await App.ApiService.DeleteItemAsync("InventoryItemsController", existing.id);
                    }
                }

                App.CacheService.Remove("inventories");
                App.CacheService.Remove("inventory_items_page_list");
                App.CacheService.Remove("inventories_page_list");
                await _viewModel.LoadInventoriesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
