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

        float distanceToTarget = 1500.0f;
        float angleY = 0;
        float angleX = (float)(Math.PI / 2);


        public void changeDistance(float distance)
        {
            distanceToTarget += distance;
        }
        public void changeAngles(float dAngleX, float dAngleY)
        {
            angleX += dAngleX;
            angleY += dAngleY;
        }

        public Vector3 getCameraPosition(Vector3 target)
        {
            Vector3 t = new Vector3(((float)Math.Cos(angleX)) * ((float)Math.Cos(angleY)), ((float)Math.Sin(angleY)), ((float)Math.Sin(angleX)) * ((float)Math.Cos(angleY)));
            return target + t * distanceToTarget;
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
