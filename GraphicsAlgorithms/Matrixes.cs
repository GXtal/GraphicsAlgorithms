using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsAlgorithms
{
    public static class Matrixes
    {
        public static Matrix4x4 CreateObserverMatrix(Vector3 cameraPosition, Vector3 target)
        {
            Vector3 up = new Vector3(0f, 1f, 0f);
            return Matrix4x4.CreateLookAt(cameraPosition, target, up);
        }

        public static Matrix4x4 CreateProjectionMatrix(float CameraFov, float SreenRation, float camZNear, float camZFar)
        {
            return Matrix4x4.CreatePerspectiveFieldOfView(CameraFov, SreenRation, camZNear, camZFar);
        }

        public static Matrix4x4 CreateViewPortMatrix(float screenWidth, float screenHeight)
        {
            return Matrix4x4.Transpose(new Matrix4x4(
                screenWidth / 2, 0, 0, 0 + screenWidth / 2,
                0, -screenHeight / 2, 0, 0 + screenHeight / 2,
                0, 0, 1, 0,
                0, 0, 0, 1
                ));
        }
    }
}
