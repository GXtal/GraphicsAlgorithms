using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAlgorithms
{
    public class LightSource
    {
        float distanceToTarget = 10.0f;
        float angleY = 0;
        float angleX = (float)(Math.PI / 2);
        public byte[] Color { get; private set; } =  new byte[3];


        public LightSource(byte r, byte g, byte b)
        {
            Color[0] = r;
            Color[1] = g;
            Color[2] = b;
        }

        public void changeDistance(float distance)
        {
            distanceToTarget += distance;
        }
        public void changeAngles(float dAngleX, float dAngleY)
        {
            angleX += dAngleX;
            angleY += dAngleY;
        }

        public Vector3 getLightPosition(Vector3 target)
        {
            Vector3 t = new Vector3(((float)Math.Cos(angleX)) * ((float)Math.Cos(angleY)), ((float)Math.Sin(angleY)), ((float)Math.Sin(angleX)) * ((float)Math.Cos(angleY)));
            return target + t * distanceToTarget;
        }
    }
}
