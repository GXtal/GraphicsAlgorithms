using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        //public float[] DiffuseColor { get; set; } =  new float[] { 0.588000f, 0.588000f, 0.588000f };
        //public float[] AmbientColor { get; set; } = new float[] { 0.588000f, 0.588000f, 0.588000f };
        //public float[] SpecularColor { get; set; } = new float[] { 1.0f, 1.0f, 1.0f };
        //public float AmbientIntensity { get; set; } = 0.0f;
        //public float DiffuseIntensity { get; set; } = 0.7f;
        public float SpecularIntensity { get; set; } = 1.0f;
        //public float SpecularAlpha { get; set; } = 5f;


        public Vector3 GetResultColor(float NL, float RV, List<Vector3> colorParts, float alpha, Material material)
        {
            var result = new Vector3();
            result.X = colorParts[3].X * 2 + colorParts[0].X * material.Ka[0] + colorParts[1].X * material.Kd[0] * NL+ colorParts[2].X * material.Ks[0] * (float)Math.Pow(RV, alpha);
            result.Y = colorParts[3].Y * 2 + colorParts[0].Y * material.Ka[1] + colorParts[1].Y * material.Kd[1] * NL+ colorParts[2].Y * material.Ks[1] * (float)Math.Pow(RV, alpha);
            result.Z = colorParts[3].Z * 2 + colorParts[0].Z * material.Ka[2] + colorParts[1].Z * material.Kd[2] * NL+ colorParts[2].Z * material.Ks[2] * (float)Math.Pow(RV, alpha);

            result.X = (result.X > 1.0f) ? 1.0f : result.X;
            result.Y= (result.Y > 1.0f) ? 1.0f : result.Y;
            result.Z = (result.Z > 1.0f) ? 1.0f : result.Z;

            result.X = (result.X < 0.0f) ? 0.0f : result.X;
            result.Y = (result.Y < 0.0f) ? 0.0f : result.Y;
            result.Z = (result.Z < 0.0f) ? 0.0f : result.Z;

            return result;
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
