using AdminUP.Models;
using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.Services
{
    /// <summary>
    /// Сервис импорта оборудования из Excel-шаблона (Приложение 4).
    /// Структура строк: тип строки в столбце A:
    ///   "СОТРУДНИК" — ФИО в столбце B (задаёт responsible_user_id для строк ниже)
    ///   "КАТЕГОРИЯ" — название категории в столбце B (только для группировки, не импортируется)
    ///   ""          — строка оборудования (столбцы B–M)
    /// Данные начинаются со строки DATA_START_ROW.
    /// </summary>
    public class ImportService
    {
        private readonly ApiService _apiService;

        // Индексы столбцов (1-based)
        private const int ColType = 1;  // A — тип строки: СОТРУДНИК / КАТЕГОРИЯ / (пусто)
        private const int ColName = 2;  // B — Название / ФИО / Категория
        private const int ColInvNum = 3;  // C — Инвентарный номер *
        private const int ColModel = 4;  // D — Модель
        private const int ColEqType = 5;  // E — Тип оборудования
        private const int ColClassroom = 6;  // F — Аудитория
        private const int ColStatus = 7;  // G — Статус
        private const int ColCost = 8;  // H — Стоимость
        private const int ColDirection = 9;  // I — Направление
        private const int ColTempUser = 10; // J — Временно ответственный
        private const int ColDate = 11; // K — Дата
        private const int ColSoftware = 12; // L — ПО
        private const int ColComment = 13; // M — Комментарий

        // Строка, с которой начинаются данные в шаблоне
        // (после заголовков: 2 строки учреждения + 1 разделитель + 5 инструкций +
        //  1 пустая + 1 «пример» + 6 примеров + 1 разделитель + 1 заголовки = строка 19)
        private const int DataStartRow = 4;

        public ImportService(ApiService apiService)
        {
            _apiService = apiService;
        }

        // ─── Скачать шаблон ───────────────────────────────────────────────────

        /// <summary>Показывает диалог сохранения и копирует встроенный шаблон.</summary>
        public void DownloadTemplate()
        {
            var dlg = new SaveFileDialog
            {
                Title = "Сохранить шаблон импорта",
                FileName = "Шаблон_импорта_оборудования",
                DefaultExt = ".xlsx",
                Filter = "Excel файл (*.xlsx)|*.xlsx"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                var exeDir = AppDomain.CurrentDomain.BaseDirectory;
                var templatePath = Path.Combine(exeDir, "Resources", "ImportTemplate.xlsx");

                if (!File.Exists(templatePath))
                {
                    MessageBox.Show(
                        "Файл шаблона не найден в папке Resources/ImportTemplate.xlsx.\n" +
                        "Убедитесь что папка Resources скопирована рядом с exe.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                File.Copy(templatePath, dlg.FileName, overwrite: true);
                MessageBox.Show($"Шаблон сохранён:\n{dlg.FileName}",
                    "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении шаблона:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ─── Импорт из файла ─────────────────────────────────────────────────

        /// <summary>Открывает диалог и запускает импорт из выбранного файла.</summary>
        public async Task<(int added, int skipped, int errors, List<string> log)> ImportFromFileAsync()
        {
            var dlg = new OpenFileDialog
            {
                Title = "Выбрать файл для импорта",
                Filter = "Excel файл (*.xlsx;*.xls)|*.xlsx;*.xls"
            };
            if (dlg.ShowDialog() != true) return (0, 0, 0, new());
            return await ImportFileAsync(dlg.FileName);
        }

        /// <summary>Читает файл и импортирует строки оборудования.</summary>
        public async Task<(int added, int skipped, int errors, List<string> log)> ImportFileAsync(string filePath)
        {
            var log = new List<string>();
            int added = 0;
            int skipped = 0;
            int errors = 0;

            try
            {
                // Загрузка справочников
                var usersTask = _apiService.GetListAsync<User>("UsersController");
                var classroomsTask = _apiService.GetListAsync<Classroom>("ClassroomsController");
                var statusesTask = _apiService.GetListAsync<Status>("StatusesController");
                var modelsTask = _apiService.GetListAsync<ModelEntity>("ModelsController");
                var directionsTask = _apiService.GetListAsync<Direction>("DirectionsController");
                var softwareTask = _apiService.GetListAsync<SoftwareEntity>("SoftwareController");
                var existingTask = _apiService.GetListAsync<Equipment>("EquipmentController");

                await Task.WhenAll(usersTask, classroomsTask, statusesTask,
                    modelsTask, directionsTask, softwareTask, existingTask);

                var users = BuildDict(usersTask.Result, u => u.FullName?.Trim(), u => u.id);
                var classrooms = BuildDict(classroomsTask.Result, c => c.name?.Trim(), c => c.id);
                var statuses = BuildDict(statusesTask.Result, s => s.name?.Trim(), s => s.id);
                var models = BuildDict(modelsTask.Result, m => m.name?.Trim(), m => m.id);
                var directions = BuildDict(directionsTask.Result, d => d.name?.Trim(), d => d.id);
                var software = BuildDict(softwareTask.Result, sw => sw.name?.Trim(), sw => sw.id);
                var existing = (existingTask.Result ?? new())
                                    .Select(e => e.inventory_number?.Trim())
                                    .Where(n => n != null)
                                    .ToHashSet(StringComparer.OrdinalIgnoreCase)!;

                using var wb = new XLWorkbook(filePath);
                var ws = wb.Worksheet(1);
                int lastRow = ws.LastRowUsed()?.RowNumber() ?? DataStartRow;

                // Состояние контекста (меняется при строках СОТРУДНИК/КАТЕГОРИЯ)
                int? currentUserId = null;
                string currentUserName = "";

                for (int row = DataStartRow; row <= lastRow; row++)
                {
                    string rowType = Cell(ws, row, ColType).ToUpper().Trim();
                    string colB = Cell(ws, row, ColName);

                    // ── Строка СОТРУДНИК ──────────────────────────────────
                    if (rowType == "СОТРУДНИК")
                    {
                        if (string.IsNullOrWhiteSpace(colB))
                        {
                            log.Add($"Строка {row}: СОТРУДНИК без имени — пропущена.");
                            continue;
                        }
                        currentUserName = colB.Trim();
                        if (users.TryGetValue(currentUserName.ToLower(), out var uid))
                        {
                            currentUserId = uid;
                            log.Add($"Строка {row}: 👤 Сотрудник «{currentUserName}» (id={uid})");
                        }
                        else
                        {
                            currentUserId = null;
                            log.Add($"Строка {row}: ⚠️  Сотрудник «{currentUserName}» не найден в системе — ответственный не будет назначен.");
                        }
                        continue;
                    }

                    // ── Строка КАТЕГОРИЯ ──────────────────────────────────
                    if (rowType == "КАТЕГОРИЯ")
                    {
                        log.Add($"Строка {row}: 📁 Категория «{colB}»");
                        continue;
                    }

                    // ── Строка оборудования ───────────────────────────────
                    string invNum = Cell(ws, row, ColInvNum);
                    string name = colB;

                    // Пропуск пустых
                    if (string.IsNullOrWhiteSpace(invNum) && string.IsNullOrWhiteSpace(name))
                    {
                        skipped++;
                        continue;
                    }
                    if (string.IsNullOrWhiteSpace(invNum))
                    {
                        log.Add($"Строка {row}: ⚠️  Пропущена — нет инвентарного номера ({name})");
                        skipped++; continue;
                    }
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        log.Add($"Строка {row}: ⚠️  Пропущена — нет названия ({invNum})");
                        skipped++; continue;
                    }
                    if (existing.Contains(invNum))
                    {
                        log.Add($"Строка {row}: ⚠️  Пропущена — инв. номер «{invNum}» уже есть в БД");
                        skipped++; continue;
                    }

                    try
                    {
                        var eq = new Equipment
                        {
                            name = name,
                            inventory_number = invNum,
                            comment = Cell(ws, row, ColComment),
                        };

                        // Ответственный из контекста сотрудника
                        if (currentUserId.HasValue)
                            eq.responsible_user_id = currentUserId;

                        // Стоимость
                        var costStr = Cell(ws, row, ColCost).Replace(" ", "").Replace(",", ".");
                        if (decimal.TryParse(costStr,
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var cost))
                            eq.cost = cost;

                        // Дата
                        var dateStr = Cell(ws, row, ColDate);
                        if (DateTime.TryParseExact(dateStr, "dd.MM.yyyy",
                            System.Globalization.CultureInfo.InvariantCulture,
                            System.Globalization.DateTimeStyles.None, out var dt))
                            eq.created_at = dt;
                        else if (DateTime.TryParse(dateStr, out var dt2))
                            eq.created_at = dt2;

                        // Временно ответственный
                        var tempName = Cell(ws, row, ColTempUser);
                        if (!string.IsNullOrWhiteSpace(tempName) &&
                            users.TryGetValue(tempName.ToLower(), out var tuid))
                            eq.temp_responsible_user_id = tuid;

                        // Аудитория
                        var roomName = Cell(ws, row, ColClassroom);
                        if (!string.IsNullOrWhiteSpace(roomName) &&
                            classrooms.TryGetValue(roomName.ToLower(), out var rid))
                            eq.classroom_id = rid;

                        // Статус (обязателен)
                        var statusName = Cell(ws, row, ColStatus);
                        if (!string.IsNullOrWhiteSpace(statusName) &&
                            statuses.TryGetValue(statusName.ToLower(), out var sid))
                            eq.status_id = sid;
                        else
                            eq.status_id = statuses.Values.FirstOrDefault() > 0
                                ? statuses.Values.First() : 1;

                        // Направление
                        var dirName = Cell(ws, row, ColDirection);
                        if (!string.IsNullOrWhiteSpace(dirName) &&
                            directions.TryGetValue(dirName.ToLower(), out var did))
                            eq.direction_id = did;

                        // Модель
                        var modelName = Cell(ws, row, ColModel);
                        if (!string.IsNullOrWhiteSpace(modelName) &&
                            models.TryGetValue(modelName.ToLower(), out var mid))
                            eq.model_id = mid;

                        var ok = await _apiService.AddItemAsync("EquipmentController", eq);
                        if (!ok) throw new Exception("API вернул ошибку при добавлении.");

                        existing.Add(invNum);
                        added++;
                        log.Add($"Строка {row}: ✅ «{name}»  [{invNum}]  сотрудник: {currentUserName}");
                    }
                    catch (Exception ex)
                    {
                        log.Add($"Строка {row}: ❌ {ex.Message}");
                        errors++;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Add($"Критическая ошибка: {ex.Message}");
                errors++;
            }

            return (added, skipped, errors, log);
        }

        // ─── Утилиты ─────────────────────────────────────────────────────────

        private static string Cell(IXLWorksheet ws, int row, int col)
        {
            var cell = ws.Cell(row, col);
            if (cell.IsEmpty()) return "";
            // Handle numeric, date, boolean and text cells uniformly
            var val = cell.Value;
            if (val.IsDateTime)
                return val.GetDateTime().ToString("dd.MM.yyyy");
            if (val.IsNumber)
            {
                var num = val.GetNumber();
                // Return integer string if no fractional part (e.g. 123 not 123.0)
                return (num % 1 == 0)
                    ? ((long)num).ToString()
                    : num.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            if (val.IsText)
                return val.GetText()?.Trim() ?? "";
            if (val.IsBoolean)
                return val.GetBoolean().ToString();
            return cell.GetFormattedString()?.Trim() ?? "";
        }

        private static Dictionary<string, int> BuildDict<T>(
            List<T>? list, Func<T, string?> keySelector, Func<T, int> valueSelector)
        {
            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in list ?? new())
            {
                var key = keySelector(item);
                if (!string.IsNullOrWhiteSpace(key) && !dict.ContainsKey(key.ToLower()))
                    dict[key.ToLower()] = valueSelector(item);
            }
            return dict;
        }
    }
}
