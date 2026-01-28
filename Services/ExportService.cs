using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace AdminUP.Services
{
    public class ExportService
    {
        public async Task<bool> ExportToExcel<T>(IEnumerable<T> data, string fileName, string sheetName = "Data")
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add(sheetName);

                    // Создаем заголовки
                    var properties = typeof(T).GetProperties();
                    for (int i = 0; i < properties.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = properties[i].Name;
                    }

                    // Заполняем данные
                    int row = 2;
                    foreach (var item in data)
                    {
                        for (int i = 0; i < properties.Length; i++)
                        {
                            var value = properties[i].GetValue(item);
                            worksheet.Cell(row, i + 1).Value = value?.ToString();
                        }
                        row++;
                    }

                    // Автоматическая ширина столбцов
                    worksheet.Columns().AdjustToContents();

                    // Сохраняем файл
                    var saveDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        FileName = fileName,
                        DefaultExt = ".xlsx",
                        Filter = "Excel files (*.xlsx)|*.xlsx"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        workbook.SaveAs(saveDialog.FileName);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return false;
        }

        public DataTable ToDataTable<T>(IEnumerable<T> items)
        {
            var dataTable = new DataTable(typeof(T).Name);

            // Создаем столбцы
            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            // Заполняем строки
            foreach (var item in items)
            {
                var values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(item, null) ?? DBNull.Value;
                }
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public async Task<bool> ExportToPdf<T>(IEnumerable<T> data, string fileName)
        {
            // Для экспорта в PDF можно использовать библиотеку iTextSharp или аналогичную
            // Здесь реализация будет зависеть от выбранной библиотеки
            MessageBox.Show("Экспорт в PDF пока не реализован", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }
    }
}
