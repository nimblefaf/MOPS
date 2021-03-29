using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MOPS
{
    public class PicConverter
    {
        static Color[] Black2 = new Color[2] { Color.FromArgb(0, 0, 0, 0), Color.FromArgb(255, 0, 0, 0) };

        //static Color[] Black4 = new Color[4] { Color.FromArgb(0, 0, 0, 0), Color.FromArgb(96, 0, 0, 0), Color.FromArgb(192, 0, 0, 0), Color.FromArgb(255, 0, 0, 0) };

        static Color[] Black16 = new Color[16]
        {
            Color.FromArgb(0, 0, 0, 0), Color.FromArgb(32, 0, 0, 0), Color.FromArgb(48, 0, 0, 0), Color.FromArgb(64, 0, 0, 0),
            Color.FromArgb(80, 0, 0, 0), Color.FromArgb(96, 0, 0, 0), Color.FromArgb(112, 0, 0, 0), Color.FromArgb(128, 0, 0, 0),
            Color.FromArgb(144, 0, 0, 0), Color.FromArgb(160, 0, 0, 0), Color.FromArgb(176, 0, 0, 0), Color.FromArgb(192, 0, 0, 0),
            Color.FromArgb(208, 0, 0, 0), Color.FromArgb(224, 0, 0, 0), Color.FromArgb(240, 0, 0, 0), Color.FromArgb(255, 0, 0, 0)
        };

        /// <summary>
        /// Returns 'true' if Bgra32-formated image has only two colors
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private static bool IsImageBinaryColored(BitmapSource img)
        {
            List<Color> pxls = new List<Color>();
            // Calculate stride of source
            int stride = (img.PixelWidth * img.Format.BitsPerPixel + 7) / 8;
            // Create data array to hold source pixel data
            int length = stride * img.PixelHeight;
            byte[] data = new byte[length];
            // Copy source image pixels to the data array
            img.CopyPixels(data, stride, 0);
            for (int i = 0; i < length; i += 4)
            {
                Color n = Color.FromArgb(data[i + 3], data[i], data[i + 1], data[i + 2]);
                if (pxls.Count == 0) pxls.Add(n);
                else if (!pxls.Contains(n))
                {
                    pxls.Add(n);
                    if (pxls.Count > 2) return false;
                }
            }
            return true;
        }

        public BitmapSource ImageOptimize(BitmapSource img)
        {
            if (img == null) return img;
            if (img.Format == PixelFormats.Bgra32)
            {
                if (IsImageBinaryColored(img)) return Bgra32_to_ind1(img);
                else return Bgra32_to_ind4(img);
            }
            return img;
        }

        public BitmapSource[] ImageArrayOptimize(BitmapSource[] arr)
        {
            if (arr == null) return arr;
            BitmapSource[] result = new BitmapSource[arr.Length];
            if (arr[0].Format == PixelFormats.Bgra32)
            {
                if (IsImageBinaryColored(arr[0]))
                {
                    for (int i = 0; i < arr.Length; i++) result[i] = Bgra32_to_ind1(arr[i]);
                    return result;
                }
                else
                {
                    for (int i = 0; i < arr.Length; i++) result[i] = Bgra32_to_ind4(arr[i]);
                    return result;
                }
            }
            return arr;
        }

        private BitmapSource Bgra32_to_ind1(BitmapSource img)
        {
            // Calculate stride of source
            int stride = (img.PixelWidth * img.Format.BitsPerPixel + 7) / 8;
            // Create data array to hold source pixel data
            int length = stride * img.PixelHeight;
            byte[] data = new byte[length];
            // Copy source image pixels to the data array
            img.CopyPixels(data, stride, 0);

            int TarStride = (img.PixelWidth * PixelFormats.Indexed1.BitsPerPixel + 7) / 8;
            int TarLength = TarStride * img.PixelHeight;
            byte[] TData = new byte[TarLength];
            int count = 0;

            string ConvertedBits = "";
            for (int i = 0; i < length; i += 4)
            {
                if (data[i + 3] < 255) ConvertedBits += "0";
                else ConvertedBits += "1";
                if (ConvertedBits.Length == 8)
                {
                    TData[count] = (byte)Convert.ToInt32(ConvertedBits, 2);
                    ConvertedBits = "";
                    count++;
                }
            }

            return BitmapSource.Create(img.PixelWidth, img.PixelHeight, img.DpiX, img.DpiY, PixelFormats.Indexed1, new BitmapPalette(Black2), TData, TarStride);
        }
        private BitmapSource Bgra32_to_ind4(BitmapSource img)
        {
            // Calculate stride of source
            int stride = (img.PixelWidth * img.Format.BitsPerPixel + 7) / 8;
            // Create data array to hold source pixel data
            int length = stride * img.PixelHeight;
            byte[] data = new byte[length];
            // Copy source image pixels to the data array
            img.CopyPixels(data, stride, 0);

            int TarStride = (img.PixelWidth * PixelFormats.Indexed4.BitsPerPixel + 7) / 8;
            int TarLength = TarStride * img.PixelHeight;
            byte[] TData = new byte[TarLength];
            int count = 0;

            string ConvertedBits = "";
            for (int i = 0; i < length; i += 4)
            {
                if (data[i + 3] < 32) ConvertedBits += "0000";
                else if (data[i + 3] < 48) ConvertedBits += "0001";
                else if (data[i + 3] < 64) ConvertedBits += "0010";
                else if (data[i + 3] < 80) ConvertedBits += "0011";
                else if (data[i + 3] < 96) ConvertedBits += "0100";
                else if (data[i + 3] < 112) ConvertedBits += "0101";
                else if (data[i + 3] < 128) ConvertedBits += "0110";
                else if (data[i + 3] < 144) ConvertedBits += "0111";
                else if (data[i + 3] < 160) ConvertedBits += "1000";
                else if (data[i + 3] < 176) ConvertedBits += "1001";
                else if (data[i + 3] < 192) ConvertedBits += "1010";
                else if (data[i + 3] < 208) ConvertedBits += "1011";
                else if (data[i + 3] < 224) ConvertedBits += "1100";
                else if (data[i + 3] < 240) ConvertedBits += "1101";
                else if (data[i + 3] < 255) ConvertedBits += "1110";
                else ConvertedBits += "1111";
                if (ConvertedBits.Length == 8)
                {
                    TData[count] = (byte)Convert.ToInt32(ConvertedBits, 2);
                    ConvertedBits = "";
                    count++;
                }
            }
            BitmapSource result = BitmapSource.Create(img.PixelWidth, img.PixelHeight, img.DpiX, img.DpiY, PixelFormats.Indexed4, new BitmapPalette(Black16), TData, TarStride);
            return result;
        }
    }
}
