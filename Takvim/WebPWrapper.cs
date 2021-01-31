using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;

namespace Takvim
{
    public sealed class WebP : IDisposable
    {
        private const int WEBP_MAX_DIMENSION = 16383;

        #region | Destruction |

        public void Dispose() => GC.SuppressFinalize(this);

        #endregion | Destruction |

        #region | Public Decode Functions |

        public Bitmap Decode(byte[] rawWebP)
        {
            Bitmap bmp = null;
            BitmapData bmpData = null;
            GCHandle pinnedWebP = GCHandle.Alloc(rawWebP, GCHandleType.Pinned);
            int size;
            try
            {
                GetInfo(rawWebP, out int imgWidth, out int imgHeight, out bool hasAlpha, out bool hasAnimation, out string format);
                if (hasAlpha)
                {
                    bmp = new Bitmap(imgWidth, imgHeight, PixelFormat.Format32bppArgb);
                }
                else
                {
                    bmp = new Bitmap(imgWidth, imgHeight, PixelFormat.Format24bppRgb);
                }

                bmpData = bmp.LockBits(new Rectangle(0, 0, imgWidth, imgHeight), ImageLockMode.WriteOnly, bmp.PixelFormat);
                int outputSize = bmpData.Stride * imgHeight;
                IntPtr ptrData = pinnedWebP.AddrOfPinnedObject();
                if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    size = UnsafeNativeMethods.WebPDecodeBGRInto(ptrData, rawWebP.Length, bmpData.Scan0, outputSize, bmpData.Stride);
                }
                else
                {
                    size = UnsafeNativeMethods.WebPDecodeBGRAInto(ptrData, rawWebP.Length, bmpData.Scan0, outputSize, bmpData.Stride);
                }

                if (size == 0)
                {
                    throw new Exception("Can´t encode WebP");
                }

                return bmp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.Decode");
            }
            finally
            {
                if (bmpData != null)
                {
                    bmp.UnlockBits(bmpData);
                }

                if (pinnedWebP.IsAllocated)
                {
                    pinnedWebP.Free();
                }
            }
        }

        public Bitmap Decode(byte[] rawWebP, WebPDecoderOptions options)
        {
            GCHandle pinnedWebP = GCHandle.Alloc(rawWebP, GCHandleType.Pinned);
            Bitmap bmp = null;
            BitmapData bmpData = null;
            VP8StatusCode result;
            try
            {
                WebPDecoderConfig config = new WebPDecoderConfig();
                if (UnsafeNativeMethods.WebPInitDecoderConfig(ref config) == 0)
                {
                    throw new Exception("WebPInitDecoderConfig failed. Wrong version?");
                }

                IntPtr ptrRawWebP = pinnedWebP.AddrOfPinnedObject();
                int height;
                int width;
                if (options.use_scaling == 0)
                {
                    result = UnsafeNativeMethods.WebPGetFeatures(ptrRawWebP, rawWebP.Length, ref config.input);
                    if (result != VP8StatusCode.VP8_STATUS_OK)
                    {
                        throw new Exception("Failed WebPGetFeatures with error " + result);
                    }

                    if (options.use_cropping == 1)
                    {
                        if (options.crop_left + options.crop_width > config.input.Width || options.crop_top + options.crop_height > config.input.Height)
                        {
                            throw new Exception("Crop options exceded WebP image dimensions");
                        }

                        width = options.crop_width;
                        height = options.crop_height;
                    }
                }
                else
                {
                    width = options.scaled_width;
                    height = options.scaled_height;
                }

                config.options.bypass_filtering = options.bypass_filtering;
                config.options.no_fancy_upsampling = options.no_fancy_upsampling;
                config.options.use_cropping = options.use_cropping;
                config.options.crop_left = options.crop_left;
                config.options.crop_top = options.crop_top;
                config.options.crop_width = options.crop_width;
                config.options.crop_height = options.crop_height;
                config.options.use_scaling = options.use_scaling;
                config.options.scaled_width = options.scaled_width;
                config.options.scaled_height = options.scaled_height;
                config.options.use_threads = options.use_threads;
                config.options.dithering_strength = options.dithering_strength;
                config.options.flip = options.flip;
                config.options.alpha_dithering_strength = options.alpha_dithering_strength;
                if (config.input.Has_alpha == 1)
                {
                    config.output.colorspace = WEBP_CSP_MODE.MODE_bgrA;
                    bmp = new Bitmap(config.input.Width, config.input.Height, PixelFormat.Format32bppArgb);
                }
                else
                {
                    config.output.colorspace = WEBP_CSP_MODE.MODE_BGR;
                    bmp = new Bitmap(config.input.Width, config.input.Height, PixelFormat.Format24bppRgb);
                }

                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                config.output.u.RGBA.rgba = bmpData.Scan0;
                config.output.u.RGBA.stride = bmpData.Stride;
                config.output.u.RGBA.size = (UIntPtr)(bmp.Height * bmpData.Stride);
                config.output.height = bmp.Height;
                config.output.width = bmp.Width;
                config.output.is_external_memory = 1;
                result = UnsafeNativeMethods.WebPDecode(ptrRawWebP, rawWebP.Length, ref config);
                if (result != VP8StatusCode.VP8_STATUS_OK)
                {
                    throw new Exception("Failed WebPDecode with error " + result);
                }

                UnsafeNativeMethods.WebPFreeDecBuffer(ref config.output);
                return bmp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.Decode");
            }
            finally
            {
                if (bmpData != null)
                {
                    bmp.UnlockBits(bmpData);
                }

                if (pinnedWebP.IsAllocated)
                {
                    pinnedWebP.Free();
                }
            }
        }

