using Pfim;
using Rasterization;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace GraphicsAlgorithms
{
    public class Texture
    {
        public Pbgra32Bitmap MapKa { get; set; }
        public Pbgra32Bitmap MapKd { get; set; }
        public Pbgra32Bitmap MapKs { get; set; }
        public Pbgra32Bitmap MapNormals { get; set; }

        private static PixelFormat PixelFormat(IImage image)
        {
            switch (image.Format)
            {
                case ImageFormat.Rgb24:
                    return PixelFormats.Bgr24;
                case ImageFormat.Rgba32:
                    return PixelFormats.Bgra32;
                case ImageFormat.Rgb8:
                    return PixelFormats.Gray8;
                case ImageFormat.R5g5b5a1:
                case ImageFormat.R5g5b5:
                    return PixelFormats.Bgr555;
                case ImageFormat.R5g6b5:
                    return PixelFormats.Bgr565;
                default:
                    throw new Exception($"Unable to convert {image.Format} to WPF PixelFormat");
            }
        }
        public static Pbgra32Bitmap GetBitmapFromFile(string path)
        {
            var ext = System.IO.Path.GetExtension(path).Trim();
            BitmapSource Bitmap;
            BitmapPalette bPalette = new(new List<System.Windows.Media.Color>() { Colors.Red, Colors.Green, Colors.Blue });
            if (ext == ".tga")
            {
                using (var image = Pfimage.FromFile(path))
                {
                    var handle = GCHandle.Alloc(image.Data, GCHandleType.Pinned);
                    var addr = handle.AddrOfPinnedObject();
                    Bitmap = BitmapSource.Create(image.Width, image.Height, 96.0, 96.0,
                        PixelFormat(image), null, addr, image.DataLen, image.Stride);
                    handle.Free();
                }
            }
            else
            {
                var p = new Uri(path, UriKind.RelativeOrAbsolute);
                Bitmap = new BitmapImage(p);
            }
            Bitmap = new FormatConvertedBitmap(Bitmap, PixelFormats.Pbgra32, bPalette, 0.0);

            return new Pbgra32Bitmap(Bitmap);
        }

        public Vector3 GetKaFragment(float y, float x)
        {
            x = x - MathF.Floor(x);
            y = y - MathF.Floor(y);

            var color = MapKa.GetPixel((int)(x * MapKa.PixelWidth), (int)((1 - y) * MapKa.PixelHeight));
            return color;
        }

        public Vector3 GetKdFragment(float y, float x)
        {
            x = x - MathF.Floor(x);
            y = y - MathF.Floor(y);

            var color = MapKd.GetPixel((int)(x * MapKd.PixelWidth), (int)((1 - y) * MapKd.PixelHeight));
            return color;
        }

        public Vector3 GetKsFragment(float y, float x)
        {
            x = x - MathF.Floor(x);
            y = y - MathF.Floor(y);

            var color = MapKs.GetPixel((int)(x * MapKs.PixelWidth), (int)((1 - y) * MapKs.PixelHeight));
            return color;
        }

        public Vector3 GetNormalFragment(float y, float x)
        {
            x = x - MathF.Floor(x);
            y = y - MathF.Floor(y);

            var color = MapNormals.GetPixel((int)(x * MapNormals.PixelWidth), (int)((1 - y) * MapNormals.PixelHeight));
            return new Vector3(
                    color.X * 2.0f - 1.0f,
                    color.Y * 2.0f - 1.0f,
                    color.Z * 2.0f - 1.0f
                );
        }
    }
}
