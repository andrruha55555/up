using AdminUP.Models;
using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.Services
{
    /// <summary>
    /// Генерация Excel-отчёта по сотрудникам (Приложение 4 ТЗ).
    /// Структура: Сотрудник (ФИО + кол-во) → Категория (101.XX) → список оборудования.
    /// Столбцы: № п/п | Основное средство | Инвентарный номер | Количество
    /// </summary>
    public class StaffReportService
    {
        private readonly ApiService _api;

        public StaffReportService(ApiService api)
        {
            _api = api;
        }

        /// <summary>Загружает данные, строит Excel и показывает SaveFileDialog.</summary>
        public async Task GenerateAsync()
        {
            try
            {
                // Параллельная загрузка
                var eqTask = _api.GetListAsync<Equipment>("EquipmentController");
                var usersTask = _api.GetListAsync<User>("UsersController");
                var modelsTask = _api.GetListAsync<ModelEntity>("ModelsController");
                var typesTask = _api.GetListAsync<EquipmentType>("EquipmentTypesController");

                await Task.WhenAll(eqTask, usersTask, modelsTask, typesTask);

                var equipment = eqTask.Result ?? new();
                var users = usersTask.Result ?? new();
                var models = modelsTask.Result ?? new();
                var types = typesTask.Result ?? new();

                // Маппинги
                var userMap = users.ToDictionary(u => u.id,
                    u => $"{u.last_name} {u.first_name} {u.middle_name}".Trim());
                var typeMap = types.ToDictionary(t => t.id, t => t.name ?? "");
                // modelId → typeName
                var modelType = models
                    .Where(m => typeMap.ContainsKey(m.equipment_type_id))
                    .ToDictionary(m => m.id, m => typeMap[m.equipment_type_id]);

                var dlg = new SaveFileDialog
                {
                    Title = "Сохранить отчёт по сотрудникам",
                    FileName = $"Отчёт_сотрудники_{DateTime.Now:yyyy-MM-dd}",
                    DefaultExt = ".xlsx",
                    Filter = "Excel файл (*.xlsx)|*.xlsx"
                };
                if (dlg.ShowDialog() != true) return;

                using var wb = new XLWorkbook();
                var ws = wb.Worksheets.Add("Отчёт по сотрудникам");

                // Цвета по ТЗ
                var BLUE = XLColor.FromHtml("#0060AC");
                var BLUE2 = XLColor.FromHtml("#4472C4");
                var BLUE_L = XLColor.FromHtml("#D6E4F7");
                var ZEBRA = XLColor.FromHtml("#EEF4FF");
                var RED = XLColor.FromHtml("#E41613");

                // ── Заголовок документа ──────────────────────────────────
                ws.Range(1, 1, 1, 4).Merge();
                ws.Cell(1, 1).Value = "КГАПОУ Пермский Авиационный техникум им. А.Д. Швецова";
                StyleTitle(ws.Cell(1, 1), BLUE, bold: true, sz: 12, white: true);
                ws.Row(1).Height = 24;

                ws.Range(2, 1, 2, 4).Merge();
                ws.Cell(2, 1).Value = "ВЕДОМОСТЬ УЧЁТА ОБОРУДОВАНИЯ ПО СОТРУДНИКАМ";
                StyleTitle(ws.Cell(2, 1), BLUE, bold: true, sz: 13, white: true);
                ws.Row(2).Height = 28;

                ws.Range(3, 1, 3, 4).Merge();
                ws.Cell(3, 1).Value = $"Дата формирования: {DateTime.Now:dd.MM.yyyy}";
                ws.Cell(3, 1).Style.Font.FontSize = 10;
                ws.Cell(3, 1).Style.Font.Italic = true;
                ws.Cell(3, 1).Style.Font.FontColor = XLColor.Gray;
                ws.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Row(3).Height = 16;

                // ── Заголовки столбцов (строка 5) ───────────────────────
                // Точно как в ТЗ: Сотрудник | Количество | № п/п | Основное средство | Инв. номер
                // Но у нас 4 столбца: №п/п | Основное средство | Инвентарный номер | Количество
                string[] hdrs = { "№ п/п", "Основное средство", "Инвентарный номер", "Количество" };
                int[] wds = { 8, 50, 22, 14 };

                for (int i = 0; i < hdrs.Length; i++)
                {
                    var c = ws.Cell(5, i + 1);
                    c.Value = hdrs[i];
                    c.Style.Font.Bold = true;
                    c.Style.Font.FontSize = 11;
                    c.Style.Font.FontColor = XLColor.White;
                    c.Style.Fill.BackgroundColor = BLUE;
                    c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    c.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    c.Style.Alignment.WrapText = true;
                    c.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                    c.Style.Border.OutsideBorderColor = XLColor.White;
                    ws.Column(i + 1).Width = wds[i];
                }
                ws.Row(5).Height = 28;

                int row = 6;

                // ── Группируем по ответственному ─────────────────────────
                var grouped = equipment
                    .GroupBy(e => e.responsible_user_id)
                    .OrderBy(g =>
                        g.Key.HasValue && userMap.TryGetValue(g.Key.Value, out var n) ? n : "я");

                foreach (var empGrp in grouped)
                {
                    string empName = empGrp.Key.HasValue
                        && userMap.TryGetValue(empGrp.Key.Value, out var en)
                        ? en : "Не назначен";
                    int empCount = empGrp.Count();

                    // ── Строка сотрудника ─────────────────────────────────
                    // "Иванов Иван Иванович            Количество: 13"
                    ws.Range(row, 1, row, 3).Merge();
                    ws.Cell(row, 1).Value = empName;
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    ws.Cell(row, 1).Style.Font.FontSize = 12;
                    ws.Cell(row, 1).Style.Font.FontColor = XLColor.White;
                    ws.Cell(row, 1).Style.Fill.BackgroundColor = BLUE2;
                    ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    ws.Cell(row, 1).Style.Alignment.Indent = 1;

                    ws.Cell(row, 4).Value = empCount;
                    ws.Cell(row, 4).Style.Font.Bold = true;
                    ws.Cell(row, 4).Style.Font.FontSize = 12;
                    ws.Cell(row, 4).Style.Font.FontColor = XLColor.White;
                    ws.Cell(row, 4).Style.Fill.BackgroundColor = BLUE2;
                    ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Row(row).Height = 24;
                    row++;

                    // ── Группируем по типу (категории) ───────────────────
                    var byType = empGrp
                        .GroupBy(e => e.model_id.HasValue
                            && modelType.ContainsKey(e.model_id.Value)
                            ? modelType[e.model_id.Value]
                            : "Прочее оборудование")
                        .OrderBy(g => g.Key);

                    int catIdx = 1;
                    foreach (var typeGrp in byType)
                    {
                        int catCount = typeGrp.Count();

                        // Строка категории: "101.34, Машины и оборудование – иное движимое имущество учреждения"
                        string catLabel = $"101.3{catIdx}, {typeGrp.Key} – иное движимое имущество учреждения";
                        ws.Range(row, 1, row, 4).Merge();
                        ws.Cell(row, 1).Value = catLabel;
                        ws.Cell(row, 1).Style.Font.Bold = true;
                        ws.Cell(row, 1).Style.Font.FontSize = 11;
                        ws.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml("#1F3864");
                        ws.Cell(row, 1).Style.Fill.BackgroundColor = BLUE_L;
                        ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        ws.Cell(row, 1).Style.Alignment.Indent = 1;
                        SetThinBorder(ws.Row(row), 4);
                        ws.Row(row).Height = 18;
                        row++;
                        catIdx++;

                        // Строки оборудования
                        int num = 1;
                        foreach (var eq in typeGrp.OrderBy(e => e.inventory_number))
                        {
                            var bg = (row % 2 == 0) ? ZEBRA : XLColor.White;
                            ws.Cell(row, 1).Value = num++;
                            ws.Cell(row, 2).Value = eq.name ?? "—";
                            ws.Cell(row, 3).Value = eq.inventory_number ?? "—";
                            ws.Cell(row, 4).Value = 1;

                            for (int c = 1; c <= 4; c++)
                            {
                                ws.Cell(row, c).Style.Fill.BackgroundColor = bg;
                                ws.Cell(row, c).Style.Font.FontSize = 11;
                                ws.Cell(row, c).Style.Alignment.Horizontal =
                                    c == 1 || c == 3 || c == 4
                                    ? XLAlignmentHorizontalValues.Center
                                    : XLAlignmentHorizontalValues.Left;
                                ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                ws.Cell(row, c).Style.Border.OutsideBorderColor = XLColor.FromHtml("#B8CCE4");
                            }
                            ws.Row(row).Height = 18;
                            row++;
                        }
                    }
                }

                ws.SheetView.FreezeRows(5);
                ws.SheetView.FreezeColumns(0);
                wb.SaveAs(dlg.FileName);

                MessageBox.Show($"Отчёт сохранён:\n{dlg.FileName}",
                    "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчёта:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Хелперы стилизации ────────────────────────────────────────────────

        private static void StyleTitle(IXLCell c, XLColor bg,
            bool bold = false, int sz = 11, bool white = false)
        {
            c.Style.Font.Bold = bold;
            c.Style.Font.FontSize = sz;
            c.Style.Font.FontColor = white ? XLColor.White : XLColor.Black;
            c.Style.Fill.BackgroundColor = bg;
            c.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            c.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        }

        private static void SetThinBorder(IXLRow row, int cols)
        {
            for (int c = 1; c <= cols; c++)
            {
                row.Cell(c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                row.Cell(c).Style.Border.OutsideBorderColor = XLColor.FromHtml("#B8CCE4");
            }
        }
    }
}
