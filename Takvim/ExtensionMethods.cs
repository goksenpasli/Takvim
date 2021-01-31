using Microsoft.Win32;
using System;
using System.Diagnostics;
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
    }
}