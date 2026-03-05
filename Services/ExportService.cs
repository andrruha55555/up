using AdminUP.Models;
using AdminUP.Services;
using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.Services
{
    public class ExportService
    {
        private readonly ApiService _apiService;

        public ExportService(ApiService apiService)
        {
            _apiService = apiService;
        }

        // ─── Главный метод экспорта ───────────────────────────────────────────
        public async Task ExportEquipmentReportAsync()
        {
            try
            {
                // 1. Загружаем все справочники параллельно
                var equipmentTask = _apiService.GetListAsync<Equipment>("EquipmentController");
                var classroomsTask = _apiService.GetListAsync<Classroom>("ClassroomsController");
                var usersTask = _apiService.GetListAsync<User>("UsersController");
                var statusesTask = _apiService.GetListAsync<Status>("StatusesController");
                var modelsTask = _apiService.GetListAsync<ModelEntity>("ModelsController");
                var directionsTask = _apiService.GetListAsync<Direction>("DirectionsController");
                var typesTask = _apiService.GetListAsync<EquipmentType>("EquipmentTypesController");
                var softwareTask = _apiService.GetListAsync<SoftwareEntity>("SoftwareController");
                var eqSoftwareTask = _apiService.GetListAsync<EquipmentSoftware>("EquipmentSoftwareController");
                var networkTask = _apiService.GetListAsync<NetworkSetting>("NetworkSettingsController");
                var historyTask = _apiService.GetListAsync<EquipmentHistory>("EquipmentHistoryController");
                var developersTask = _apiService.GetListAsync<Developer>("DevelopersController");

                await Task.WhenAll(equipmentTask, classroomsTask, usersTask, statusesTask,
                    modelsTask, directionsTask, typesTask, softwareTask, eqSoftwareTask,
                    networkTask, historyTask, developersTask);

                var equipment = equipmentTask.Result ?? new();
                var classrooms = classroomsTask.Result ?? new();
                var users = usersTask.Result ?? new();
                var statuses = statusesTask.Result ?? new();
                var models = modelsTask.Result ?? new();
                var directions = directionsTask.Result ?? new();
                var types = typesTask.Result ?? new();
                var software = softwareTask.Result ?? new();
                var eqSoftware = eqSoftwareTask.Result ?? new();
                var network = networkTask.Result ?? new();
                var history = historyTask.Result ?? new();
                var developers = developersTask.Result ?? new();

                // Словари для быстрого lookup
                var classroomMap = classrooms.ToDictionary(x => x.id, x => $"{x.name} ({x.short_name})");
                var userMap = users.ToDictionary(x => x.id, x => $"{x.last_name} {x.first_name} {x.middle_name}".Trim());
                var statusMap = statuses.ToDictionary(x => x.id, x => x.name);
                var modelMap = models.ToDictionary(x => x.id, x => x.name);
                var directionMap = directions.ToDictionary(x => x.id, x => x.name);
                var typeMap = types.ToDictionary(x => x.id, x => x.name);
                var softwareMap = software.ToDictionary(x => x.id, x => x.name);
                var developerMap = developers.ToDictionary(x => x.id, x => x.name);

                // Привязка ПО к оборудованию
                var eqSoftwareGroup = eqSoftware
                    .GroupBy(x => x.equipment_id)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.software_id).ToList());

                // 2. Диалог сохранения
                var saveDialog = new SaveFileDialog
                {
                    Title = "Сохранить отчёт по оборудованию",
                    FileName = $"Отчёт_оборудование_{DateTime.Now:yyyy-MM-dd}",
                    DefaultExt = ".xlsx",
                    Filter = "Excel файл (*.xlsx)|*.xlsx"
                };
                if (saveDialog.ShowDialog() != true) return;

                // 3. Создаём книгу
                using var wb = new XLWorkbook();

                BuildMainSheet(wb, equipment, classroomMap, userMap, statusMap,
                    modelMap, directionMap, typeMap, eqSoftwareGroup, softwareMap);

                BuildSoftwareSheet(wb, equipment, eqSoftwareGroup, softwareMap,
                    software, developerMap);

                BuildNetworkSheet(wb, equipment, network, classroomMap);

                BuildHistorySheet(wb, equipment, history, classroomMap, userMap);

                BuildSummarySheet(wb, equipment, statusMap, classroomMap, directionMap);

                wb.SaveAs(saveDialog.FileName);

                MessageBox.Show($"Отчёт успешно сохранён:\n{saveDialog.FileName}",
                    "Экспорт завершён", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ─── Лист 1: Основной список оборудования ────────────────────────────
        private void BuildMainSheet(
            XLWorkbook wb,
            List<Equipment> equipment,
            Dictionary<int, string> classroomMap,
            Dictionary<int, string> userMap,
            Dictionary<int, string> statusMap,
            Dictionary<int, string> modelMap,
            Dictionary<int, string> directionMap,
            Dictionary<int, string> typeMap,
            Dictionary<int, List<int>> eqSoftwareGroup,
            Dictionary<int, string> softwareMap)
        {
            var ws = wb.Worksheets.Add("Оборудование");

            // Заголовок отчёта
            ws.Cell(1, 1).Value = "ОТЧЁТ ПО ОБОРУДОВАНИЮ";
            ws.Range(1, 1, 1, 12).Merge();
            StyleReportTitle(ws.Cell(1, 1));

            ws.Cell(2, 1).Value = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";
            ws.Range(2, 1, 2, 12).Merge();
            ws.Cell(2, 1).Style.Font.Italic = true;
            ws.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;
            ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Заголовки колонок
            string[] headers = {
                "№", "Инв. номер", "Название", "Модель", "Тип",
                "Аудитория", "Ответственный", "Направление",
                "Статус", "Стоимость", "Установленное ПО", "Комментарий"
            };

            int headerRow = 4;
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(headerRow, i + 1);
                cell.Value = headers[i];
                StyleHeader(cell);
            }

            // Данные
            int row = headerRow + 1;
            int num = 1;
            foreach (var eq in equipment.OrderBy(e => e.name))
            {
                // Установленное ПО — перечислим через запятую
                string swList = "—";
                if (eqSoftwareGroup.TryGetValue(eq.id, out var swIds) && swIds.Count > 0)
                    swList = string.Join(", ", swIds
                        .Where(id => softwareMap.ContainsKey(id))
                        .Select(id => softwareMap[id]));

                // Тип через модель
                string typeName = "—";
                if (eq.model_id.HasValue && modelMap.TryGetValue(eq.model_id.Value, out _))
                {
                    var m = new ModelEntity(); // already have type from typeMap via model lookup
                    // We just show model name; type would need model->type relation
                    typeName = "—";
                }

                ws.Cell(row, 1).Value = num++;
                ws.Cell(row, 2).Value = eq.inventory_number ?? "—";
                ws.Cell(row, 3).Value = eq.name ?? "—";
                ws.Cell(row, 4).Value = eq.model_id.HasValue && modelMap.TryGetValue(eq.model_id.Value, out var mn) ? mn : "—";
                ws.Cell(row, 5).Value = typeName;
                ws.Cell(row, 6).Value = eq.classroom_id.HasValue && classroomMap.TryGetValue(eq.classroom_id.Value, out var cn) ? cn : "—";
                ws.Cell(row, 7).Value = eq.responsible_user_id.HasValue && userMap.TryGetValue(eq.responsible_user_id.Value, out var un) ? un : "—";
                ws.Cell(row, 8).Value = eq.direction_id.HasValue && directionMap.TryGetValue(eq.direction_id.Value, out var dn) ? dn : "—";
                ws.Cell(row, 9).Value = statusMap.TryGetValue(eq.status_id, out var sn) ? sn : "—";
                ws.Cell(row, 10).Value = eq.cost.HasValue ? (double)eq.cost.Value : 0;
                ws.Cell(row, 11).Value = swList;
                ws.Cell(row, 12).Value = eq.comment ?? "—";

                // Форматирование стоимости
                ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00 \"₽\"";

                // Зебра
                if (row % 2 == 0)
                    ws.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F8FF");

                // Цвет статуса
                var statusCell = ws.Cell(row, 9);
                var statusVal = statusCell.Value.ToString();
                if (statusVal.Contains("работ", StringComparison.OrdinalIgnoreCase) ||
                    statusVal.Contains("актив", StringComparison.OrdinalIgnoreCase))
                    statusCell.Style.Font.FontColor = XLColor.DarkGreen;
                else if (statusVal.Contains("ремонт", StringComparison.OrdinalIgnoreCase) ||
                         statusVal.Contains("неиспр", StringComparison.OrdinalIgnoreCase))
                    statusCell.Style.Font.FontColor = XLColor.DarkRed;
                else
                    statusCell.Style.Font.FontColor = XLColor.DarkOrange;

                row++;
            }

            // Итоговая строка
            ws.Cell(row, 1).Value = "ИТОГО:";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 9).Value = $"{equipment.Count} ед.";
            ws.Cell(row, 9).Style.Font.Bold = true;
            ws.Cell(row, 10).FormulaA1 = $"=SUM(J{headerRow + 1}:J{row - 1})";
            ws.Cell(row, 10).Style.Font.Bold = true;
            ws.Cell(row, 10).Style.NumberFormat.Format = "#,##0.00 \"₽\"";
            ws.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8EEFF");

            // Рамка таблицы
            var tableRange = ws.Range(headerRow, 1, row, headers.Length);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            tableRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#2B4A9F");
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#BDC8E8");

            // Ширина столбцов
            ws.Column(1).Width = 6;
            ws.Column(2).Width = 16;
            ws.Column(3).Width = 28;
            ws.Column(4).Width = 20;
            ws.Column(5).Width = 18;
            ws.Column(6).Width = 22;
            ws.Column(7).Width = 26;
            ws.Column(8).Width = 20;
            ws.Column(9).Width = 16;
            ws.Column(10).Width = 16;
            ws.Column(11).Width = 30;
            ws.Column(12).Width = 30;

            ws.Row(headerRow).Height = 30;
            ws.SheetView.FreezeRows(headerRow);
        }

        // ─── Лист 2: Установленное ПО ─────────────────────────────────────────
        private void BuildSoftwareSheet(
            XLWorkbook wb,
            List<Equipment> equipment,
            Dictionary<int, List<int>> eqSoftwareGroup,
            Dictionary<int, string> softwareMap,
            List<SoftwareEntity> software,
            Dictionary<int, string> developerMap)
        {
            var ws = wb.Worksheets.Add("Программное обеспечение");

            ws.Cell(1, 1).Value = "УСТАНОВЛЕННОЕ ПРОГРАММНОЕ ОБЕСПЕЧЕНИЕ";
            ws.Range(1, 1, 1, 6).Merge();
            StyleReportTitle(ws.Cell(1, 1));

            string[] headers = { "№", "Оборудование", "Инв. номер", "ПО", "Версия", "Разработчик" };
            int headerRow = 3;
            for (int i = 0; i < headers.Length; i++)
                StyleHeader(ws.Cell(headerRow, i + 1)).Value = headers[i];

            int row = headerRow + 1;
            int num = 1;

            var softwareDict = software.ToDictionary(s => s.id);

            foreach (var eq in equipment.OrderBy(e => e.name))
            {
                if (!eqSoftwareGroup.TryGetValue(eq.id, out var swIds) || swIds.Count == 0)
                    continue;

                foreach (var swId in swIds)
                {
                    if (!softwareDict.TryGetValue(swId, out var sw)) continue;

                    ws.Cell(row, 1).Value = num++;
                    ws.Cell(row, 2).Value = eq.name ?? "—";
                    ws.Cell(row, 3).Value = eq.inventory_number ?? "—";
                    ws.Cell(row, 4).Value = sw.name ?? "—";
                    ws.Cell(row, 5).Value = sw.version ?? "—";
                    ws.Cell(row, 6).Value = sw.developer_id.HasValue && developerMap.TryGetValue(sw.developer_id.Value, out var dev) ? dev : "—";

                    if (row % 2 == 0)
                        ws.Range(row, 1, row, 6).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F8FF");
                    row++;
                }
            }

            var tableRange = ws.Range(headerRow, 1, row - 1, 6);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            tableRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#2B4A9F");
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#BDC8E8");

            ws.Column(1).Width = 6;
            ws.Column(2).Width = 28;
            ws.Column(3).Width = 18;
            ws.Column(4).Width = 28;
            ws.Column(5).Width = 14;
            ws.Column(6).Width = 24;
            ws.Row(headerRow).Height = 30;
            ws.SheetView.FreezeRows(headerRow);
        }

        // ─── Лист 3: Сетевые настройки ───────────────────────────────────────
        private void BuildNetworkSheet(
            XLWorkbook wb,
            List<Equipment> equipment,
            List<NetworkSetting> network,
            Dictionary<int, string> classroomMap)
        {
            var ws = wb.Worksheets.Add("Сетевые настройки");

            ws.Cell(1, 1).Value = "СЕТЕВЫЕ НАСТРОЙКИ ОБОРУДОВАНИЯ";
            ws.Range(1, 1, 1, 9).Merge();
            StyleReportTitle(ws.Cell(1, 1));

            string[] headers = { "№", "Оборудование", "Инв. номер", "Аудитория", "IP-адрес", "Маска", "Шлюз", "DNS 1", "DNS 2" };
            int headerRow = 3;
            for (int i = 0; i < headers.Length; i++)
                StyleHeader(ws.Cell(headerRow, i + 1)).Value = headers[i];

            var eqMap = equipment.ToDictionary(e => e.id);
            var netByEq = network.GroupBy(n => n.equipment_id).ToDictionary(g => g.Key, g => g.First());

            int row = headerRow + 1;
            int num = 1;
            foreach (var eq in equipment.Where(e => netByEq.ContainsKey(e.id)).OrderBy(e => e.name))
            {
                var n = netByEq[eq.id];
                ws.Cell(row, 1).Value = num++;
                ws.Cell(row, 2).Value = eq.name ?? "—";
                ws.Cell(row, 3).Value = eq.inventory_number ?? "—";
                ws.Cell(row, 4).Value = eq.classroom_id.HasValue && classroomMap.TryGetValue(eq.classroom_id.Value, out var cn) ? cn : "—";
                ws.Cell(row, 5).Value = n.ip_address ?? "—";
                ws.Cell(row, 6).Value = n.subnet_mask ?? "—";
                ws.Cell(row, 7).Value = n.gateway ?? "—";
                ws.Cell(row, 8).Value = n.dns1 ?? "—";
                ws.Cell(row, 9).Value = n.dns2 ?? "—";

                // IP моноширинным
                ws.Cell(row, 5).Style.Font.FontName = "Courier New";
                ws.Cell(row, 6).Style.Font.FontName = "Courier New";
                ws.Cell(row, 7).Style.Font.FontName = "Courier New";

                if (row % 2 == 0)
                    ws.Range(row, 1, row, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F8FF");
                row++;
            }

            var tableRange = ws.Range(headerRow, 1, Math.Max(row - 1, headerRow), 9);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            tableRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#2B4A9F");
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#BDC8E8");

            int[] widths = { 6, 28, 18, 22, 16, 16, 16, 16, 16 };
            for (int i = 0; i < widths.Length; i++) ws.Column(i + 1).Width = widths[i];
            ws.Row(headerRow).Height = 30;
            ws.SheetView.FreezeRows(headerRow);
        }

        // ─── Лист 4: История перемещений ─────────────────────────────────────
        private void BuildHistorySheet(
            XLWorkbook wb,
            List<Equipment> equipment,
            List<EquipmentHistory> history,
            Dictionary<int, string> classroomMap,
            Dictionary<int, string> userMap)
        {
            var ws = wb.Worksheets.Add("История перемещений");

            ws.Cell(1, 1).Value = "ИСТОРИЯ ПЕРЕМЕЩЕНИЙ ОБОРУДОВАНИЯ";
            ws.Range(1, 1, 1, 7).Merge();
            StyleReportTitle(ws.Cell(1, 1));

            string[] headers = { "№", "Оборудование", "Инв. номер", "Аудитория", "Ответственный", "Дата изменения", "Комментарий" };
            int headerRow = 3;
            for (int i = 0; i < headers.Length; i++)
                StyleHeader(ws.Cell(headerRow, i + 1)).Value = headers[i];

            var eqMap = equipment.ToDictionary(e => e.id);

            int row = headerRow + 1;
            int num = 1;
            foreach (var h in history.OrderByDescending(h => h.changed_at))
            {
                eqMap.TryGetValue(h.equipment_id, out var eq);

                ws.Cell(row, 1).Value = num++;
                ws.Cell(row, 2).Value = eq?.name ?? $"ID {h.equipment_id}";
                ws.Cell(row, 3).Value = eq?.inventory_number ?? "—";
                ws.Cell(row, 4).Value = h.classroom_id.HasValue && classroomMap.TryGetValue(h.classroom_id.Value, out var cn) ? cn : "—";
                ws.Cell(row, 5).Value = h.responsible_user_id.HasValue && userMap.TryGetValue(h.responsible_user_id.Value, out var un) ? un : "—";
                ws.Cell(row, 6).Value = h.changed_at?.ToString("dd.MM.yyyy HH:mm") ?? "—";
                ws.Cell(row, 7).Value = h.comment ?? "—";

                if (row % 2 == 0)
                    ws.Range(row, 1, row, 7).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F8FF");
                row++;
            }

            var tableRange = ws.Range(headerRow, 1, Math.Max(row - 1, headerRow), 7);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            tableRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#2B4A9F");
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#BDC8E8");

            int[] widths = { 6, 28, 18, 22, 26, 18, 34 };
            for (int i = 0; i < widths.Length; i++) ws.Column(i + 1).Width = widths[i];
            ws.Row(headerRow).Height = 30;
            ws.SheetView.FreezeRows(headerRow);
        }

        // ─── Лист 5: Сводка по статусам и аудиториям ─────────────────────────
        private void BuildSummarySheet(
            XLWorkbook wb,
            List<Equipment> equipment,
            Dictionary<int, string> statusMap,
            Dictionary<int, string> classroomMap,
            Dictionary<int, string> directionMap)
        {
            var ws = wb.Worksheets.Add("Сводка");

            ws.Cell(1, 1).Value = "СВОДНАЯ СТАТИСТИКА";
            ws.Range(1, 1, 1, 4).Merge();
            StyleReportTitle(ws.Cell(1, 1));

            // Блок 1: По статусам
            ws.Cell(3, 1).Value = "По статусам";
            ws.Cell(3, 1).Style.Font.Bold = true;
            ws.Cell(3, 1).Style.Font.FontSize = 12;
            ws.Cell(3, 1).Style.Font.FontColor = XLColor.FromHtml("#2B4A9F");

            StyleHeader(ws.Cell(4, 1)).Value = "Статус";
            StyleHeader(ws.Cell(4, 2)).Value = "Количество";
            StyleHeader(ws.Cell(4, 3)).Value = "Сумма стоимости";

            int row = 5;
            var byStatus = equipment.GroupBy(e => e.status_id);
            foreach (var g in byStatus.OrderByDescending(g => g.Count()))
            {
                ws.Cell(row, 1).Value = statusMap.TryGetValue(g.Key, out var sn) ? sn : g.Key.ToString();
                ws.Cell(row, 2).Value = g.Count();
                ws.Cell(row, 3).Value = (double)(g.Sum(e => e.cost ?? 0));
                ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00 \"₽\"";
                if (row % 2 == 0)
                    ws.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F8FF");
                row++;
            }
            ws.Cell(row, 1).Value = "Итого";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = equipment.Count;
            ws.Cell(row, 2).Style.Font.Bold = true;
            ws.Cell(row, 3).Value = (double)(equipment.Sum(e => e.cost ?? 0));
            ws.Cell(row, 3).Style.Font.Bold = true;
            ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00 \"₽\"";

            var statusTable = ws.Range(4, 1, row, 3);
            statusTable.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            statusTable.Style.Border.OutsideBorderColor = XLColor.FromHtml("#2B4A9F");
            statusTable.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            statusTable.Style.Border.InsideBorderColor = XLColor.FromHtml("#BDC8E8");

            // Блок 2: По аудиториям
            row += 2;
            ws.Cell(row, 1).Value = "По аудиториям";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 12;
            ws.Cell(row, 1).Style.Font.FontColor = XLColor.FromHtml("#2B4A9F");
            row++;

            StyleHeader(ws.Cell(row, 1)).Value = "Аудитория";
            StyleHeader(ws.Cell(row, 2)).Value = "Количество";
            StyleHeader(ws.Cell(row, 3)).Value = "Сумма стоимости";
            int classHeaderRow = row;
            row++;

            var byClassroom = equipment.GroupBy(e => e.classroom_id);
            foreach (var g in byClassroom.OrderByDescending(g => g.Count()))
            {
                string label = g.Key.HasValue && classroomMap.TryGetValue(g.Key.Value, out var cn) ? cn : "Не назначено";
                ws.Cell(row, 1).Value = label;
                ws.Cell(row, 2).Value = g.Count();
                ws.Cell(row, 3).Value = (double)(g.Sum(e => e.cost ?? 0));
                ws.Cell(row, 3).Style.NumberFormat.Format = "#,##0.00 \"₽\"";
                if (row % 2 == 0)
                    ws.Range(row, 1, row, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#F5F8FF");
                row++;
            }

            var classTable = ws.Range(classHeaderRow, 1, row - 1, 3);
            classTable.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            classTable.Style.Border.OutsideBorderColor = XLColor.FromHtml("#2B4A9F");
            classTable.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            classTable.Style.Border.InsideBorderColor = XLColor.FromHtml("#BDC8E8");

            ws.Column(1).Width = 30;
            ws.Column(2).Width = 14;
            ws.Column(3).Width = 22;
        }

        // ─── Стили ───────────────────────────────────────────────────────────
        private static void StyleReportTitle(IXLCell cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = 14;
            cell.Style.Font.FontName = "Arial";
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2B4A9F");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.WorksheetRow().Height = 32;
        }

        private static IXLCell StyleHeader(IXLCell cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontName = "Arial";
            cell.Style.Font.FontSize = 10;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#4472C4");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Alignment.WrapText = true;
            return cell;
        }

        // ─── Старый универсальный метод (оставляем для совместимости) ────────
        public async Task<bool> ExportToExcel<T>(System.Collections.Generic.IEnumerable<T> data, string fileName, string sheetName = "Data")
        {
            try
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(sheetName);
                var properties = typeof(T).GetProperties();

                for (int i = 0; i < properties.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = properties[i].Name;
                    StyleHeader(cell);
                }

                int row = 2;
                foreach (var item in data)
                {
                    for (int i = 0; i < properties.Length; i++)
                        worksheet.Cell(row, i + 1).Value = properties[i].GetValue(item)?.ToString();
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                var saveDialog = new SaveFileDialog { FileName = fileName, DefaultExt = ".xlsx", Filter = "Excel files (*.xlsx)|*.xlsx" };
                if (saveDialog.ShowDialog() == true)
                {
                    workbook.SaveAs(saveDialog.FileName);
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return false;
        }
    }
}
