using System.Drawing;
using System.Numerics;

namespace GraphicsAlgorithms
{
    public class Texture
    {
        public Vector3[] MapKa { get; set; }
        public int MapKaHeight;
        public int MapKaWidth;
        public Vector3[] MapKd { get; set; }
        public int MapKdHeight;
        public int MapKdWidth;
        public Vector3[] MapKs { get; set; }
        public int MapKsHeight;
        public int MapKsWidth;
        public Vector3[] MapNormals { get; set; }
        public int MapNormalsHeight;
        public int MapNormalsWidth;

        public Vector3 GetKaFragment(float y, float x)
        {
            x = x > 1.0f ? x - (int)x : x;
            y = y > 1.0f ? y - (int)y : y;

            x = x < 0.0f ? x + 1 : x;
            y = y < 0.0f ? y + 1 : y;
            var height = MapKaHeight;
            var width = MapKaWidth;    
            var color = MapKa.GetPixel((int)(x * MapKaWidth), (int)(y * MapKaHeight));
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

            x = x < 0.0f ? x + 1 : x;
            y = y < 0.0f ? y + 1 : y;
            var color = MapKd.GetPixel((int)(x * MapKd.Width), (int)(y * MapKd.Height));
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

            x = x < 0.0f ? x + 1 : x;
            y = y < 0.0f ? y + 1 : y;
            var color = MapKs.GetPixel((int)(x * MapKs.Width), (int)(y * MapKs.Height));
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

            x = x < 0.0f ? x + 1 : x;
            y = y < 0.0f ? y + 1 : y;
            var color = MapNormals.GetPixel((int)(x * MapNormals.Width), (int)(y * MapNormals.Height));
            return new Vector3(
                    (color.R / 256.0f) / 2.0f - 1.0f,
                    (color.G / 256.0f) / 2.0f - 1.0f,
                    (color.B / 256.0f) / 2.0f - 1.0f
                );
        }
    }
}
