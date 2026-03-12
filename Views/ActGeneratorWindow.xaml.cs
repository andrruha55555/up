using AdminUP.Models;
using AdminUP.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdminUP.Views
{
    // ── Вспомогательные display-классы для ComboBox ──────────────────────────

    /// <summary>Обёртка оборудования для отображения в ComboBox.</summary>
    public class EquipmentDisplay
    {
        public Equipment Equipment { get; set; } = null!;
        /// <summary>Инв.номер — Название (для поиска и отображения)</summary>
        public string DisplayName => $"{Equipment.inventory_number}  —  {Equipment.name}";
    }

    /// <summary>Обёртка расходника для отображения в ComboBox.</summary>
    public class ConsumableDisplay
    {
        public Consumable Consumable { get; set; } = null!;
        /// <summary>Название (кол-во: N)</summary>
        public string DisplayName => $"{Consumable.name}  (кол-во: {Consumable.quantity})";
    }

    /// <summary>Обёртка пользователя: ФИО одной строкой.</summary>
    public class UserDisplay
    {
        public User User { get; set; } = null!;
        public string FullName =>
            $"{User.last_name} {User.first_name} {User.middle_name}".Trim();
    }

    /// <summary>
    /// Шторка выбора данных для генерации акта приёма-передачи.
    /// Поддерживает три вида акта (Приложения 1–3 ТЗ).
    /// </summary>
    public partial class ActGeneratorWindow : Window
    {
        private readonly ApiService _api;
        private readonly DocumentService _docSvc;

        public ActGeneratorWindow(ApiService api)
        {
            InitializeComponent();
            _api = api;
            _docSvc = new DocumentService();

            DateBox.Text = DateTime.Now.ToString("dd.MM.yyyy");
            _ = LoadDataAsync();
        }

        // ── Загрузка справочников ───────────────────────────────────────────

        private async System.Threading.Tasks.Task LoadDataAsync()
        {
            GenerateBtn.IsEnabled = false;

            var eqTask = _api.GetListAsync<Equipment>("EquipmentController");
            var consTask = _api.GetListAsync<Consumable>("ConsumablesController");
            var usersTask = _api.GetListAsync<User>("UsersController");

            await System.Threading.Tasks.Task.WhenAll(eqTask, consTask, usersTask);

            EquipmentCombo.ItemsSource = (eqTask.Result ?? new())
                .Select(e => new EquipmentDisplay { Equipment = e }).ToList();
            ConsumableCombo.ItemsSource = (consTask.Result ?? new())
                .Select(c => new ConsumableDisplay { Consumable = c }).ToList();
            EmployeeCombo.ItemsSource = (usersTask.Result ?? new())
                .Select(u => new UserDisplay { User = u }).ToList();

            GenerateBtn.IsEnabled = true;
        }

        // ── Переключение типа акта ──────────────────────────────────────────

        private void ActTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActTypeCombo.SelectedItem is not ComboBoxItem item) return;
            bool isConsumable = item.Tag?.ToString() == "2";
            EquipmentPanel.Visibility = isConsumable ? Visibility.Collapsed : Visibility.Visible;
            ConsumablePanel.Visibility = isConsumable ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── Автозаполнение полей при выборе оборудования ────────────────────

        private void EquipmentCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EquipmentCombo.SelectedItem is not EquipmentDisplay ed) return;
            InvNumBox.Text = ed.Equipment.inventory_number ?? "";
            CostBox.Text = ed.Equipment.cost?.ToString("F2") ?? "";
        }

        // ── Автозаполнение полей при выборе расходника ──────────────────────

        private void ConsumableCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConsumableCombo.SelectedItem is not ConsumableDisplay cd) return;
            QtyBox.Text = cd.Consumable.quantity.ToString();
        }

        // ── Генерация акта ──────────────────────────────────────────────────

        private async void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            // Обязательная проверка: сотрудник
            if (EmployeeCombo.SelectedItem is not UserDisplay ud)
            {
                MessageBox.Show("Выберите сотрудника (получателя).", "Проверка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string employee = ud.FullName;
            string date = string.IsNullOrWhiteSpace(DateBox.Text)
                ? DateTime.Now.ToString("dd.MM.yyyy")
                : DateBox.Text.Trim();

            string actTag = (ActTypeCombo.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "1";

            GenerateBtn.IsEnabled = false;

            try
            {
                switch (actTag)
                {
                    case "1":
                        if (EquipmentCombo.SelectedItem is not EquipmentDisplay ed1)
                        {
                            Warn("Выберите оборудование."); return;
                        }
                        await _docSvc.GenerateAct1Async(
                            employee, date,
                            ed1.Equipment.name ?? "",
                            SerialBox.Text.Trim(),
                            InvNumBox.Text.Trim(),
                            CostBox.Text.Trim());
                        break;

                    case "2":
                        if (ConsumableCombo.SelectedItem is not ConsumableDisplay cd2)
                        {
                            Warn("Выберите расходный материал."); return;
                        }
                        await _docSvc.GenerateAct2Async(
                            employee, date,
                            cd2.Consumable.name ?? "",
                            QtyBox.Text.Trim(),
                            ConsCostBox.Text.Trim());
                        break;

                    case "3":
                        if (EquipmentCombo.SelectedItem is not EquipmentDisplay ed3)
                        {
                            Warn("Выберите оборудование."); return;
                        }
                        await _docSvc.GenerateAct3Async(
                            employee, date,
                            ed3.Equipment.name ?? "",
                            SerialBox.Text.Trim(),
                            InvNumBox.Text.Trim(),
                            CostBox.Text.Trim());
                        break;
                }
            }
            finally
            {
                GenerateBtn.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e) => Close();

        private static void Warn(string msg) =>
            MessageBox.Show(msg, "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
