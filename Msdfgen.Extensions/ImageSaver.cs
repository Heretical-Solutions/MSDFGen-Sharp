using Msdfgen;

namespace Msdfgen.Extensions
{
    public static class ImageSaver
    {
        public static void Save(Bitmap<float> bitmap, string filename)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            var rgba = new byte[width * height * 4];

            for (int y = 0; y < height; y++)
            {
                int srcY = height - 1 - y;
                int dstRow = y * width * 4;

                if (bitmap.Channels == 1)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte v = Clamp(bitmap[x, srcY, 0] * 255.0f);
                        int i = dstRow + x * 4;
                        rgba[i] = v;
                        rgba[i + 1] = v;
                        rgba[i + 2] = v;
                        rgba[i + 3] = 255;
                    }
                }
                else if (bitmap.Channels == 3)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int i = dstRow + x * 4;
                        rgba[i] = Clamp(bitmap[x, srcY, 0] * 255.0f);
                        rgba[i + 1] = Clamp(bitmap[x, srcY, 1] * 255.0f);
                        rgba[i + 2] = Clamp(bitmap[x, srcY, 2] * 255.0f);
                        rgba[i + 3] = 255;
                    }
                }
                else if (bitmap.Channels == 4)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int i = dstRow + x * 4;
                        rgba[i] = Clamp(bitmap[x, srcY, 0] * 255.0f);
                        rgba[i + 1] = Clamp(bitmap[x, srcY, 1] * 255.0f);
                        rgba[i + 2] = Clamp(bitmap[x, srcY, 2] * 255.0f);
                        rgba[i + 3] = Clamp(bitmap[x, srcY, 3] * 255.0f);
                    }
                }
            }

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