        public Bitmap GetThumbnailFast(byte[] rawWebP, int width, int height)
        {
            GCHandle pinnedWebP = GCHandle.Alloc(rawWebP, GCHandleType.Pinned);
            Bitmap bmp = null;
            BitmapData bmpData = null;
            try
            {
                WebPDecoderConfig config = new WebPDecoderConfig();
                if (UnsafeNativeMethods.WebPInitDecoderConfig(ref config) == 0)
                {
                    throw new Exception("WebPInitDecoderConfig failed. Wrong version?");
                }

                config.options.bypass_filtering = 1;
                config.options.no_fancy_upsampling = 1;
                config.options.use_threads = 1;
                config.options.use_scaling = 1;
                config.options.scaled_width = width;
                config.options.scaled_height = height;
                bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                config.output.colorspace = WEBP_CSP_MODE.MODE_BGR;
                config.output.u.RGBA.rgba = bmpData.Scan0;
                config.output.u.RGBA.stride = bmpData.Stride;
                config.output.u.RGBA.size = (UIntPtr)(height * bmpData.Stride);
                config.output.height = height;
                config.output.width = width;
                config.output.is_external_memory = 1;
                IntPtr ptrRawWebP = pinnedWebP.AddrOfPinnedObject();
                VP8StatusCode result = UnsafeNativeMethods.WebPDecode(ptrRawWebP, rawWebP.Length, ref config);
                if (result != VP8StatusCode.VP8_STATUS_OK)
                {
                    throw new Exception("Failed WebPDecode with error " + result);
                }

                UnsafeNativeMethods.WebPFreeDecBuffer(ref config.output);
                return bmp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.Thumbnail");
            }
            finally
            {
                if (bmpData != null)
                {
                    bmp.UnlockBits(bmpData);
                }

                if (pinnedWebP.IsAllocated)
                {
                    pinnedWebP.Free();
                }
            }
        }

        public Bitmap GetThumbnailQuality(byte[] rawWebP, int width, int height)
        {
            GCHandle pinnedWebP = GCHandle.Alloc(rawWebP, GCHandleType.Pinned);
            Bitmap bmp = null;
            BitmapData bmpData = null;
            try
            {
                WebPDecoderConfig config = new WebPDecoderConfig();
                if (UnsafeNativeMethods.WebPInitDecoderConfig(ref config) == 0)
                {
                    throw new Exception("WebPInitDecoderConfig failed. Wrong version?");
                }

                IntPtr ptrRawWebP = pinnedWebP.AddrOfPinnedObject();
                VP8StatusCode result = UnsafeNativeMethods.WebPGetFeatures(ptrRawWebP, rawWebP.Length, ref config.input);
                if (result != VP8StatusCode.VP8_STATUS_OK)
                {
                    throw new Exception("Failed WebPGetFeatures with error " + result);
                }

                config.options.bypass_filtering = 0;
                config.options.no_fancy_upsampling = 0;
                config.options.use_threads = 1;
                config.options.use_scaling = 1;
                config.options.scaled_width = width;
                config.options.scaled_height = height;
                if (config.input.Has_alpha == 1)
                {
                    config.output.colorspace = WEBP_CSP_MODE.MODE_bgrA;
                    bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                }
                else
                {
                    config.output.colorspace = WEBP_CSP_MODE.MODE_BGR;
                    bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                }

                bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                config.output.u.RGBA.rgba = bmpData.Scan0;
                config.output.u.RGBA.stride = bmpData.Stride;
                config.output.u.RGBA.size = (UIntPtr)(height * bmpData.Stride);
                config.output.height = height;
                config.output.width = width;
                config.output.is_external_memory = 1;
                result = UnsafeNativeMethods.WebPDecode(ptrRawWebP, rawWebP.Length, ref config);
                if (result != VP8StatusCode.VP8_STATUS_OK)
                {
                    throw new Exception("Failed WebPDecode with error " + result);
                }

                UnsafeNativeMethods.WebPFreeDecBuffer(ref config.output);
                return bmp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.Thumbnail");
            }
            finally
            {
                if (bmpData != null)
                {
                    bmp.UnlockBits(bmpData);
                }

                if (pinnedWebP.IsAllocated)
                {
                    pinnedWebP.Free();
                }
            }
        }

        public Bitmap Load(string pathFileName)
        {
            try
            {
                byte[] rawWebP = File.ReadAllBytes(pathFileName);
                return Decode(rawWebP);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.Load");
            }
        }

        public Bitmap Load(string pathFileName, WebPDecoderOptions webPDecoderOptions)
        {
            try
            {
                byte[] rawWebP = File.ReadAllBytes(pathFileName);
                return Decode(rawWebP, webPDecoderOptions);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.Load");
            }
        }

        #endregion | Public Decode Functions |

        #region | Public Encode Functions |

