using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAlgorithms
{
    public class Camera
    {
        public float Fov { get; set; } = (float)(Math.PI / 4);
        public float Znear { get; set; } = 1f;
        public float Zfar { get; set; } = 100.0f;
        public float Theta { get; private set; } = 0.0f;
        public float Phi { get; private set; } = (float)(Math.PI / 2);

        float distanceToTarget = 500.0f;
        float angleX = 0;
        float angleY = 0;


        public void changeDistance(float distance)
        {
            distanceToTarget += distance;
        }
        public void changeAngles(float dAngleX, float dAngleY)
        {
            angleX += dAngleX;
            angleY += dAngleY;
        }


        public Matrix4x4 CreateObserverMatrix(Vector3 target)
        {
            Vector3 up = new Vector3(-((float)Math.Cos(angleX)) * ((float)Math.Sin(angleY)), ((float)Math.Cos(angleY)), -((float)Math.Sin(angleX)) * ((float)Math.Sin(angleY)));
            Vector3 t = new Vector3(((float)Math.Cos(angleX)) * ((float)Math.Cos(angleY)), ((float)Math.Sin(angleY)), ((float)Math.Sin(angleX)) * ((float)Math.Cos(angleY)));
            t = Vector3.Normalize(t);
            var eye = target + t * distanceToTarget;
            return Matrix4x4.CreateLookAt(eye, target, up);
        }

        public Matrix4x4 CreateProjectionMatrix(float SreenRation)
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(Fov, SreenRation, Znear, Zfar);
        }
    }
}
