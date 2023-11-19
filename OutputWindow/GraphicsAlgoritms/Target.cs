using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAlgorithms
{
    public class Target
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }

        public Vector3 GetPosition() {
            return new Vector3(PositionX, PositionY, PositionZ);
        }
    }
}
