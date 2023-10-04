using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAlgorithms
{
    public class Camera
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; } = 500;
        public float Fov { get; set; } = (float)(Math.PI / 4);
        public float Znear { get; set; } = 1f;
        public float Zfar { get; set; } = 100.0f;

        public Matrix4x4 CreateObserverMatrix(Vector3 target)
        {
            Vector3 up = new Vector3(0f, 1f, 0f);
            return Matrix4x4.CreateLookAt(new Vector3(PositionX, PositionY, PositionZ), target, up);
        }

        public Matrix4x4 CreateProjectionMatrix(float SreenRation)
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(Fov, SreenRation, Znear, Zfar);
        }
    }
}
