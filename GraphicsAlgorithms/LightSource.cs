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
        public float[] DiffuseColor { get; set; } =  new float[] { 1.0f, 1.0f, 1.0f};
        public float[] AmbientColor { get; set; } = new float[] { 0.5f, 0.3f, 0.4f};
        public float[] SpecularColor { get; set; } = new float[] { 1.0f, 1.0f, 1.0f };
        public float AmbientIntensity { get; set; } = 0.0f;
        public float DiffuseIntensity { get; set; } = 0.7f;
        public float SpecularIntensity { get; set; } = 1.0f;
        public float SpecularAlpha { get; set; } = 5f;


        public Vector3 GetResultColor(float NL, float RV)
        {
            var result = new Vector3();
            result.X = AmbientColor[0] * AmbientIntensity + DiffuseColor[0] * DiffuseIntensity * NL + SpecularColor[0] * SpecularIntensity * (float)Math.Pow(RV, SpecularAlpha);
            result.Y = AmbientColor[1] * AmbientIntensity + DiffuseColor[1] * DiffuseIntensity * NL + SpecularColor[1] * SpecularIntensity * (float)Math.Pow(RV, SpecularAlpha);
            result.Z = AmbientColor[2] * AmbientIntensity + DiffuseColor[2] * DiffuseIntensity * NL + SpecularColor[2] * SpecularIntensity * (float)Math.Pow(RV, SpecularAlpha);

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
