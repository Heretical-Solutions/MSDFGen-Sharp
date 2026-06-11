using System;
using System.IO;
using Msdfgen;
using Msdfgen.Extensions;
using System.Threading.Tasks;

namespace MsdfAtlasGen.Cli
{
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
            var rgba = new byte[width * height * 4];

            Parallel.For(0, height, y =>
            {
                int srcY = height - 1 - y;
                int srcRowOffset = channels * (width * srcY);
                int dstRow = y * width * 4;

                if (channels == 1)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte b = Clamp(msdfPixels[srcRowOffset + x] * 255.0f);
                        int i = dstRow + x * 4;
                        rgba[i] = b;
                        rgba[i + 1] = b;
                        rgba[i + 2] = b;
                        rgba[i + 3] = 255;
                    }
                }
                else if (channels == 3)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int srcOffset = srcRowOffset + (x * 3);
                        int i = dstRow + x * 4;
                        rgba[i] = Clamp(msdfPixels[srcOffset] * 255.0f);
                        rgba[i + 1] = Clamp(msdfPixels[srcOffset + 1] * 255.0f);
                        rgba[i + 2] = Clamp(msdfPixels[srcOffset + 2] * 255.0f);
                        rgba[i + 3] = 255;
                    }
                }
                else if (channels == 4)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int srcOffset = srcRowOffset + (x * 4);
                        int i = dstRow + x * 4;
                        rgba[i] = Clamp(msdfPixels[srcOffset] * 255.0f);
                        rgba[i + 1] = Clamp(msdfPixels[srcOffset + 1] * 255.0f);
                        rgba[i + 2] = Clamp(msdfPixels[srcOffset + 2] * 255.0f);
                        rgba[i + 3] = Clamp(msdfPixels[srcOffset + 3] * 255.0f);
                    }
                }
            });

            PngEncoder.WritePng(rgba, width, height, filename);
        }

        private static byte Clamp(float v)
        {
            if (v < 0) return 0;
            if (v > 255) return 255;
            return (byte)v;
        }
    }
}