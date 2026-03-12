using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    // ── Вспомогательные display-классы для ComboBox ──────────────────────────

    public class EquipmentDisplay
    {
        public Equipment Equipment { get; set; } = null!;
        public string DisplayName => $"{Equipment.inventory_number}  —  {Equipment.name}";
    }

    public class ConsumableDisplay
    {
        public Consumable Consumable { get; set; } = null!;
        public string DisplayName => $"{Consumable.name}  (кол-во: {Consumable.quantity})";
    }

    public class UserDisplay
    {
        public User User { get; set; } = null!;
        public string FullName =>
            $"{User.last_name} {User.first_name} {User.middle_name}".Trim();
    }

    public partial class ActGeneratorWindow : Window
    {
        private readonly ApiService _api;
        private readonly DocumentService _docSvc;

        // ✅ Флаг: окно полностью инициализировано
        private bool _isLoaded;

        public ActGeneratorWindow(ApiService api)
        {
            InitializeComponent();
            _api = api;
            _docSvc = new DocumentService();

            // ✅ Безопасно ставить после InitializeComponent
            if (DateBox != null)
                DateBox.Text = DateTime.Now.ToString("dd.MM.yyyy");

            _isLoaded = true;

            // Применяем начальное состояние панелей вручную после загрузки
            UpdatePanelsVisibility();

            _ = LoadDataAsync();
        }

        // ── Загрузка справочников ───────────────────────────────────────────

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            // ✅ Null-guard: кнопка может не существовать
            if (GenerateBtn != null)
                GenerateBtn.IsEnabled = false;

            try
            {
                var eqTask = _api.GetListAsync<Equipment>("EquipmentController");
                var consTask = _api.GetListAsync<Consumable>("ConsumablesController");
                var usersTask = _api.GetListAsync<User>("UsersController");

                await System.Threading.Tasks.Task.WhenAll(eqTask, consTask, usersTask);

                if (EquipmentCombo != null)
                    EquipmentCombo.ItemsSource = (eqTask.Result ?? [])
                        .Select(e => new EquipmentDisplay { Equipment = e }).ToList();

                if (ConsumableCombo != null)
                    ConsumableCombo.ItemsSource = (consTask.Result ?? [])
                        .Select(c => new ConsumableDisplay { Consumable = c }).ToList();

                if (EmployeeCombo != null)
                    EmployeeCombo.ItemsSource = (usersTask.Result ?? [])
                        .Select(u => new UserDisplay { User = u }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (GenerateBtn != null)
                    GenerateBtn.IsEnabled = true;
            }
        }

        // ── Переключение типа акта ──────────────────────────────────────────

        private void ActTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ✅ ИСПРАВЛЕНИЕ: не выполнять до полной инициализации окна
            if (!_isLoaded) return;
            UpdatePanelsVisibility();
        }

        /// <summary>Обновляет видимость панелей в зависимости от выбранного типа акта.</summary>
        private void UpdatePanelsVisibility()
        {
            // ✅ Null-guards: панели могут не существовать при вызове из конструктора
            if (EquipmentPanel == null || ConsumablePanel == null) return;

            bool isConsumable =
                (ActTypeCombo?.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "2";

            EquipmentPanel.Visibility = isConsumable ? Visibility.Collapsed : Visibility.Visible;
            ConsumablePanel.Visibility = isConsumable ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── Автозаполнение при выборе оборудования ──────────────────────────

        private void EquipmentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EquipmentCombo?.SelectedItem is not EquipmentDisplay ed) return;

            if (InvNumBox != null)
                InvNumBox.Text = ed.Equipment.inventory_number ?? "";

            if (CostBox != null)
                CostBox.Text = ed.Equipment.cost?.ToString("F2") ?? "";
        }

        // ── Автозаполнение при выборе расходника ────────────────────────────

        private void ConsumableCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConsumableCombo?.SelectedItem is not ConsumableDisplay cd) return;

            if (QtyBox != null)
                QtyBox.Text = cd.Consumable.quantity.ToString();
        }

        // ── Генерация акта ──────────────────────────────────────────────────

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeeCombo?.SelectedItem is not UserDisplay ud)
            {
                Warn("Выберите сотрудника (получателя).");
                return;
            }

            string employee = ud.FullName;
            string date = string.IsNullOrWhiteSpace(DateBox?.Text)
                ? DateTime.Now.ToString("dd.MM.yyyy")
                : DateBox!.Text.Trim();

            string actTag = (ActTypeCombo?.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "1";

            if (GenerateBtn != null)
                GenerateBtn.IsEnabled = false;

            try
            {
                switch (actTag)
                {
                    case "1":
                        if (EquipmentCombo?.SelectedItem is not EquipmentDisplay ed1)
                        { Warn("Выберите оборудование."); return; }

                        await _docSvc.GenerateAct1Async(
                            employee, date,
                            ed1.Equipment.name ?? "",
                            SerialBox?.Text.Trim() ?? "",
                            InvNumBox?.Text.Trim() ?? "",
                            CostBox?.Text.Trim() ?? "");
                        break;

                    case "2":
                        if (ConsumableCombo?.SelectedItem is not ConsumableDisplay cd2)
                        { Warn("Выберите расходный материал."); return; }

                        await _docSvc.GenerateAct2Async(
                            employee, date,
                            cd2.Consumable.name ?? "",
                            QtyBox?.Text.Trim() ?? "",
                            ConsCostBox?.Text.Trim() ?? "");
                        break;

                    case "3":
                        if (EquipmentCombo?.SelectedItem is not EquipmentDisplay ed3)
                        { Warn("Выберите оборудование."); return; }

                        await _docSvc.GenerateAct3Async(
                            employee, date,
                            ed3.Equipment.name ?? "",
                            SerialBox?.Text.Trim() ?? "",
                            InvNumBox?.Text.Trim() ?? "",
                            CostBox?.Text.Trim() ?? "");
                        break;

                    default:
                        Warn("Выберите тип акта.");
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации акта:\n{ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (GenerateBtn != null)
                    GenerateBtn.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();

        private static void Warn(string msg) =>
            MessageBox.Show(msg, "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}