using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MOPS
{
    public class PicConverter
    {
        static Color[] White2 = new Color[2] { Color.FromArgb(0, 0, 0, 0), Color.FromArgb(255, 255, 255, 255) };
        static Color[] Black2 = new Color[2] { Color.FromArgb(0, 0, 0, 0), Color.FromArgb(255, 0, 0, 0) };

        //static Color[] Black4 = new Color[4] { Color.FromArgb(0, 0, 0, 0), Color.FromArgb(96, 0, 0, 0), Color.FromArgb(192, 0, 0, 0), Color.FromArgb(255, 0, 0, 0) };
        static Color[] White4 = new Color[4] { Color.FromArgb(0, 0, 0, 0), Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 255, 255, 255) };

        static Color[] Black16 = new Color[16]
        {
            Color.FromArgb(0, 0, 0, 0), Color.FromArgb(32, 0, 0, 0), Color.FromArgb(48, 0, 0, 0), Color.FromArgb(64, 0, 0, 0),
            Color.FromArgb(80, 0, 0, 0), Color.FromArgb(96, 0, 0, 0), Color.FromArgb(112, 0, 0, 0), Color.FromArgb(128, 0, 0, 0),
            Color.FromArgb(144, 0, 0, 0), Color.FromArgb(160, 0, 0, 0), Color.FromArgb(176, 0, 0, 0), Color.FromArgb(192, 0, 0, 0),
            Color.FromArgb(208, 0, 0, 0), Color.FromArgb(224, 0, 0, 0), Color.FromArgb(240, 0, 0, 0), Color.FromArgb(255, 0, 0, 0)
        };
        static Color[] White16 = new Color[16]
        {
            Color.FromArgb(0, 255, 255, 255), Color.FromArgb(32, 255, 255, 255), Color.FromArgb(48, 255, 255, 255), Color.FromArgb(64, 255, 255, 255),
            Color.FromArgb(80, 255, 255, 255), Color.FromArgb(96, 255, 255, 255), Color.FromArgb(112, 255, 255, 255), Color.FromArgb(128, 255, 255, 255),
            Color.FromArgb(144, 255, 255, 255), Color.FromArgb(160, 255, 255, 255), Color.FromArgb(176, 255, 255, 255), Color.FromArgb(192, 255, 255, 255),
            Color.FromArgb(208, 255, 255, 255), Color.FromArgb(224, 255, 255, 255), Color.FromArgb(240, 255, 255, 255), Color.FromArgb(255, 255, 255, 255)
        };


        private class PixelManager : IEquatable<PixelManager> //For counting BGRA32 pixels
        {
            public byte a = 0;
            public byte r = 0;
            public byte g = 0;
            public byte b = 0;
            public int count = 1;

            public PixelManager(byte r, byte g, byte b, byte a)
            {
                this.a = a;
                this.r = r;
                this.g = g;
                this.b = b;
            }

            public bool Equals(PixelManager val)
            {
                return this.r == val.r & this.g == val.g & this.b == val.b & this.a == val.a;
            }

            public static int PixelCounter(BitmapSource img)
            {
                List<PixelManager> pxls = new List<PixelManager>();
                // Calculate stride of source
                int stride = (img.PixelWidth * img.Format.BitsPerPixel + 7) / 8;
                // Create data array to hold source pixel data
                int length = stride * img.PixelHeight;
                byte[] data = new byte[length];
                // Copy source image pixels to the data array
                img.CopyPixels(data, stride, 0);
                for (int i = 0; i < length; i += 4)
                {
                    PixelManager n = new PixelManager(data[i], data[i + 1], data[i + 2], data[i + 3]);
                    if (pxls.Count == 0) pxls.Add(n);
                    else if (!pxls.Contains(n))
                    {
                        pxls.Add(n);
                        if (pxls.Count == 10) break;
                    }
                    else
                    {
                        for (int j = 0; j < pxls.Count; j++)
                        {
                            if (pxls[j].Equals(n))
                            {
                                pxls[j].count++;
                                break;
                            }
                        }
                    }
                }
                return pxls.Count;
            }
        }

        public BitmapSource ImageOptimize(BitmapSource img)
        {
            if (img == null) return img;
            if (img.Format == PixelFormats.Bgra32)
            {
                if (PixelManager.PixelCounter(img) == 2) return Bgra32_to_ind1(img);
                else if (PixelManager.PixelCounter(img) > 2) return Bgra32_to_ind4(img);
            }
            return img;
        }

        public BitmapSource[] ImageArrayOptimize(BitmapSource[] arr)
        {
            if (arr == null) return arr;
            BitmapSource[] result = new BitmapSource[arr.Length];
            if (arr[0].Format == PixelFormats.Bgra32)
            {
                if (PixelManager.PixelCounter(arr[0]) == 2)
                {
                    for (int i = 0; i < arr.Length; i++) result[i] = Bgra32_to_ind1(arr[i]);
                    return result;
                }
                else if (PixelManager.PixelCounter(arr[0]) > 2)
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


        public BitmapSource InvertPic(BitmapSource img)
        {
            if (img == null) return img;
            if (img.Format == PixelFormats.Indexed1) return Ind1ToWhite(img);
            else if (img.Format == PixelFormats.Indexed2) return Ind2ToWhite(img);
            else if (img.Format == PixelFormats.Indexed4) return Ind4ToWhite(img);
            else
            {
                if (img.Format != PixelFormats.Bgra32)
                {
                    FormatConvertedBitmap ConvertedPic = new FormatConvertedBitmap();
                    ConvertedPic.BeginInit();
                    ConvertedPic.Source = img;
                    ConvertedPic.DestinationFormat = PixelFormats.Bgra32;
                    ConvertedPic.DestinationPalette = BitmapPalettes.BlackAndWhiteTransparent;
                    ConvertedPic.EndInit();

                    img = ConvertedPic;
                }
                int stride = (img.PixelWidth * img.Format.BitsPerPixel + 7) / 8;
                int length = stride * img.PixelHeight;
                byte[] data = new byte[length];
                img.CopyPixels(data, stride, 0);

                if (img.Format.BitsPerPixel != 1)
                    for (int i = 0; i < length; i += 4)
                    {
                        data[i] = (byte)(255 - data[i]); //R
                        data[i + 1] = (byte)(255 - data[i + 1]); //G
                        data[i + 2] = (byte)(255 - data[i + 2]); //B
                                                                 //data[i + 3] = (byte)(255 - data[i + 3]); //A
                    }
                return BitmapSource.Create(img.PixelWidth, img.PixelHeight, img.DpiX, img.DpiY, img.Format, img.Palette, data, stride);
            }
        }

        private BitmapSource Ind1ToWhite(BitmapSource img)
        {
            int stride = (img.PixelWidth * img.Format.BitsPerPixel + 7) / 8;
            int length = stride * img.PixelHeight;
            byte[] data = new byte[length];
            img.CopyPixels(data, stride, 0);
            return BitmapSource.Create(img.PixelWidth, img.PixelHeight, img.DpiX, img.DpiY, PixelFormats.Indexed1, new BitmapPalette(White2), data, stride);
        }

        private BitmapSource Ind2ToWhite(BitmapSource img)
        {
            int stride = (img.PixelWidth * img.Format.BitsPerPixel + 7) / 8;
            int length = stride * img.PixelHeight;
            byte[] data = new byte[length];
            img.CopyPixels(data, stride, 0);
            return BitmapSource.Create(img.PixelWidth, img.PixelHeight, img.DpiX, img.DpiY, PixelFormats.Indexed2, new BitmapPalette(White4), data, stride);
        }

        private BitmapSource Ind4ToWhite(BitmapSource img)
        {
            int stride = (img.PixelWidth * img.Format.BitsPerPixel + 7) / 8;
            int length = stride * img.PixelHeight;
            byte[] data = new byte[length];
            img.CopyPixels(data, stride, 0);
            return BitmapSource.Create(img.PixelWidth, img.PixelHeight, img.DpiX, img.DpiY, PixelFormats.Indexed4, new BitmapPalette(White16), data, stride);
        }
    }
}
