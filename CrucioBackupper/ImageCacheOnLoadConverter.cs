using Serilog;
using System;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CrucioBackupper
{
    public class ImageCacheOnLoadConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var path = (string)value;
            if (path == null)
            {
                return null;
            }
            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(path);
                image.EndInit();
                return image;
            }
            catch (Exception e)
            {
                Log.ForContext<ImageCacheOnLoadConverter>().Error(e, "无法加载图片：{Name}", Path.GetFileNameWithoutExtension(path));
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("Not implemented.");
        }
    }
}
