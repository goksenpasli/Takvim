using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Tesseract;
using Path = System.IO.Path;

namespace Takvim
{
    public static class Ocr
    {
        public static string OcrYap(this byte[] dosya)
        {
            if (Directory.Exists(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\tessdata"))
            {
                try
                {
                    return GetOcr(dosya);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return string.Empty;
                }
            }

            MessageBox.Show("Tesseract Engine Klasörünü Kontrol Edin.", "TAKVİM", MessageBoxButton.OK, MessageBoxImage.Error);
            return string.Empty;
        }

        public static string OcrYap(this string dosya)
        {
            if (!Directory.Exists(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\tessdata"))
            {
                return null;
            }

            try
            {
                using TesseractEngine engine = new TesseractEngine("./tessdata", "tur", EngineMode.LstmOnly);
                switch (Path.GetExtension(dosya).ToLower())
                {
                    case ".tif":
                    case ".tiff":
                        {
                            using Pix pixImage = Pix.LoadFromFile(dosya);
                            using Page page = engine.Process(pixImage);
                            return page.GetText();
                        }

                    case ".jpg":
                    case ".png":
                    case ".gif":
                    case ".bmp":
                    case ".jpeg":
                        {
                            using Pix pixImage = Pix.LoadFromFile(dosya);
                            using Page page = engine.Process(pixImage);
                            return page.GetText();
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "EBYS", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }

            return null;
        }

        private static string GetOcr(byte[] dosya)
        {
            using TesseractEngine engine = new TesseractEngine("./tessdata", "tur", EngineMode.LstmOnly);
            using Pix pixImage = Pix.LoadFromMemory(dosya);
            using Page page = engine.Process(pixImage);
            return page.GetText();
        }
    }
}