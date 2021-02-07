using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Takvim
{
    internal static class ExtensionMethods
    {
        private static readonly IntPtr hwnd = Process.GetCurrentProcess().Handle;

        public static System.Drawing.Bitmap BitmapChangeFormat(this System.Drawing.Bitmap bitmap, System.Drawing.Imaging.PixelFormat format) => bitmap.Clone(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), format);

        public static Brush ConvertToBrush(this System.Drawing.Color color) => new SolidColorBrush(Color.FromArgb(color.A, color.R, color.G, color.B));

        public static System.Drawing.Color ConvertToColor(this Brush color)
        {
            SolidColorBrush sb = (SolidColorBrush)color;
            return System.Drawing.Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool DestroyIcon(this IntPtr handle);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr ExtractIcon(this IntPtr hInst, string lpszExeFileName, int nIconIndex);
        public static BitmapSource IconCreate(this string filepath)
        {
            if (filepath != null)
            {
                try
                {
                    System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(filepath);
                    BitmapSource bitmapsource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    icon.Dispose();
                    if (bitmapsource.CanFreeze)
                    {
                        bitmapsource.Freeze();
                    }
                    return bitmapsource;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

        public static BitmapSource RegistryIconCreate(this string value)
        {
            if (value != null)
            {
                RegistryKey keyForExt = Registry.ClassesRoot.OpenSubKey(value);
                if (keyForExt == null)
                {
                    return null;
                }

                string className = Convert.ToString(keyForExt.GetValue(null));
                RegistryKey keyForClass = Registry.ClassesRoot.OpenSubKey(className);
                if (keyForClass == null)
                {
                    return null;
                }

                RegistryKey keyForIcon = keyForClass.OpenSubKey("DefaultIcon");
                if (keyForIcon == null)
                {
                    RegistryKey keyForCLSID = keyForClass.OpenSubKey("CLSID");
                    if (keyForCLSID != null)
                    {
                        string clsid = $"CLSID\\{Convert.ToString(keyForCLSID.GetValue(null))}\\DefaultIcon";
                        keyForIcon = Registry.ClassesRoot.OpenSubKey(clsid);
                        if (keyForIcon == null)
                        {
                            return null;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }

                string[] defaultIcon = Convert.ToString(keyForIcon.GetValue(null)).Split(',');
                int index = defaultIcon.Length > 1 ? int.Parse(defaultIcon[1]) : 0;
                IntPtr hIcon = hwnd.ExtractIcon(defaultIcon[0], index);
                if (hIcon != IntPtr.Zero)
                {
                    BitmapSource icon = Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    hIcon.DestroyIcon();
                    icon.Freeze();
                    return icon;
                }

                hIcon.DestroyIcon();
            }

            return null;
        }
        public static BitmapImage ToBitmapImage(this System.Drawing.Image bitmap, System.Drawing.Imaging.ImageFormat format, double decodeheight = 0)
        {
            if (bitmap != null)
            {
                MemoryStream memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, format);
                memoryStream.Position = 0;
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                if (decodeheight != 0)
                {
                    image.DecodePixelHeight = bitmap.Height > (int)decodeheight ? (int)decodeheight : bitmap.Height;
                }

                image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                image.CacheOption = BitmapCacheOption.None;
                image.StreamSource = memoryStream;
                image.EndInit();
                bitmap.Dispose();
                GC.Collect();
                if (!image.IsFrozen && image.CanFreeze)
                {
                    image.Freeze();
                }
                return image;
            }

            return null;
        }
        public static BitmapImage WebpDecode(this byte[] rawWebp, double decodeheight = 0)
        {
            using WebP webp = new WebP();
            WebPDecoderOptions options = new WebPDecoderOptions { use_threads = 1 };
            using System.Drawing.Bitmap bmp = webp.Decode(rawWebp, options);
            BitmapImage bitmapimage = null;

            if (bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                bitmapimage = bmp.ToBitmapImage(System.Drawing.Imaging.ImageFormat.Png, decodeheight);
            }
            else
            {
                bitmapimage = bmp.ToBitmapImage(System.Drawing.Imaging.ImageFormat.Jpeg, decodeheight);
            }

            rawWebp = null;
            if (!bitmapimage.IsFrozen && bitmapimage.CanFreeze)
            {
                bitmapimage.Freeze();
            }
            return bitmapimage;
        }
        public static byte[] WebpEncode(this string resimdosyayolu, int kalite)
        {
            try
            {
                using WebP webp = new WebP();
                using System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(resimdosyayolu);
                return bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format24bppRgb || bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? webp.EncodeLossy(bmp, kalite) : webp.EncodeLossy(bmp.BitmapChangeFormat(System.Drawing.Imaging.PixelFormat.Format24bppRgb), kalite);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "EBYS", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}