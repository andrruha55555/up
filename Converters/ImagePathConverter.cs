using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace AdminUP.Converters
{
    public class ImagePathConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string path || string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                var fullPath = Path.IsPathRooted(path)
                    ? path
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);

                if (!File.Exists(fullPath))
                    return null;

                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(fullPath, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.DecodePixelWidth = 80;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => Binding.DoNothing;
    }
}
