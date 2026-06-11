using System;
using SkiaSharp;
using Msdfgen;

namespace Msdfgen.Extensions
{
    public static class ImageSaver
    {
        public static void Save(Bitmap<float> bitmap, string filename)
        {
            using var skBitmap = new SKBitmap(bitmap.Width, bitmap.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);

            unsafe
            {
                byte* dst = (byte*)skBitmap.GetPixels().ToPointer();
                int dstStride = skBitmap.RowBytes;

                for (int y = 0; y < bitmap.Height; y++)
                {
                    int srcY = bitmap.Height - 1 - y;
                    byte* row = dst + y * dstStride;

                    if (bitmap.Channels == 1)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            byte v = ClampPositive(bitmap[x, srcY, 0] * 255.0f);
                            int dstOffset = x * 4;
                            row[dstOffset] = v;
                            row[dstOffset + 1] = v;
                            row[dstOffset + 2] = v;
                            row[dstOffset + 3] = 255;
                        }
                    }
                    else if (bitmap.Channels == 3)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            int dstOffset = x * 4;
                            row[dstOffset] = ClampPositive(bitmap[x, srcY, 0] * 255.0f);
                            row[dstOffset + 1] = ClampPositive(bitmap[x, srcY, 1] * 255.0f);
                            row[dstOffset + 2] = ClampPositive(bitmap[x, srcY, 2] * 255.0f);
                            row[dstOffset + 3] = 255;
                        }
                    }
                    else if (bitmap.Channels == 4)
                    {
                        for (int x = 0; x < bitmap.Width; x++)
                        {
                            int dstOffset = x * 4;
                            row[dstOffset] = ClampPositive(bitmap[x, srcY, 0] * 255.0f);
                            row[dstOffset + 1] = ClampPositive(bitmap[x, srcY, 1] * 255.0f);
                            row[dstOffset + 2] = ClampPositive(bitmap[x, srcY, 2] * 255.0f);
                            row[dstOffset + 3] = ClampPositive(bitmap[x, srcY, 3] * 255.0f);
                        }
                    }
                }
            }

            using var image = SKImage.FromBitmap(skBitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = System.IO.File.OpenWrite(filename);
            data.SaveTo(stream);
        }

        private static byte ClampPositive(float v)
        {
            if (v < 0) return 0;
            if (v > 255) return 255;
            return (byte)v;
        }
    }
}
