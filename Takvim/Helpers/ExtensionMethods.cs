﻿using System;
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
using Microsoft.Win32;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Takvim
{
    internal static class ExtensionMethods
    {
        private static readonly IntPtr hwnd = Process.GetCurrentProcess().Handle;

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

        public static BitmapSource IconCreate(this string filepath)
        {
            if (filepath != null)
            {
                try
                {
                    using Icon icon = Icon.ExtractAssociatedIcon(filepath);
                    BitmapSource bitmapsource = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                    icon.Dispose();
                    bitmapsource.Freeze();
                    return bitmapsource;
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
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
                    {
                        TiffBitmapEncoder encoder = new() { Compression = TiffCompressOption.Zip };
                        encoder.Frames.Add(BitmapFrame.Create((BitmapSource)bitmapsource));
                        encoder.Save(outStream);
                        return outStream.ToArray();
                    }
                case Format.Tiff:
                    {
                        TiffBitmapEncoder encoder = new() { Compression = TiffCompressOption.Ccitt4 };
                        encoder.Frames.Add(BitmapFrame.Create((BitmapSource)bitmapsource));
                        encoder.Save(outStream);
                        return outStream.ToArray();
                    }
                case Format.Jpg:
                    {
                        JpegBitmapEncoder encoder = new() { QualityLevel = 75 };
                        encoder.Frames.Add(BitmapFrame.Create((BitmapSource)bitmapsource));
                        encoder.Save(outStream);
                        return outStream.ToArray();
                    }
                case Format.Png:
                    {
                        PngBitmapEncoder encoder = new();
                        encoder.Frames.Add(BitmapFrame.Create((BitmapSource)bitmapsource));
                        encoder.Save(outStream);
                        return outStream.ToArray();
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }

        public static ImageSource WebpDecode(this byte[] rawWebp, double decodeheight = 0)
        {
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
    }
}