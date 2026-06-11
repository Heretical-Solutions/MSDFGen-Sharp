using System;
using System.IO;
using SkiaSharp;
using Msdfgen;
using System.Threading.Tasks;

namespace MsdfAtlasGen.Cli
{
    /// <summary>
    /// Saves atlas bitmaps to image files.
    /// </summary>
    public static class AtlasSaver
    {
        public static void SaveAtlas(Bitmap<float> bitmap, string filename)
        {
            string? dir = Path.GetDirectoryName(filename);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            int width = bitmap.Width;
            int height = bitmap.Height;
            int channels = bitmap.Channels;
            var msdfPixels = bitmap.Pixels;

            using var skBitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Unpremul);

            unsafe
            {
                byte* dstPixels = (byte*)skBitmap.GetPixels().ToPointer();
                int dstStride = skBitmap.RowBytes;

                Parallel.For(0, height, y =>
                {
                    int srcY = height - 1 - y;
                    int srcRowOffset = channels * (width * srcY);
                    byte* dstRow = dstPixels + y * dstStride;

                    if (channels == 1)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            float v = msdfPixels[srcRowOffset + x];
                            byte b = Clamp(v * 255.0f);
                            int dstOffset = x * 4;
                            dstRow[dstOffset] = b;
                            dstRow[dstOffset + 1] = b;
                            dstRow[dstOffset + 2] = b;
                            dstRow[dstOffset + 3] = 255;
                        }
                    }
                    else if (channels == 3)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int srcOffset = srcRowOffset + (x * 3);
                            int dstOffset = x * 4;
                            dstRow[dstOffset] = Clamp(msdfPixels[srcOffset] * 255.0f);
                            dstRow[dstOffset + 1] = Clamp(msdfPixels[srcOffset + 1] * 255.0f);
                            dstRow[dstOffset + 2] = Clamp(msdfPixels[srcOffset + 2] * 255.0f);
                            dstRow[dstOffset + 3] = 255;
                        }
                    }
                    else if (channels == 4)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            int srcOffset = srcRowOffset + (x * 4);
                            int dstOffset = x * 4;
                            dstRow[dstOffset] = Clamp(msdfPixels[srcOffset] * 255.0f);
                            dstRow[dstOffset + 1] = Clamp(msdfPixels[srcOffset + 1] * 255.0f);
                            dstRow[dstOffset + 2] = Clamp(msdfPixels[srcOffset + 2] * 255.0f);
                            dstRow[dstOffset + 3] = Clamp(msdfPixels[srcOffset + 3] * 255.0f);
                        }
                    }
                });
            }

            using var image = SKImage.FromBitmap(skBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 80);
            using var stream = File.OpenWrite(filename);
            data.SaveTo(stream);
        }

        private static byte Clamp(float v)
        {
            if (v < 0) return 0;
            if (v > 255) return 255;
            return (byte)v;
        }
    }
}
