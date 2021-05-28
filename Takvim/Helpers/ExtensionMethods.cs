﻿using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
        public const uint FILE_ATTRIBUTE_NORMAL = 0x00000080;

        public const uint SHGFI_ICON = 0x000000100;

        public const uint SHGFI_LARGEICON = 0x000000000;

        public const uint SHGFI_OPENICON = 0x000000002;

        public const uint SHGFI_SMALLICON = 0x000000001;

        public const uint SHGFI_TYPENAME = 0x000000400;

        public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

        private static readonly IntPtr hwnd = Process.GetCurrentProcess().Handle;

        public enum FolderType
        {
            Closed,

            Open
        }

        public enum IconSize
        {
            Large,

            Small
        }

        internal enum Format
        {
            Tiff,

            TiffRenkli,

            Jpg,

            Png
        }

        public static Bitmap BitmapChangeFormat(this Bitmap bitmap, System.Drawing.Imaging.PixelFormat format) => bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), format);

        public static bool Contains(this string source, string toCheck, StringComparison comp) => source?.IndexOf(toCheck, comp) >= 0;

        public static System.Windows.Media.Brush ConvertToBrush(this System.Drawing.Color color) => new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));

        public static System.Drawing.Color ConvertToColor(this System.Windows.Media.Brush color)
        {
            SolidColorBrush sb = (SolidColorBrush)color;
            return System.Drawing.Color.FromArgb(sb.Color.A, sb.Color.R, sb.Color.G, sb.Color.B);
        }

        public static PdfDocument CreatePdfFile(this IList SeçiliResimler, bool compress = false)
        {
            try
            {
                using PdfDocument doc = new();
                if (compress)
                {
                    doc.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression;
                    doc.Options.UseFlateDecoderForJpegImages = PdfUseFlateDecoderForJpegImages.Automatic;
                    doc.Options.NoCompression = false;
                    doc.Options.CompressContentStreams = true;
                }
                foreach (BitmapFrame item in SeçiliResimler)
                {
                    PdfPage page = doc.AddPage();
                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    XImage xImage = XImage.FromBitmapSource(item);
                    gfx.DrawImage(xImage, 0, 0, page.Width, page.Height);
                }
                return doc;
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
                return null;
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool DestroyIcon(this IntPtr handle);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr ExtractIcon(this IntPtr hInst, string lpszExeFileName, int nIconIndex);

        public static BitmapSource IconCreate(this string path, IconSize size)
        {
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;

            if (IconSize.Small == size)
            {
                flags += SHGFI_SMALLICON;
            }
            else
            {
                flags += SHGFI_LARGEICON;
            }
            SHFILEINFO shfi = new();

            IntPtr res = SHGetFileInfo(path, FILE_ATTRIBUTE_NORMAL, out shfi, (uint)Marshal.SizeOf(shfi), flags);

            //if (res == IntPtr.Zero)
            //{
            //    throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
            //}

            Icon.FromHandle(shfi.hIcon);
            using Icon icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
            DestroyIcon(shfi.hIcon);
            BitmapSource bitmapsource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bitmapsource.Freeze();
            return bitmapsource;
        }

        public static string GetFileType(this string filename)
        {
            SHFILEINFO shinfo = new();
            SHGetFileInfo
                (
                        filename,
                        FILE_ATTRIBUTE_NORMAL,
                        out shinfo, (uint)Marshal.SizeOf(shinfo),
                        SHGFI_TYPENAME |
                        SHGFI_USEFILEATTRIBUTES
                    );

            return shinfo.szTypeName;
        }

        public static BitmapSource IconCreate(this string filepath, int iconindex)
        {
            if (filepath != null)
            {
                string defaultIcon = filepath;
                IntPtr hIcon = hwnd.ExtractIcon(defaultIcon, iconindex);
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

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        public static ImageSource ToBitmapImage(this Image bitmap, ImageFormat format, double decodeheight = 0)
        {
            if (bitmap != null)
            {
                using MemoryStream memoryStream = new();
                bitmap.Save(memoryStream, format);
                memoryStream.Position = 0;
                BitmapImage image = new();
                image.BeginInit();
                if (decodeheight != 0)
                {
                    image.DecodePixelHeight = bitmap.Height > (int)decodeheight ? (int)decodeheight : bitmap.Height;
                }

                image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = memoryStream;
                image.EndInit();
                bitmap.Dispose();
                image.Freeze();
                return image;
            }

            return null;
        }

        public static byte[] ToTiffJpegByteArray(this ImageSource bitmapsource, Format format)
        {
            using MemoryStream outStream = new();
            switch (format)
            {
                case Format.TiffRenkli:
                    TiffBitmapEncoder tifzipencoder = new() { Compression = TiffCompressOption.Zip };
                    tifzipencoder.Frames.Add(BitmapFrame.Create((BitmapSource)bitmapsource));
                    tifzipencoder.Save(outStream);
                    return outStream.ToArray();

                case Format.Tiff:
                    TiffBitmapEncoder tifccittencoder = new() { Compression = TiffCompressOption.Ccitt4 };
                    tifccittencoder.Frames.Add(BitmapFrame.Create((BitmapSource)bitmapsource));
                    tifccittencoder.Save(outStream);
                    return outStream.ToArray();

                case Format.Jpg:
                    JpegBitmapEncoder jpgencoder = new() { QualityLevel = 75 };
                    jpgencoder.Frames.Add(BitmapFrame.Create((BitmapSource)bitmapsource));
                    jpgencoder.Save(outStream);
                    return outStream.ToArray();

                case Format.Png:
                    PngBitmapEncoder pngencoder = new();
                    pngencoder.Frames.Add(BitmapFrame.Create((BitmapSource)bitmapsource));
                    pngencoder.Save(outStream);
                    return outStream.ToArray();

                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        public static ImageSource WebpDecode(this byte[] rawWebp, double decodeheight = 0)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
            using WebP webp = new();
            WebPDecoderOptions options = new() { use_threads = 1 };
            using Bitmap bmp = webp.Decode(rawWebp, options);
            ImageSource imagesource = null;

            if (bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            {
                imagesource = bmp.ToBitmapImage(ImageFormat.Png, decodeheight);
            }
            else
            {
                imagesource = bmp.ToBitmapImage(ImageFormat.Jpeg, decodeheight);
            }

            rawWebp = null;
            imagesource.Freeze();
            return imagesource;
        }

        public static byte[] WebpEncode(this byte[] resim, int kalite)
        {
            try
            {
                using WebP webp = new();
                using MemoryStream ms = new(resim);
                using Bitmap bmp = Image.FromStream(ms) as Bitmap;
                resim = null;
                return webp.EncodeLossy(bmp, kalite);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static byte[] WebpEncode(this string resimdosyayolu, int kalite)
        {
            try
            {
                using WebP webp = new();
                using Bitmap bmp = new(resimdosyayolu);
                return bmp.PixelFormat is System.Drawing.Imaging.PixelFormat.Format24bppRgb or System.Drawing.Imaging.PixelFormat.Format32bppArgb ? webp.EncodeLossy(bmp, kalite) : webp.EncodeLossy(bmp.BitmapChangeFormat(System.Drawing.Imaging.PixelFormat.Format24bppRgb), kalite);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;

            public int iIcon;

            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };
    }
}