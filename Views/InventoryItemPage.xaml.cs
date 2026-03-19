
using AdminUP.Models;
using AdminUP.ViewModels;
using AdminUP.Views.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
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
            AddBtn.Click += ConductInventory_Click;    // "Провести инвентаризацию"
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

                // Кнопка "Удалить" только для admin
                DeleteBtn.Visibility = App.AuthService.IsAdmin
                    ? System.Windows.Visibility.Visible
                    : System.Windows.Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки:\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Провести инвентаризацию — открывается список запланированного оборудования
        /// с чекбоксами. Сотрудник/Преподаватель отмечает что нашёл.
        /// </summary>
        private async void ConductInventory_Click(object sender, RoutedEventArgs e)
        {
            if (!(InventoryComboBox.SelectedValue is int invId) || invId == 0)
            {
                MessageBox.Show("Выберите инвентаризацию из списка.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Получаем только оборудование которое запланировано для этой инвентаризации
                var allEquipment = await App.ApiService.GetListAsync<Equipment>("EquipmentController")
                                   ?? new List<Equipment>();

                var existingItems = _viewModel.InventoryItemList
                    .Where(i => i.inventory_id == invId)
                    .ToList();

                if (existingItems.Count == 0)
                {
                    MessageBox.Show("В данной инвентаризации нет оборудования.\n" +
                                    "Администратор должен сначала добавить оборудование в план инвентаризации.",
                        "Нет оборудования", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Для не-admin — только оборудование за которое они ответственны
                bool isAdmin = App.AuthService.IsAdmin;
                int myId = App.AuthService.CurrentUserId;

                List<Equipment> equipmentForThisInventory;
                if (isAdmin)
                {
                    var ids = existingItems.Select(i => i.equipment_id).ToHashSet();
                    equipmentForThisInventory = allEquipment.Where(eq => ids.Contains(eq.id)).ToList();
                }
                else
                {
                    var ids = existingItems.Select(i => i.equipment_id).ToHashSet();
                    equipmentForThisInventory = allEquipment
                        .Where(eq => ids.Contains(eq.id) &&
                               (eq.responsible_user_id == myId || eq.temp_responsible_user_id == myId))
                        .ToList();

                    if (equipmentForThisInventory.Count == 0)
                    {
                        MessageBox.Show("В данной инвентаризации нет оборудования закреплённого за вами.",
                            "Нет доступного оборудования", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }

                var invName = _viewModel.InventoryList.FirstOrDefault(i => i.id == invId)?.name ?? "";

                var control = new InventoryItemEditControl(
                    inventoryId: invId,
                    checkedByUserId: myId,
                    allEquipment: equipmentForThisInventory,
                    existingItems: existingItems
                );

                var dlg = new EditDialog(control, $"Провести инвентаризацию: {invName}")
                {
                    Owner = Window.GetWindow(this),
                    Width = 680,
                    Height = 560
                };

                if (dlg.ShowDialog() == true)
                    await SaveInventoryChanges(control);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task SaveInventoryChanges(InventoryItemEditControl control)
        {
            var (toAdd, toUpdate, toDelete) = control.GetChanges();
            int saved = 0;

            foreach (var item in toAdd)
            {
                await App.ApiService.AddItemAsync("InventoryItemsController", item);
                saved++;
            }
            foreach (var item in toUpdate)
            {
                await App.ApiService.UpdateItemAsync("InventoryItemsController", item.id, item);
                saved++;
            }
            // Снятие галочки не удаляет запись — просто обнуляем checked_by_user_id
            foreach (var item in toDelete)
            {
                // Вместо удаления — обновляем запись убирая проверку
                await App.ApiService.UpdateItemAsync("InventoryItemsController", item.id, item);
            }

            App.CacheService.Remove("inventory_items_page_list");
            await _viewModel.LoadDataAsync();
            InventoryComboBox.ItemsSource = _viewModel.InventoryList;

            MessageBox.Show($"Сохранено: {saved + toDelete.Count} позиций.",
                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var row = _viewModel.SelectedInventoryItem;
            if (row == null)
            {
                MessageBox.Show("Выберите позицию.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            // Открываем как при проведении инвентаризации
            ConductInventory_Click(sender, e);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var row = _viewModel.SelectedInventoryItem;
            if (row == null)
            {
                MessageBox.Show("Выберите позицию.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var res = MessageBox.Show($"Удалить «{row.EquipmentName}» из инвентаризации?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (res != MessageBoxResult.Yes) return;
            await _viewModel.DeleteInventoryItemAsync(row.id);
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
            => ConductInventory_Click(sender, e);
    }
}
