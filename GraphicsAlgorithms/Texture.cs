using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAlgorithms
{
    public class Texture
    {
        public Bitmap MapKa { get; set; }
        public Bitmap MapKd { get; set; }
        public Bitmap MapKs { get; set; }
        public Bitmap MapNormals { get; set; }
        
        public Vector3 GetKaFragment(float y, float x)
        {
            x = x > 1.0f ? x - (int)x : x;
            y = y > 1.0f ? y - (int)y : y;
            var color = MapKa.GetPixel((int)(y * MapKa.Height), (int)(x * MapKa.Width));
            return new Vector3(
                    color.R / 256.0f,
                    color.G / 256.0f,
                    color.B / 256.0f
                );
        }

        public Vector3 GetKdFragment(float y, float x)
        {
            x = x > 1.0f ? x - (int)x : x;
            y = y > 1.0f ? y - (int)y : y;
            var color = MapKd.GetPixel((int)(y * MapKd.Height), (int)(x * MapKd.Width));
            return new Vector3(
                    color.R / 256.0f,
                    color.G / 256.0f,
                    color.B / 256.0f
                );
        }

        public Vector3 GetKsFragment(float y, float x)
        {
            x = x > 1.0f ? x - (int)x : x;
            y = y > 1.0f ? y - (int)y : y;
            var color = MapKs.GetPixel((int)(y * MapKs.Height), (int)(x * MapKs.Width));
            return new Vector3(
                    color.R / 256.0f,
                    color.G / 256.0f,
                    color.B / 256.0f
                );
        }

        public Vector3 GetNormalFragment(float y, float x)
        {
            x = x > 1.0f ? x - (int)x : x;
            y = y > 1.0f ? y - (int)y : y;
            var color = MapNormals.GetPixel((int)(y * MapNormals.Height), (int)(x * MapNormals.Width));
            return new Vector3(
                    (color.R / 256.0f) / 2.0f - 1.0f,
                    (color.G / 256.0f) / 2.0f - 1.0f,
                    (color.B / 256.0f) / 2.0f - 1.0f
                );
        }
    }
}
