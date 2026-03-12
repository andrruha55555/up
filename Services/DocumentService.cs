using Microsoft.Win32;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.Services
{
    /// <summary>
    /// Генерирует акты приёма-передачи из docx-шаблонов (Приложения 1–3 ТЗ).
    /// Логика: читает word/document.xml из шаблона, заменяет {{placeholder}},
    /// сохраняет новый .docx через SaveFileDialog.
    /// </summary>
    public class DocumentService
    {
        private readonly string _templatesDir;

        public DocumentService()
        {
            _templatesDir = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Resources", "ActTemplates");
        }

        // ── Приложение 1: оборудование на временное пользование ─────────────
        /// <summary>
        /// Акт приёма-передачи оборудования на временное пользование.
        /// Плейсхолдеры: {{DATE}}, {{EMPLOYEE_SHORT}}, {{EQ_NAME}}, {{EQ_SERIAL}}, {{EQ_COST}}
        /// </summary>
        public Task GenerateAct1Async(
            string employee, string date,
            string eqName, string eqSerial, string eqInv, string eqCost)
        {
            return Generate("Act1_TempUse.docx",
                $"Акт1_временное_пользование_{DateTime.Now:yyyyMMdd}", new[]
            {
                ("{{DATE}}",           date),
                ("{{EMPLOYEE_SHORT}}", ToShort(employee)),
                ("{{EQ_NAME}}",        eqName),
                ("{{EQ_SERIAL}}",      string.IsNullOrWhiteSpace(eqSerial) ? eqInv : eqSerial),
                ("{{EQ_COST}}",        eqCost),
            });
        }

        // ── Приложение 2: расходные материалы ───────────────────────────────
        /// <summary>
        /// Акт приёма-передачи расходных материалов.
        /// Плейсхолдеры: {{DATE}}, {{EMPLOYEE_SHORT}}, {{CONS_NAME}}, {{CONS_QTY}}, {{CONS_COST}}
        /// </summary>
        public Task GenerateAct2Async(
            string employee, string date,
            string consName, string consQty, string consCost)
        {
            return Generate("Act2_Consumables.docx",
                $"Акт2_расходные_материалы_{DateTime.Now:yyyyMMdd}", new[]
            {
                ("{{DATE}}",           date),
                ("{{EMPLOYEE_SHORT}}", ToShort(employee)),
                ("{{CONS_NAME}}",      consName),
                ("{{CONS_QTY}}",       consQty),
                ("{{CONS_COST}}",      consCost),
            });
        }

        // ── Приложение 3: передача оборудования ─────────────────────────────
        /// <summary>
        /// Акт приёма-передачи оборудования.
        /// Плейсхолдеры: {{DATE}}, {{EMPLOYEE_SHORT}}, {{EQ_NAME}}, {{EQ_SERIAL}}, {{EQ_COST}}
        /// </summary>
        public Task GenerateAct3Async(
            string employee, string date,
            string eqName, string eqSerial, string eqInv, string eqCost)
        {
            return Generate("Act3_Equipment.docx",
                $"Акт3_передача_оборудования_{DateTime.Now:yyyyMMdd}", new[]
            {
                ("{{DATE}}",           date),
                ("{{EMPLOYEE_SHORT}}", ToShort(employee)),
                ("{{EQ_NAME}}",        eqName),
                ("{{EQ_SERIAL}}",      string.IsNullOrWhiteSpace(eqSerial) ? eqInv : eqSerial),
                ("{{EQ_COST}}",        eqCost),
            });
        }

        // ── Общая логика ─────────────────────────────────────────────────────

        /// <summary>
        /// Открывает шаблон, заменяет плейсхолдеры в document.xml,
        /// показывает SaveFileDialog и сохраняет итоговый .docx.
        /// </summary>
        private async Task Generate(
            string templateFile, string suggestedName,
            (string key, string value)[] replacements)
        {
            var templatePath = Path.Combine(_templatesDir, templateFile);
            if (!File.Exists(templatePath))
            {
                MessageBox.Show(
                    $"Шаблон не найден:\n{templatePath}\n\n" +
                    "Убедитесь что папка Resources/ActTemplates скопирована рядом с exe.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var dlg = new SaveFileDialog
            {
                Title = "Сохранить акт",
                FileName = suggestedName,
                DefaultExt = ".docx",
                Filter = "Word документ (*.docx)|*.docx"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                // Временная копия шаблона
                var tmp = Path.GetTempFileName() + ".docx";
                File.Copy(templatePath, tmp, overwrite: true);

                // Патчим word/document.xml внутри ZIP
                using (var zip = ZipFile.Open(tmp, ZipArchiveMode.Update))
                {
                    var entry = zip.GetEntry("word/document.xml")
                               ?? zip.GetEntry("word\\document.xml")
                               ?? throw new Exception("word/document.xml не найден в шаблоне");

                    string xml;
                    using (var sr = new StreamReader(entry.Open(), Encoding.UTF8))
                        xml = await sr.ReadToEndAsync();

                    foreach (var (key, value) in replacements)
                        xml = xml.Replace(key, XmlEsc(value ?? ""));

                    entry.Delete();
                    using var sw = new StreamWriter(zip.CreateEntry("word/document.xml").Open(), Encoding.UTF8);
                    await sw.WriteAsync(xml);
                }

                File.Copy(tmp, dlg.FileName, overwrite: true);
                File.Delete(tmp);

                if (MessageBox.Show(
                    $"Акт сохранён:\n{dlg.FileName}\n\nОткрыть файл?",
                    "Готово", MessageBoxButton.YesNo, MessageBoxImage.Information)
                    == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(
                        new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = dlg.FileName,
                            UseShellExecute = true
                        });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации акта:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Утилиты ───────────────────────────────────────────────────────────

        /// <summary>«Иванов Иван Иванович» → «Иванов И.И.»</summary>
        private static string ToShort(string full)
        {
            if (string.IsNullOrWhiteSpace(full)) return full;
            var p = full.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder(p[0]);
            for (int i = 1; i < p.Length; i++)
                if (p[i].Length > 0) sb.Append($" {p[i][0]}.");
            return sb.ToString();
        }

        /// <summary>Экранирует строку для вставки в XML.</summary>
        private static string XmlEsc(string s) =>
            s.Replace("&", "&amp;").Replace("<", "&lt;")
             .Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
    }
}
