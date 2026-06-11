using System;
using static Msdfgen.Arithmetics;

namespace Msdfgen
{
    public static class SdfRenderer
    {
        public static void RenderMSDF(Bitmap<float> output, Bitmap<float> msdf, Range pxRange, float sdThreshold = 0.5f)
        {
            double scaleX = (double)msdf.Width / output.Width;
            double scaleY = (double)msdf.Height / output.Height;
            
            double scaleFactor = (double)(output.Width + output.Height) / (msdf.Width + msdf.Height);
            Range scaledRange = pxRange * scaleFactor;
            DistanceMapping distanceMapping = DistanceMapping.Inverse(scaledRange);
            float sdBias = 0.5f - sdThreshold;
            
            for (int y = 0; y < output.Height; y++)
            {
                for (int x = 0; x < output.Width; x++)
                {
                    double srcX = (x + 0.5) * scaleX;
                    double srcY = (y + 0.5) * scaleY;
                    
                    float r, g, b;
                    Interpolate(msdf, srcX, srcY, out r, out g, out b);
                    
                    float sd = Median(r, g, b);
                    float opacity = (float)Math.Max(0.0, Math.Min(1.0, distanceMapping.Map(sd + sdBias) + 0.5));
                    
                    output[x, y, 0] = opacity;
                    if (output.Channels >= 3)
                    {
                        output[x, y, 1] = opacity;
                        output[x, y, 2] = opacity;
                    }
                }
            }
        }
        
        public static void RenderSDF(Bitmap<float> output, Bitmap<float> sdf, Range pxRange, float sdThreshold = 0.5f)
        {
            double scaleX = (double)sdf.Width / output.Width;
            double scaleY = (double)sdf.Height / output.Height;
            
            double scaleFactor = (double)(output.Width + output.Height) / (sdf.Width + sdf.Height);
            Range scaledRange = pxRange * scaleFactor;
            DistanceMapping distanceMapping = DistanceMapping.Inverse(scaledRange);
            float sdBias = 0.5f - sdThreshold;
            
            for (int y = 0; y < output.Height; y++)
            {
                for (int x = 0; x < output.Width; x++)
                {
                    double srcX = (x + 0.5) * scaleX;
                    double srcY = (y + 0.5) * scaleY;
                    
                    float sd = InterpolateSingle(sdf, srcX, srcY, 0);
                    float opacity = (float)Math.Max(0.0, Math.Min(1.0, distanceMapping.Map(sd + sdBias) + 0.5));
                    
                    output[x, y, 0] = opacity;
                    if (output.Channels >= 3)
                    {
                        output[x, y, 1] = opacity;
                        output[x, y, 2] = opacity;
                    }
                }
            }
        }

        private static int ClampInt(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }
        
        private static void Interpolate(Bitmap<float> sdf, double x, double y, out float r, out float g, out float b)
        {
            int x0 = (int)Math.Floor(x - 0.5);
            int y0 = (int)Math.Floor(y - 0.5);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            
            double fx = x - 0.5 - x0;
            double fy = y - 0.5 - y0;
            
            x0 = ClampInt(x0, 0, sdf.Width - 1);
            x1 = ClampInt(x1, 0, sdf.Width - 1);
            y0 = ClampInt(y0, 0, sdf.Height - 1);
            y1 = ClampInt(y1, 0, sdf.Height - 1);
            
            float r00 = sdf[x0, y0, 0], r10 = sdf[x1, y0, 0], r01 = sdf[x0, y1, 0], r11 = sdf[x1, y1, 0];
            float g00 = sdf[x0, y0, 1], g10 = sdf[x1, y0, 1], g01 = sdf[x0, y1, 1], g11 = sdf[x1, y1, 1];
            float b00 = sdf[x0, y0, 2], b10 = sdf[x1, y0, 2], b01 = sdf[x0, y1, 2], b11 = sdf[x1, y1, 2];
            
            r = (float)((1 - fx) * (1 - fy) * r00 + fx * (1 - fy) * r10 + (1 - fx) * fy * r01 + fx * fy * r11);
            g = (float)((1 - fx) * (1 - fy) * g00 + fx * (1 - fy) * g10 + (1 - fx) * fy * g01 + fx * fy * g11);
            b = (float)((1 - fx) * (1 - fy) * b00 + fx * (1 - fy) * b10 + (1 - fx) * fy * b01 + fx * fy * b11);
        }
        
        private static float InterpolateSingle(Bitmap<float> sdf, double x, double y, int channel)
        {
            int x0 = (int)Math.Floor(x - 0.5);
            int y0 = (int)Math.Floor(y - 0.5);
            int x1 = x0 + 1;
            int y1 = y0 + 1;
            
            double fx = x - 0.5 - x0;
            double fy = y - 0.5 - y0;
            
            x0 = ClampInt(x0, 0, sdf.Width - 1);
            x1 = ClampInt(x1, 0, sdf.Width - 1);
            y0 = ClampInt(y0, 0, sdf.Height - 1);
            y1 = ClampInt(y1, 0, sdf.Height - 1);
            
            float v00 = sdf[x0, y0, channel];
            float v10 = sdf[x1, y0, channel];
            float v01 = sdf[x0, y1, channel];
            float v11 = sdf[x1, y1, channel];
            
            return (float)((1 - fx) * (1 - fy) * v00 + fx * (1 - fy) * v10 + (1 - fx) * fy * v01 + fx * fy * v11);
        }
    }
}