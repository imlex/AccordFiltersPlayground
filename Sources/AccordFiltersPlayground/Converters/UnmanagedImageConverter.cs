using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Accord.Imaging;

namespace AccordFiltersPlayground.Converters
{
    [ValueConversion(typeof(UnmanagedImage), typeof(ImageSource))]
    public class UnmanagedImageConverter : IValueConverter
    {
        private static PixelFormat ConvertPixelFormat(System.Drawing.Imaging.PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case System.Drawing.Imaging.PixelFormat.Format24bppRgb:
                    return PixelFormats.Bgr24;
                case System.Drawing.Imaging.PixelFormat.Format32bppArgb:
                    return PixelFormats.Bgra32;
                case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
                    return PixelFormats.Gray8;

                case System.Drawing.Imaging.PixelFormat.Indexed:
                case System.Drawing.Imaging.PixelFormat.Gdi:
                case System.Drawing.Imaging.PixelFormat.Alpha:
                case System.Drawing.Imaging.PixelFormat.PAlpha:
                case System.Drawing.Imaging.PixelFormat.Extended:
                case System.Drawing.Imaging.PixelFormat.Canonical:
                case System.Drawing.Imaging.PixelFormat.Undefined:
                case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
                case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
                case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb555:
                case System.Drawing.Imaging.PixelFormat.Format16bppRgb565:
                case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:
                case System.Drawing.Imaging.PixelFormat.Format32bppRgb:
                case System.Drawing.Imaging.PixelFormat.Format32bppPArgb:
                case System.Drawing.Imaging.PixelFormat.Format48bppRgb:
                case System.Drawing.Imaging.PixelFormat.Format64bppArgb:
                case System.Drawing.Imaging.PixelFormat.Format64bppPArgb:
                case System.Drawing.Imaging.PixelFormat.Max:
                    Debug.Assert(false);
                    throw new ArgumentOutOfRangeException(nameof(pixelFormat), pixelFormat, null);
            }
            throw new ArgumentOutOfRangeException(nameof(pixelFormat), pixelFormat, null);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is UnmanagedImage ui
                ? BitmapSource.Create(ui.Width, ui.Height, 96, 96, ConvertPixelFormat(ui.PixelFormat), null, ui.ImageData, ui.Bytes, ui.Stride)
                : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}