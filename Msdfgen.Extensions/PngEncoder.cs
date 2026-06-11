using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Msdfgen.Extensions
{
    public static class PngEncoder
    {
        public static void WritePng(byte[] rgbaPixels, int width, int height, string filename)
        {
            using var stream = File.OpenWrite(filename);
            WritePng(rgbaPixels, width, height, stream);
        }

        public static void WritePng(byte[] rgbaPixels, int width, int height, Stream stream)
        {
            var writer = new BinaryWriter(stream);
            writer.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });

            WriteChunk(writer, "IHDR", BuildIhdr(width, height));
            WriteChunk(writer, "IDAT", BuildIdat(rgbaPixels, width, height));
            WriteChunk(writer, "IEND", Array.Empty<byte>());
            writer.Flush();
        }

        public static byte[] EncodePng(byte[] rgbaPixels, int width, int height)
        {
            using var ms = new MemoryStream();
            WritePng(rgbaPixels, width, height, ms);
            return ms.ToArray();
        }

        private static byte[] BuildIhdr(int width, int height)
        {
            var data = new byte[13];
            WriteBigEndian(data, 0, width);
            WriteBigEndian(data, 4, height);
            data[8] = 8;
            data[9] = 6;
            data[10] = 0;
            data[11] = 0;
            data[12] = 0;
            return data;
        }

        private static byte[] BuildIdat(byte[] rgbaPixels, int width, int height)
        {
            int bytesPerRow = (width * 4) + 1;
            var filtered = new byte[bytesPerRow * height];
            for (int y = 0; y < height; y++)
            {
                int rowStart = y * bytesPerRow;
                filtered[rowStart] = 0;
                Buffer.BlockCopy(rgbaPixels, y * width * 4, filtered, rowStart + 1, width * 4);
            }

            using var compressedMs = new MemoryStream();
            using (var deflate = new DeflateStream(compressedMs, CompressionLevel.Optimal, true))
            {
                deflate.Write(filtered, 0, filtered.Length);
            }
            return compressedMs.ToArray();
        }

        private static void WriteChunk(BinaryWriter writer, string type, byte[] data)
        {
            writer.Write(FlipEndian((uint)data.Length));
            writer.Write(Encoding.ASCII.GetBytes(type));
            writer.Write(data);

            var crcInput = new byte[type.Length + data.Length];
            Encoding.ASCII.GetBytes(type, 0, type.Length, crcInput, 0);
            Buffer.BlockCopy(data, 0, crcInput, type.Length, data.Length);
            writer.Write(FlipEndian(Crc32(crcInput)));
        }

        private static void WriteBigEndian(byte[] buffer, int offset, int value)
        {
            buffer[offset] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)value;
        }

        private static uint FlipEndian(uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        private static uint Crc32(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                    crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
            }
            return crc ^ 0xFFFFFFFF;
        }
    }
}