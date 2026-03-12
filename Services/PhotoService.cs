using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AdminUP.Services
{
    /// <summary>
    /// Сервис выбора фотографий оборудования и расходников через диалоговое окно.
    /// Форматы: jpg, jpeg, png, gif, bmp (как указано в ТЗ).
    /// Файлы хранятся в папке Photos/ рядом с exe; в БД сохраняется путь.
    /// </summary>
    public class PhotoService
    {
        private readonly string _photosDir;

        public PhotoService()
        {
            _photosDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Photos");
            Directory.CreateDirectory(_photosDir);
        }

        /// <summary>Полный путь к папке с фотографиями.</summary>
        public string PhotosDirectory => _photosDir;

        // ── Выбор и сохранение ────────────────────────────────────────────────

        /// <summary>
        /// Открывает диалог выбора изображения (jpg/png/gif/bmp).
        /// Копирует файл в папку Photos/ с уникальным именем.
        /// Возвращает относительный путь (например Photos/eq_20240915_143022.jpg)
        /// или null если пользователь отменил.
        /// </summary>
        /// <param name="prefix">Префикс имени файла: "eq" для оборудования, "cons" для расходников.</param>
        public string? PickAndSave(string prefix = "photo")
        {
            var dlg = new OpenFileDialog
            {
                Title = "Выбрать фотографию",
                Filter = "Изображения (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp"
            };
            if (dlg.ShowDialog() != true) return null;

            try
            {
                var ext = Path.GetExtension(dlg.FileName).ToLowerInvariant();
                var fileName = $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}{ext}";
                var destPath = Path.Combine(_photosDir, fileName);
                File.Copy(dlg.FileName, destPath, overwrite: true);
                return $"Photos/{fileName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении фото:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        // ── Загрузка изображения ──────────────────────────────────────────────

        /// <summary>
        /// Загружает BitmapImage из сохранённого относительного или абсолютного пути.
        /// Возвращает null если файл не найден или повреждён.
        /// </summary>
        /// <param name="imagePath">Путь из поля image_path модели.</param>
        /// <param name="decodeWidth">Максимальная ширина в пикселях (для экономии памяти).</param>
        public BitmapImage? Load(string? imagePath, int decodeWidth = 300)
        {
            if (string.IsNullOrWhiteSpace(imagePath)) return null;

            var full = Path.IsPathRooted(imagePath)
                ? imagePath
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);

            if (!File.Exists(full)) return null;

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(full, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.DecodePixelWidth = decodeWidth;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch { return null; }
        }
    }
}