        public byte[] EncodeLossless(Bitmap bmp)
        {
            if (bmp.Width == 0 || bmp.Height == 0)
            {
                throw new ArgumentException("Bitmap contains no data.", nameof(bmp));
            }

            if (bmp.Width > WEBP_MAX_DIMENSION || bmp.Height > WEBP_MAX_DIMENSION)
            {
                throw new NotSupportedException("Bitmap's dimension is too large. Max is " + WEBP_MAX_DIMENSION + "x" + WEBP_MAX_DIMENSION + " pixels.");
            }

            if (bmp.PixelFormat != PixelFormat.Format24bppRgb && bmp.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new NotSupportedException("Sadece 24 ve 32 bit resim desteklenir.");
            }

            BitmapData bmpData = null;
            IntPtr unmanagedData = IntPtr.Zero;
            try
            {
                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                int size;
                if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    size = UnsafeNativeMethods.WebPEncodeLosslessBGR(bmpData.Scan0, bmp.Width, bmp.Height, bmpData.Stride, out unmanagedData);
                }
                else
                {
                    size = UnsafeNativeMethods.WebPEncodeLosslessBGRA(bmpData.Scan0, bmp.Width, bmp.Height, bmpData.Stride, out unmanagedData);
                }

                byte[] rawWebP = new byte[size];
                Marshal.Copy(unmanagedData, rawWebP, 0, size);
                return rawWebP;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.EncodeLossless (Simple)");
            }
            finally
            {
                if (bmpData != null)
                {
                    bmp.UnlockBits(bmpData);
                }

                if (unmanagedData != IntPtr.Zero)
                {
                    UnsafeNativeMethods.WebPFree(unmanagedData);
                }
            }
        }

        public byte[] EncodeLossless(Bitmap bmp, int speed)
        {
            WebPConfig config = new WebPConfig();
            if (UnsafeNativeMethods.WebPConfigInit(ref config, WebPPreset.WEBP_PRESET_DEFAULT, (speed + 1) * 10) == 0)
            {
                throw new Exception("Can´t config preset");
            }

            if (UnsafeNativeMethods.WebPGetDecoderVersion() > 1082)
            {
                if (UnsafeNativeMethods.WebPConfigLosslessPreset(ref config, speed) == 0)
                {
                    throw new Exception("Can´t config lossless preset");
                }
            }
            else
            {
                config.lossless = 1;
                config.method = speed;
                if (config.method > 6)
                {
                    config.method = 6;
                }

                config.quality = (speed + 1) * 10;
            }

            config.pass = speed + 1;
            config.thread_level = 1;
            config.alpha_filtering = 2;
            config.use_sharp_yuv = 1;
            config.exact = 0;
            return AdvancedEncode(bmp, config, false);
        }

        public byte[] EncodeLossy(Bitmap bmp, int quality = 75)
        {
            if (bmp.Width == 0 || bmp.Height == 0)
            {
                throw new ArgumentException("Bitmap contains no data.", nameof(bmp));
            }

            if (bmp.Width > WEBP_MAX_DIMENSION || bmp.Height > WEBP_MAX_DIMENSION)
            {
                throw new NotSupportedException("Bitmap's dimension is too large. Max is " + WEBP_MAX_DIMENSION + "x" + WEBP_MAX_DIMENSION + " pixels.");
            }

            if (bmp.PixelFormat != PixelFormat.Format24bppRgb && bmp.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new NotSupportedException("Sadece 24 ve 32 bit resim desteklenir.");
            }

            BitmapData bmpData = null;
            IntPtr unmanagedData = IntPtr.Zero;
            int size;
            try
            {
                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                if (bmp.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    size = UnsafeNativeMethods.WebPEncodeBGR(bmpData.Scan0, bmp.Width, bmp.Height, bmpData.Stride, quality, out unmanagedData);
                }
                else
                {
                    size = UnsafeNativeMethods.WebPEncodeBGRA(bmpData.Scan0, bmp.Width, bmp.Height, bmpData.Stride, quality, out unmanagedData);
                }

                if (size == 0)
                {
                    throw new Exception("Can´t encode WebP");
                }

                byte[] rawWebP = new byte[size];
                Marshal.Copy(unmanagedData, rawWebP, 0, size);
                return rawWebP;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.EncodeLossly");
            }
            finally
            {
                if (bmpData != null)
                {
                    bmp.UnlockBits(bmpData);
                }

                if (unmanagedData != IntPtr.Zero)
                {
                    UnsafeNativeMethods.WebPFree(unmanagedData);
                }
            }
        }

        public byte[] EncodeLossy(Bitmap bmp, int quality, int speed, bool info = false)
        {
            WebPConfig config = new WebPConfig();
            if (UnsafeNativeMethods.WebPConfigInit(ref config, WebPPreset.WEBP_PRESET_DEFAULT, 75) == 0)
            {
                throw new Exception("Can´t config preset");
            }

            config.method = speed;
            if (config.method > 6)
            {
                config.method = 6;
            }

            config.quality = quality;
            config.autofilter = 1;
            config.pass = speed + 1;
            config.segments = 4;
            config.partitions = 3;
            config.thread_level = 1;
            config.alpha_quality = quality;
            config.alpha_filtering = 2;
            config.use_sharp_yuv = 1;
            if (UnsafeNativeMethods.WebPGetDecoderVersion() > 1082)
            {
                config.preprocessing = 4;
                config.use_sharp_yuv = 1;
            }
            else
            {
                config.preprocessing = 3;
            }

            return AdvancedEncode(bmp, config, info);
        }

        public byte[] EncodeNearLossless(Bitmap bmp, int quality, int speed = 9)
        {
            if (UnsafeNativeMethods.WebPGetDecoderVersion() <= 1082)
            {
                throw new Exception("This dll version not suport EncodeNearLossless");
            }

            WebPConfig config = new WebPConfig();
            if (UnsafeNativeMethods.WebPConfigInit(ref config, WebPPreset.WEBP_PRESET_DEFAULT, (speed + 1) * 10) == 0)
            {
                throw new Exception("Can´t config preset");
            }

            if (UnsafeNativeMethods.WebPConfigLosslessPreset(ref config, speed) == 0)
            {
                throw new Exception("Can´t config lossless preset");
            }

            config.pass = speed + 1;
            config.near_lossless = quality;
            config.thread_level = 1;
            config.alpha_filtering = 2;
            config.use_sharp_yuv = 1;
            config.exact = 0;
            return AdvancedEncode(bmp, config, false);
        }

        public void Save(Bitmap bmp, string pathFileName, int quality = 75)
        {
            byte[] rawWebP;
            try
            {
                rawWebP = EncodeLossy(bmp, quality);
                File.WriteAllBytes(pathFileName, rawWebP);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.Save");
            }
        }

        #endregion | Public Encode Functions |

        #region | Another Public Functions |

        public void GetInfo(byte[] rawWebP, out int width, out int height, out bool has_alpha, out bool has_animation, out string format)
        {
            VP8StatusCode result;
            GCHandle pinnedWebP = GCHandle.Alloc(rawWebP, GCHandleType.Pinned);
            try
            {
                IntPtr ptrRawWebP = pinnedWebP.AddrOfPinnedObject();
                WebPBitstreamFeatures features = new WebPBitstreamFeatures();
                result = UnsafeNativeMethods.WebPGetFeatures(ptrRawWebP, rawWebP.Length, ref features);
                if (result != 0)
                {
                    throw new Exception(result.ToString());
                }

                width = features.Width;
                height = features.Height;
                has_alpha = features.Has_alpha == 1;
                has_animation = features.Has_animation == 1;
                format = features.Format switch
                {
                    1 => "lossy",
                    2 => "lossless",
                    _ => "undefined",
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.GetInfo");
            }
            finally
            {
                if (pinnedWebP.IsAllocated)
                {
                    pinnedWebP.Free();
                }
            }
        }

        public float[] GetPictureDistortion(Bitmap source, Bitmap reference, int metric_type)
        {
            WebPPicture wpicSource = new WebPPicture();
            WebPPicture wpicReference = new WebPPicture();
            BitmapData sourceBmpData = null;
            BitmapData referenceBmpData = null;
            float[] result = new float[5];
            GCHandle pinnedResult = GCHandle.Alloc(result, GCHandleType.Pinned);
            try
            {
                if (source == null)
                {
                    throw new Exception("Source picture is void");
                }

                if (reference == null)
                {
                    throw new Exception("Reference picture is void");
                }

                if (metric_type > 2)
                {
                    throw new Exception("Bad metric_type. Use 0 = PSNR, 1 = SSIM, 2 = LSIM");
                }

                if (source.Width != reference.Width || source.Height != reference.Height)
                {
                    throw new Exception("Source and Reference pictures have diferent dimensions");
                }

                sourceBmpData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, source.PixelFormat);
                wpicSource = new WebPPicture();
                if (UnsafeNativeMethods.WebPPictureInitInternal(ref wpicSource) != 1)
                {
                    throw new Exception("Can´t init WebPPictureInit");
                }

                wpicSource.width = source.Width;
                wpicSource.height = source.Height;
                if (sourceBmpData.PixelFormat == PixelFormat.Format32bppArgb)
                {
                    wpicSource.use_argb = 1;
                    if (UnsafeNativeMethods.WebPPictureImportBGRA(ref wpicSource, sourceBmpData.Scan0, sourceBmpData.Stride) != 1)
                    {
                        throw new Exception("Can´t allocate memory in WebPPictureImportBGR");
                    }
                }
                else
                {
                    wpicSource.use_argb = 0;
                    if (UnsafeNativeMethods.WebPPictureImportBGR(ref wpicSource, sourceBmpData.Scan0, sourceBmpData.Stride) != 1)
                    {
                        throw new Exception("Can´t allocate memory in WebPPictureImportBGR");
                    }
                }

                referenceBmpData = reference.LockBits(new Rectangle(0, 0, reference.Width, reference.Height), ImageLockMode.ReadOnly, reference.PixelFormat);
                wpicReference = new WebPPicture();
                if (UnsafeNativeMethods.WebPPictureInitInternal(ref wpicReference) != 1)
                {
                    throw new Exception("Can´t init WebPPictureInit");
                }

                wpicReference.width = reference.Width;
                wpicReference.height = reference.Height;
                wpicReference.use_argb = 1;
                if (sourceBmpData.PixelFormat == PixelFormat.Format32bppArgb)
                {
                    wpicSource.use_argb = 1;
                    if (UnsafeNativeMethods.WebPPictureImportBGRA(ref wpicReference, referenceBmpData.Scan0, referenceBmpData.Stride) != 1)
                    {
                        throw new Exception("Can´t allocate memory in WebPPictureImportBGR");
                    }
                }
                else
                {
                    wpicSource.use_argb = 0;
                    if (UnsafeNativeMethods.WebPPictureImportBGR(ref wpicReference, referenceBmpData.Scan0, referenceBmpData.Stride) != 1)
                    {
                        throw new Exception("Can´t allocate memory in WebPPictureImportBGR");
                    }
                }

                IntPtr ptrResult = pinnedResult.AddrOfPinnedObject();
                if (UnsafeNativeMethods.WebPPictureDistortion(ref wpicSource, ref wpicReference, metric_type, ptrResult) != 1)
                {
                    throw new Exception("Can´t measure.");
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.GetPictureDistortion");
            }
            finally
            {
                if (sourceBmpData != null)
                {
                    source.UnlockBits(sourceBmpData);
                }

                if (referenceBmpData != null)
                {
                    reference.UnlockBits(referenceBmpData);
                }

                if (wpicSource.argb != IntPtr.Zero)
                {
                    UnsafeNativeMethods.WebPPictureFree(ref wpicSource);
                }

                if (wpicReference.argb != IntPtr.Zero)
                {
                    UnsafeNativeMethods.WebPPictureFree(ref wpicReference);
                }

                if (pinnedResult.IsAllocated)
                {
                    pinnedResult.Free();
                }
            }
        }

        public string GetVersion()
        {
            try
            {
                uint v = (uint)UnsafeNativeMethods.WebPGetDecoderVersion();
                uint revision = v % 256;
                uint minor = (v >> 8) % 256;
                uint major = (v >> 16) % 256;
                return major + "." + minor + "." + revision;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.GetVersion");
            }
        }

        #endregion | Another Public Functions |

        #region | Private Methods |

        private byte[] AdvancedEncode(Bitmap bmp, WebPConfig config, bool info)
        {
            byte[] rawWebP = null;
            byte[] dataWebp = null;
            WebPPicture wpic = new WebPPicture();
            BitmapData bmpData = null;
            WebPAuxStats stats = new WebPAuxStats();
            IntPtr ptrStats = IntPtr.Zero;
            GCHandle pinnedArrayHandle = new GCHandle();
            int dataWebpSize;
            try
            {
                if (UnsafeNativeMethods.WebPValidateConfig(ref config) != 1)
                {
                    throw new Exception("Bad config parameters");
                }

                if (bmp.Width == 0 || bmp.Height == 0)
                {
                    throw new ArgumentException("Bitmap contains no data.", nameof(bmp));
                }

                if (bmp.Width > WEBP_MAX_DIMENSION || bmp.Height > WEBP_MAX_DIMENSION)
                {
                    throw new NotSupportedException("Bitmap's dimension is too large. Max is " + WEBP_MAX_DIMENSION + "x" + WEBP_MAX_DIMENSION + " pixels.");
                }

                if (bmp.PixelFormat != PixelFormat.Format24bppRgb && bmp.PixelFormat != PixelFormat.Format32bppArgb)
                {
                    throw new NotSupportedException("Sadece 24 ve 32 bit resim desteklenir.");
                }

                bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                if (UnsafeNativeMethods.WebPPictureInitInternal(ref wpic) != 1)
                {
                    throw new Exception("Can´t init WebPPictureInit");
                }

                wpic.width = bmp.Width;
                wpic.height = bmp.Height;
                wpic.use_argb = 1;
                if (bmp.PixelFormat == PixelFormat.Format32bppArgb)
                {
                    int result = UnsafeNativeMethods.WebPPictureImportBGRA(ref wpic, bmpData.Scan0, bmpData.Stride);
                    if (result != 1)
                    {
                        throw new Exception("Can´t allocate memory in WebPPictureImportBGRA");
                    }

                    wpic.colorspace = (uint)WEBP_CSP_MODE.MODE_bgrA;
                    dataWebpSize = bmp.Width * bmp.Height * 32;
                    dataWebp = new byte[bmp.Width * bmp.Height * 32];
                }
                else
                {
                    int result = UnsafeNativeMethods.WebPPictureImportBGR(ref wpic, bmpData.Scan0, bmpData.Stride);
                    if (result != 1)
                    {
                        throw new Exception("Can´t allocate memory in WebPPictureImportBGR");
                    }

                    dataWebpSize = bmp.Width * bmp.Height * 24;
                }

                if (info)
                {
                    stats = new WebPAuxStats();
                    ptrStats = Marshal.AllocHGlobal(Marshal.SizeOf(stats));
                    Marshal.StructureToPtr(stats, ptrStats, false);
                    wpic.stats = ptrStats;
                }

                if (dataWebpSize > 2147483591)
                {
                    dataWebpSize = 2147483591;
                }

                dataWebp = new byte[bmp.Width * bmp.Height * 32];
                pinnedArrayHandle = GCHandle.Alloc(dataWebp, GCHandleType.Pinned);
                IntPtr initPtr = pinnedArrayHandle.AddrOfPinnedObject();
                wpic.custom_ptr = initPtr;
                UnsafeNativeMethods.OnCallback = MyWriter;
                wpic.writer = Marshal.GetFunctionPointerForDelegate(UnsafeNativeMethods.OnCallback);
                if (UnsafeNativeMethods.WebPEncode(ref config, ref wpic) != 1)
                {
                    throw new Exception("Encoding error: " + (WebPEncodingError)wpic.error_code);
                }

                UnsafeNativeMethods.OnCallback = null;
                bmp.UnlockBits(bmpData);
                bmpData = null;
                int size = (int)((long)wpic.custom_ptr - (long)initPtr);
                rawWebP = new byte[size];
                Array.Copy(dataWebp, rawWebP, size);
                pinnedArrayHandle.Free();
                dataWebp = null;
                if (info)
                {
                    stats = (WebPAuxStats)Marshal.PtrToStructure(ptrStats, typeof(WebPAuxStats));
                    MessageBox.Show(
                        $"Dimension: {wpic.width} x {wpic.height} pixels\nOutput:    {stats.coded_size} bytes\nPSNR Y:    {stats.PSNRY} db\nPSNR u:    {stats.PSNRU} db\nPSNR v:    {stats.PSNRV} db\nPSNR ALL:  {stats.PSNRALL} db\nBlock intra4:  {stats.block_count_intra4}\nBlock intra16: {stats.block_count_intra16}\nBlock skipped: {stats.block_count_skipped}\nHeader size:    {stats.header_bytes} bytes\nMode-partition: {stats.mode_partition_0} bytes\nMacroblocks 0:  {stats.segment_size_segments0} residuals bytes\nMacroblocks 1:  {stats.segment_size_segments1} residuals bytes\nMacroblocks 2:  {stats.segment_size_segments2} residuals bytes\nMacroblocks 3:  {stats.segment_size_segments3} residuals bytes\nQuantizer   0:  {stats.segment_quant_segments0} residuals bytes\nQuantizer   1:  {stats.segment_quant_segments1} residuals bytes\nQuantizer   2:  {stats.segment_quant_segments2} residuals bytes\nQuantizer   3:  {stats.segment_quant_segments3} residuals bytes\nFilter level 0: {stats.segment_level_segments0} residuals bytes\nFilter level 1: {stats.segment_level_segments1} residuals bytes\nFilter level 2: {stats.segment_level_segments2} residuals bytes\nFilter level 3: {stats.segment_level_segments3} residuals bytes\n",
                        "Compression statistics");
                }

                return rawWebP;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\nIn WebP.AdvancedEncode");
            }
            finally
            {
                if (pinnedArrayHandle.IsAllocated)
                {
                    pinnedArrayHandle.Free();
                }

                if (ptrStats != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptrStats);
                }

                if (bmpData != null)
                {
                    bmp.UnlockBits(bmpData);
                }

                if (wpic.argb != IntPtr.Zero)
                {
                    UnsafeNativeMethods.WebPPictureFree(ref wpic);
                }
            }
        }

        private int MyWriter([In] IntPtr data, UIntPtr data_size, ref WebPPicture picture)
        {
            UnsafeNativeMethods.CopyMemory(picture.custom_ptr, data, (uint)data_size);
            picture.custom_ptr = new IntPtr(picture.custom_ptr.ToInt64() + (int)data_size);
            return 1;
        }

        #endregion | Private Methods |
    }

    #region | Import libwebp functions |

    [SuppressUnmanagedCodeSecurity]
    internal sealed class UnsafeNativeMethods
    {
        public static WebPMemoryWrite OnCallback;

        private const int WEBP_DECODER_ABI_VERSION = 0x0208;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int WebPMemoryWrite([In] IntPtr data, UIntPtr data_size, ref WebPPicture wpic);

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        public static int WebPConfigInit(ref WebPConfig config, WebPPreset preset, float quality)
        {
            return IntPtr.Size switch
            {
                4 => WebPConfigInitInternal_x86(ref config, preset, quality, WEBP_DECODER_ABI_VERSION),
                8 => WebPConfigInitInternal_x64(ref config, preset, quality, WEBP_DECODER_ABI_VERSION),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPConfigLosslessPreset(ref WebPConfig config, int level)
        {
            return IntPtr.Size switch
            {
                4 => WebPConfigLosslessPreset_x86(ref config, level),
                8 => WebPConfigLosslessPreset_x64(ref config, level),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static VP8StatusCode WebPDecode(IntPtr data, int data_size, ref WebPDecoderConfig webPDecoderConfig)
        {
            return IntPtr.Size switch
            {
                4 => WebPDecode_x86(data, (UIntPtr)data_size, ref webPDecoderConfig),
                8 => WebPDecode_x64(data, (UIntPtr)data_size, ref webPDecoderConfig),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPDecodeBGRAInto(IntPtr data, int data_size, IntPtr output_buffer, int output_buffer_size, int output_stride)
        {
            return IntPtr.Size switch
            {
                4 => WebPDecodeBGRAInto_x86(data, (UIntPtr)data_size, output_buffer, output_buffer_size, output_stride),
                8 => WebPDecodeBGRAInto_x64(data, (UIntPtr)data_size, output_buffer, output_buffer_size, output_stride),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPDecodeBGRInto(IntPtr data, int data_size, IntPtr output_buffer, int output_buffer_size, int output_stride)
        {
            return IntPtr.Size switch
            {
                4 => WebPDecodeBGRInto_x86(data, (UIntPtr)data_size, output_buffer, output_buffer_size, output_stride),
                8 => WebPDecodeBGRInto_x64(data, (UIntPtr)data_size, output_buffer, output_buffer_size, output_stride),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPEncode(ref WebPConfig config, ref WebPPicture picture)
        {
            return IntPtr.Size switch
            {
                4 => WebPEncode_x86(ref config, ref picture),
                8 => WebPEncode_x64(ref config, ref picture),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPEncodeBGR(IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output)
        {
            return IntPtr.Size switch
            {
                4 => WebPEncodeBGR_x86(bgr, width, height, stride, quality_factor, out output),
                8 => WebPEncodeBGR_x64(bgr, width, height, stride, quality_factor, out output),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPEncodeBGRA(IntPtr bgra, int width, int height, int stride, float quality_factor, out IntPtr output)
        {
            return IntPtr.Size switch
            {
                4 => WebPEncodeBGRA_x86(bgra, width, height, stride, quality_factor, out output),
                8 => WebPEncodeBGRA_x64(bgra, width, height, stride, quality_factor, out output),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPEncodeLosslessBGR(IntPtr bgr, int width, int height, int stride, out IntPtr output)
        {
            return IntPtr.Size switch
            {
                4 => WebPEncodeLosslessBGR_x86(bgr, width, height, stride, out output),
                8 => WebPEncodeLosslessBGR_x64(bgr, width, height, stride, out output),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPEncodeLosslessBGRA(IntPtr bgra, int width, int height, int stride, out IntPtr output)
        {
            return IntPtr.Size switch
            {
                4 => WebPEncodeLosslessBGRA_x86(bgra, width, height, stride, out output),
                8 => WebPEncodeLosslessBGRA_x64(bgra, width, height, stride, out output),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static void WebPFree(IntPtr p)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    WebPFree_x86(p);
                    break;

                case 8:
                    WebPFree_x64(p);
                    break;

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        public static void WebPFreeDecBuffer(ref WebPDecBuffer buffer)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    WebPFreeDecBuffer_x86(ref buffer);
                    break;

                case 8:
                    WebPFreeDecBuffer_x64(ref buffer);
                    break;

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        public static int WebPGetDecoderVersion()
        {
            return IntPtr.Size switch
            {
                4 => WebPGetDecoderVersion_x86(),
                8 => WebPGetDecoderVersion_x64(),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static VP8StatusCode WebPGetFeatures(IntPtr rawWebP, int data_size, ref WebPBitstreamFeatures features)
        {
            return IntPtr.Size switch
            {
                4 => WebPGetFeaturesInternal_x86(rawWebP, (UIntPtr)data_size, ref features, WEBP_DECODER_ABI_VERSION),
                8 => WebPGetFeaturesInternal_x64(rawWebP, (UIntPtr)data_size, ref features, WEBP_DECODER_ABI_VERSION),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPGetInfo(IntPtr data, int data_size, out int width, out int height)
        {
            return IntPtr.Size switch
            {
                4 => WebPGetInfo_x86(data, (UIntPtr)data_size, out width, out height),
                8 => WebPGetInfo_x64(data, (UIntPtr)data_size, out width, out height),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPInitDecoderConfig(ref WebPDecoderConfig webPDecoderConfig)
        {
            return IntPtr.Size switch
            {
                4 => WebPInitDecoderConfigInternal_x86(ref webPDecoderConfig, WEBP_DECODER_ABI_VERSION),
                8 => WebPInitDecoderConfigInternal_x64(ref webPDecoderConfig, WEBP_DECODER_ABI_VERSION),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPPictureDistortion(ref WebPPicture srcPicture, ref WebPPicture refPicture, int metric_type, IntPtr pResult)
        {
            return IntPtr.Size switch
            {
                4 => WebPPictureDistortion_x86(ref srcPicture, ref refPicture, metric_type, pResult),
                8 => WebPPictureDistortion_x64(ref srcPicture, ref refPicture, metric_type, pResult),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static void WebPPictureFree(ref WebPPicture picture)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    WebPPictureFree_x86(ref picture);
                    break;

                case 8:
                    WebPPictureFree_x64(ref picture);
                    break;

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        public static int WebPPictureImportBGR(ref WebPPicture wpic, IntPtr bgr, int stride)
        {
            return IntPtr.Size switch
            {
                4 => WebPPictureImportBGR_x86(ref wpic, bgr, stride),
                8 => WebPPictureImportBGR_x64(ref wpic, bgr, stride),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPPictureImportBGRA(ref WebPPicture wpic, IntPtr bgra, int stride)
        {
            return IntPtr.Size switch
            {
                4 => WebPPictureImportBGRA_x86(ref wpic, bgra, stride),
                8 => WebPPictureImportBGRA_x64(ref wpic, bgra, stride),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPPictureImportBGRX(ref WebPPicture wpic, IntPtr bgr, int stride)
        {
            return IntPtr.Size switch
            {
                4 => WebPPictureImportBGRX_x86(ref wpic, bgr, stride),
                8 => WebPPictureImportBGRX_x64(ref wpic, bgr, stride),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPPictureInitInternal(ref WebPPicture wpic)
        {
            return IntPtr.Size switch
            {
                4 => WebPPictureInitInternal_x86(ref wpic, WEBP_DECODER_ABI_VERSION),
                8 => WebPPictureInitInternal_x64(ref wpic, WEBP_DECODER_ABI_VERSION),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        public static int WebPValidateConfig(ref WebPConfig config)
        {
            return IntPtr.Size switch
            {
                4 => WebPValidateConfig_x86(ref config),
                8 => WebPValidateConfig_x64(ref config),
                _ => throw new InvalidOperationException("Invalid platform. Can not find proper function"),
            };
        }

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPConfigInitInternal")]
        private static extern int WebPConfigInitInternal_x64(ref WebPConfig config, WebPPreset preset, float quality, int WEBP_DECODER_ABI_VERSION);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPConfigInitInternal")]
        private static extern int WebPConfigInitInternal_x86(ref WebPConfig config, WebPPreset preset, float quality, int WEBP_DECODER_ABI_VERSION);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPConfigLosslessPreset")]
        private static extern int WebPConfigLosslessPreset_x64(ref WebPConfig config, int level);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPConfigLosslessPreset")]
        private static extern int WebPConfigLosslessPreset_x86(ref WebPConfig config, int level);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecode")]
        private static extern VP8StatusCode WebPDecode_x64(IntPtr data, UIntPtr data_size, ref WebPDecoderConfig config);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecode")]
        private static extern VP8StatusCode WebPDecode_x86(IntPtr data, UIntPtr data_size, ref WebPDecoderConfig config);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRAInto")]
        private static extern int WebPDecodeBGRAInto_x64([In] IntPtr data, UIntPtr data_size, IntPtr output_buffer, int output_buffer_size, int output_stride);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRAInto")]
        private static extern int WebPDecodeBGRAInto_x86([In] IntPtr data, UIntPtr data_size, IntPtr output_buffer, int output_buffer_size, int output_stride);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRInto")]
        private static extern int WebPDecodeBGRInto_x64([In] IntPtr data, UIntPtr data_size, IntPtr output_buffer, int output_buffer_size, int output_stride);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPDecodeBGRInto")]
        private static extern int WebPDecodeBGRInto_x86([In] IntPtr data, UIntPtr data_size, IntPtr output_buffer, int output_buffer_size, int output_stride);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncode")]
        private static extern int WebPEncode_x64(ref WebPConfig config, ref WebPPicture picture);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncode")]
        private static extern int WebPEncode_x86(ref WebPConfig config, ref WebPPicture picture);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeBGR")]
        private static extern int WebPEncodeBGR_x64([In] IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeBGR")]
        private static extern int WebPEncodeBGR_x86([In] IntPtr bgr, int width, int height, int stride, float quality_factor, out IntPtr output);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeBGRA")]
        private static extern int WebPEncodeBGRA_x64([In] IntPtr bgra, int width, int height, int stride, float quality_factor, out IntPtr output);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeBGRA")]
        private static extern int WebPEncodeBGRA_x86([In] IntPtr bgra, int width, int height, int stride, float quality_factor, out IntPtr output);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeLosslessBGR")]
        private static extern int WebPEncodeLosslessBGR_x64([In] IntPtr bgr, int width, int height, int stride, out IntPtr output);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeLosslessBGR")]
        private static extern int WebPEncodeLosslessBGR_x86([In] IntPtr bgr, int width, int height, int stride, out IntPtr output);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeLosslessBGRA")]
        private static extern int WebPEncodeLosslessBGRA_x64([In] IntPtr bgra, int width, int height, int stride, out IntPtr output);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPEncodeLosslessBGRA")]
        private static extern int WebPEncodeLosslessBGRA_x86([In] IntPtr bgra, int width, int height, int stride, out IntPtr output);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPFree")]
        private static extern void WebPFree_x64(IntPtr p);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPFree")]
        private static extern void WebPFree_x86(IntPtr p);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPFreeDecBuffer")]
        private static extern void WebPFreeDecBuffer_x64(ref WebPDecBuffer buffer);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPFreeDecBuffer")]
        private static extern void WebPFreeDecBuffer_x86(ref WebPDecBuffer buffer);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetDecoderVersion")]
        private static extern int WebPGetDecoderVersion_x64();

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetDecoderVersion")]
        private static extern int WebPGetDecoderVersion_x86();

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetFeaturesInternal")]
        private static extern VP8StatusCode WebPGetFeaturesInternal_x64([In] IntPtr rawWebP, UIntPtr data_size, ref WebPBitstreamFeatures features, int WEBP_DECODER_ABI_VERSION);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetFeaturesInternal")]
        private static extern VP8StatusCode WebPGetFeaturesInternal_x86([In] IntPtr rawWebP, UIntPtr data_size, ref WebPBitstreamFeatures features, int WEBP_DECODER_ABI_VERSION);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetInfo")]
        private static extern int WebPGetInfo_x64([In] IntPtr data, UIntPtr data_size, out int width, out int height);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPGetInfo")]
        private static extern int WebPGetInfo_x86([In] IntPtr data, UIntPtr data_size, out int width, out int height);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPInitDecoderConfigInternal")]
        private static extern int WebPInitDecoderConfigInternal_x64(ref WebPDecoderConfig webPDecoderConfig, int WEBP_DECODER_ABI_VERSION);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPInitDecoderConfigInternal")]
        private static extern int WebPInitDecoderConfigInternal_x86(ref WebPDecoderConfig webPDecoderConfig, int WEBP_DECODER_ABI_VERSION);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureDistortion")]
        private static extern int WebPPictureDistortion_x64(ref WebPPicture srcPicture, ref WebPPicture refPicture, int metric_type, IntPtr pResult);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureDistortion")]
        private static extern int WebPPictureDistortion_x86(ref WebPPicture srcPicture, ref WebPPicture refPicture, int metric_type, IntPtr pResult);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureFree")]
        private static extern void WebPPictureFree_x64(ref WebPPicture wpic);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureFree")]
        private static extern void WebPPictureFree_x86(ref WebPPicture wpic);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportBGR")]
        private static extern int WebPPictureImportBGR_x64(ref WebPPicture wpic, IntPtr bgr, int stride);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportBGR")]
        private static extern int WebPPictureImportBGR_x86(ref WebPPicture wpic, IntPtr bgr, int stride);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportBGRA")]
        private static extern int WebPPictureImportBGRA_x64(ref WebPPicture wpic, IntPtr bgra, int stride);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportBGRA")]
        private static extern int WebPPictureImportBGRA_x86(ref WebPPicture wpic, IntPtr bgra, int stride);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportBGRX")]
        private static extern int WebPPictureImportBGRX_x64(ref WebPPicture wpic, IntPtr bgr, int stride);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureImportBGRX")]
        private static extern int WebPPictureImportBGRX_x86(ref WebPPicture wpic, IntPtr bgr, int stride);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureInitInternal")]
        private static extern int WebPPictureInitInternal_x64(ref WebPPicture wpic, int WEBP_DECODER_ABI_VERSION);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPPictureInitInternal")]
        private static extern int WebPPictureInitInternal_x86(ref WebPPicture wpic, int WEBP_DECODER_ABI_VERSION);

        [DllImport("libwebp_x64.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPValidateConfig")]
        private static extern int WebPValidateConfig_x64(ref WebPConfig config);

        [DllImport("libwebp_x86.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "WebPValidateConfig")]
        private static extern int WebPValidateConfig_x86(ref WebPConfig config);
    }

    #endregion | Import libwebp functions |

    #region | Predefined |

    public enum VP8StatusCode
    {
        VP8_STATUS_OK = 0,

        VP8_STATUS_OUT_OF_MEMORY,

        VP8_STATUS_INVALID_PARAM,

        VP8_STATUS_BITSTREAM_ERROR,

        VP8_STATUS_UNSUPPORTED_FEATURE,

        VP8_STATUS_SUSPENDED,

        VP8_STATUS_USER_ABORT,

        VP8_STATUS_NOT_ENOUGH_DATA
    }

    public enum WEBP_CSP_MODE
    {
        MODE_RGB = 0,

        MODE_RGBA = 1,

        MODE_BGR = 2,

        MODE_BGRA = 3,

        MODE_ARGB = 4,

        MODE_RGBA_4444 = 5,

        MODE_RGB_565 = 6,

        MODE_rgbA = 7,

        MODE_bgrA = 8,

        MODE_Argb = 9,

        MODE_rgbA_4444 = 10,

        MODE_YUV = 11,

        MODE_YUVA = 12,

        MODE_LAST = 13
    }

    public enum WebPEncodingError
    {
        VP8_ENC_OK = 0,

        VP8_ENC_ERROR_OUT_OF_MEMORY,

        VP8_ENC_ERROR_BITSTREAM_OUT_OF_MEMORY,

        VP8_ENC_ERROR_NULL_PARAMETER,

        VP8_ENC_ERROR_INVALID_CONFIGURATION,

        VP8_ENC_ERROR_BAD_DIMENSION,

        VP8_ENC_ERROR_PARTITION0_OVERFLOW,

        VP8_ENC_ERROR_PARTITION_OVERFLOW,

        VP8_ENC_ERROR_BAD_WRITE,

        VP8_ENC_ERROR_FILE_TOO_BIG,

        VP8_ENC_ERROR_USER_ABORT,

        VP8_ENC_ERROR_LAST
    }

    public enum WebPImageHint
    {
        WEBP_HINT_DEFAULT = 0,

        WEBP_HINT_PICTURE,

        WEBP_HINT_PHOTO,

        WEBP_HINT_GRAPH,

        WEBP_HINT_LAST
    }

    public enum WebPPreset
    {
        WEBP_PRESET_DEFAULT = 0,

        WEBP_PRESET_PICTURE,

        WEBP_PRESET_PHOTO,

        WEBP_PRESET_DRAWING,

        WEBP_PRESET_ICON,

        WEBP_PRESET_TEXT
    }

    internal enum DecState
    {
        STATE_WEBP_HEADER,

        STATE_VP8_HEADER,

        STATE_VP8_PARTS0,

        STATE_VP8_DATA,

        STATE_VP8L_HEADER,

        STATE_VP8L_DATA,

        STATE_DONE,

        STATE_ERROR
    }

    #endregion | Predefined |

    #region | libwebp structs |

    [StructLayout(LayoutKind.Explicit)]
    public struct RGBA_YUVA_Buffer
    {
        [FieldOffset(0)] public WebPRGBABuffer RGBA;

        [FieldOffset(0)] public WebPYUVABuffer YUVA;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPAuxStats
    {
        public int coded_size;

        public float PSNRY;

        public float PSNRU;

        public float PSNRV;

        public float PSNRALL;

        public float PSNRAlpha;

        public int block_count_intra4;

        public int block_count_intra16;

        public int block_count_skipped;

        public int header_bytes;

        public int mode_partition_0;

        public int residual_bytes_DC_segments0;

        public int residual_bytes_AC_segments0;

        public int residual_bytes_uv_segments0;

        public int residual_bytes_DC_segments1;

        public int residual_bytes_AC_segments1;

        public int residual_bytes_uv_segments1;

        public int residual_bytes_DC_segments2;

        public int residual_bytes_AC_segments2;

        public int residual_bytes_uv_segments2;

        public int residual_bytes_DC_segments3;

        public int residual_bytes_AC_segments3;

        public int residual_bytes_uv_segments3;

        public int segment_size_segments0;

        public int segment_size_segments1;

        public int segment_size_segments2;

        public int segment_size_segments3;

        public int segment_quant_segments0;

        public int segment_quant_segments1;

        public int segment_quant_segments2;

        public int segment_quant_segments3;

        public int segment_level_segments0;

        public int segment_level_segments1;

        public int segment_level_segments2;

        public int segment_level_segments3;

        public int alpha_data_size;

        public int layer_data_size;

        public int lossless_features;

        public int histogram_bits;

        public int transform_bits;

        public int cache_bits;

        public int palette_size;

        public int lossless_size;

        public int lossless_hdr_size;

        public int lossless_data_size;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
        private readonly uint[] pad;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPBitstreamFeatures
    {
        public int Width;

        public int Height;

        public int Has_alpha;

        public int Has_animation;

        public int Format;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5, ArraySubType = UnmanagedType.U4)]
        private readonly uint[] pad;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPConfig
    {
        public int lossless;

        public float quality;

        public int method;

        public WebPImageHint image_hint;

        public int target_size;

        public float target_PSNR;

        public int segments;

        public int sns_strength;

        public int filter_strength;

        public int filter_sharpness;

        public int filter_type;

        public int autofilter;

        public int alpha_compression;

        public int alpha_filtering;

        public int alpha_quality;

        public int pass;

        public int show_compressed;

        public int preprocessing;

        public int partitions;

        public int partition_limit;

        public int emulate_jpeg_size;

        public int thread_level;

        public int low_memory;

        public int near_lossless;

        public int exact;

        public int delta_palettization;

        public int use_sharp_yuv;

        private readonly int pad1;

        private readonly int pad2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPDecBuffer
    {
        public WEBP_CSP_MODE colorspace;

        public int width;

        public int height;

        public int is_external_memory;

        public RGBA_YUVA_Buffer u;

        private readonly uint pad1;

        private readonly uint pad2;

        private readonly uint pad3;

        private readonly uint pad4;

        public IntPtr private_memory;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPDecoderConfig
    {
        public WebPBitstreamFeatures input;

        public WebPDecBuffer output;

        public WebPDecoderOptions options;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPDecoderOptions
    {
        public int bypass_filtering;

        public int no_fancy_upsampling;

        public int use_cropping;

        public int crop_left;

        public int crop_top;

        public int crop_width;

        public int crop_height;

        public int use_scaling;

        public int scaled_width;

        public int scaled_height;

        public int use_threads;

        public int dithering_strength;

        public int flip;

        public int alpha_dithering_strength;

        private readonly uint pad1;

        private readonly uint pad2;

        private readonly uint pad3;

        private readonly uint pad4;

        private readonly uint pad5;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPPicture : IDisposable
    {
        public int use_argb;

        public uint colorspace;

        public int width;

        public int height;

        public IntPtr y;

        public IntPtr u;

        public IntPtr v;

        public int y_stride;

        public int uv_stride;

        public IntPtr a;

        public int a_stride;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
        private readonly uint[] pad1;

        public IntPtr argb;

        public int argb_stride;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.U4)]
        private readonly uint[] pad2;

        public IntPtr writer;

        public IntPtr custom_ptr;

        public int extra_info_type;

        public IntPtr extra_info;

        public IntPtr stats;

        public uint error_code;

        public IntPtr progress_hook;

        public IntPtr user_data;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13, ArraySubType = UnmanagedType.U4)]
        private readonly uint[] pad3;

        private readonly IntPtr memory_;

        private readonly IntPtr memory_argb_;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.U4)]
        private readonly uint[] pad4;

        public void Dispose() => throw new NotImplementedException();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPRGBABuffer
    {
        public IntPtr rgba;

        public int stride;

        public UIntPtr size;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WebPYUVABuffer
    {
        public IntPtr y;

        public IntPtr u;

        public IntPtr v;

        public IntPtr a;

        public int y_stride;

        public int u_stride;

        public int v_stride;

        public int a_stride;

        public UIntPtr y_size;

        public UIntPtr u_size;

        public UIntPtr v_size;

        public UIntPtr a_size;
    }

    #endregion | libwebp structs |
}