using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAlgorithms
{
    public class Material
    {
        //public string Name { get; private set; }
        public float[] Ka { get; set; } = new float[] {0.0f, 0.0f,0.0f};
        public float[] Kd { get; set; } = new float[] { 0.0f, 0.0f, 0.0f };
        public float[] Ks { get; set; } = new float[] { 0.0f, 0.0f, 0.0f };
        public float Alpha { get; set; } = 1.0f;
        public List<List<int>> Faces { get; } = new();
        public List<List<int>> TextFaces { get; private set; } = new();
        public Texture TextureParts { get; set; } = new();

    }
}
