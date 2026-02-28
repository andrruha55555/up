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

                if (_viewModel.SelectedInventoryId.HasValue)
                    InventoryComboBox.SelectedValue = _viewModel.SelectedInventoryId.Value;

                _viewModel.FilterInventoryItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки страницы InventoryItem:\n\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var newItem = new InventoryItem();

            if (InventoryComboBox.SelectedValue is int invId && invId != 0)
                newItem.inventory_id = invId;

            ShowEditDialog(newItem, "Добавление элемента инвентаризации");
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedInventoryItem == null)
            {
                MessageBox.Show("Выберите элемент инвентаризации для редактирования", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ShowEditDialog(_viewModel.SelectedInventoryItem, "Редактирование элемента инвентаризации");
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedInventoryItem == null)
            {
                MessageBox.Show("Выберите элемент инвентаризации для удаления", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var res = MessageBox.Show("Удалить выбранный элемент?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (res != MessageBoxResult.Yes) return;

            try
            {
                await _viewModel.DeleteInventoryItemAsync(_viewModel.SelectedInventoryItem.id);
                await _viewModel.LoadDataAsync();
                _viewModel.FilterInventoryItems();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления:\n\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.FilterInventoryItems();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            _viewModel.SearchText = string.Empty;
            _viewModel.FilterInventoryItems();
        }

        private void InventoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loaded) return;

            if (InventoryComboBox.SelectedValue is int selectedId && selectedId != 0)
                _viewModel.SelectedInventoryId = selectedId;
            else
                _viewModel.SelectedInventoryId = null;

            _viewModel.FilterInventoryItems();
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditButton_Click(sender, e);
        }

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

                    await _viewModel.LoadDataAsync();
                    InventoryComboBox.ItemsSource = _viewModel.InventoryList;
                    _viewModel.FilterInventoryItems();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка сохранения:\n\n" + ex.Message,
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}