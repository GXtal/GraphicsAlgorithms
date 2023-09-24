using System.Numerics;
using System.Collections.Concurrent;
using static System.Globalization.CultureInfo;
using System.Reflection;

namespace GraphicsAlgorithms;

public class Object3D
{
    List<Vector3> Vertexes { get; set; } = new();
    List<List<int>> Faces { get; set; } = new();

    float PositionX { get; set; }
    float PositionY { get; set; }
    float PositionZ { get; set; }
    float RotationX { get; set; }
    float RotationY { get; set; }
    float RotationZ { get; set; }
    float ScaleX { get; set; }
    float ScaleY { get; set; }
    float ScaleZ { get; set; }

    private Matrix4x4 CreateObserverMatrixB(Vector3 cameraPosition)
    {              
        Vector3 target = new Vector3(0f, 0f, 0f);
        Vector3 up = new Vector3(0f, 1f, 0f);
        Vector3 zAxis = Vector3.Normalize(cameraPosition - target);
        Vector3 xAxis = Vector3.Normalize(Vector3.Cross(up, zAxis));
        Vector3 yAxis = up;

        return Matrix4x4.Transpose(new Matrix4x4(
            xAxis.X,    xAxis.Y,    xAxis.Z,    -Vector3.Dot(xAxis, cameraPosition),
            yAxis.X,    yAxis.Y,    yAxis.Z,    -Vector3.Dot(yAxis, cameraPosition),
            zAxis.X,    zAxis.Y,    zAxis.Z,    -Vector3.Dot(zAxis, cameraPosition),
            0f,         0f,         0f,         1f));
    }

    private Matrix4x4 CreateObserverMatrix(Vector3 cameraPosition)
    {
        Vector3 target = new Vector3(0f, 0f, 0f);
        Vector3 up = new Vector3(0f, 1f, 0f);

        return Matrix4x4.CreateLookAt(cameraPosition, target, up);        
    }

    private Matrix4x4 CreateWorldMatrix()
    {

        Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(PositionX, PositionY, PositionZ);

        Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(ScaleX, ScaleY, ScaleZ);

        Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationX(RotationX) *
                                   Matrix4x4.CreateRotationY(RotationY) *
                                   Matrix4x4.CreateRotationZ(RotationZ);
        
        Matrix4x4 worldMatrix = translationMatrix * scaleMatrix * rotationMatrix;


        return worldMatrix;
    }

    private Matrix4x4 CreateWorldMatrixB()
    {
        Matrix4x4 translationMatrix = Matrix4x4.Transpose(new Matrix4x4(
                1f, 0f, 0f, PositionX,
                0f, 1f, 0f, PositionY,
                0f, 0f, 1f, PositionZ,
                0f, 0f, 0f, 1f));

        Matrix4x4 scaleMatrix = Matrix4x4.Transpose(new Matrix4x4(
                ScaleX, 0f,     0f,     0f,
                0f,     ScaleY, 0f,     0f,
                0f,     0f,     ScaleZ, 0f,
                0f,     0f,     0f,     1.0f));

        Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationX(RotationX) *
                                   Matrix4x4.CreateRotationY(RotationY) *
                                   Matrix4x4.CreateRotationZ(RotationZ);

        Matrix4x4 worldMatrix = translationMatrix * scaleMatrix * rotationMatrix;

        return worldMatrix;
    }

    public void LoadModel(string fileName)
    {
        foreach (string line in File.ReadLines(fileName))
        {
            string[] args = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (args.Length > 0)
            {
                if (args[0] == "v")
                {
                    float x = float.Parse(args[1], InvariantCulture);
                    float y = float.Parse(args[2], InvariantCulture);
                    float z = float.Parse(args[3], InvariantCulture);
                    Vertexes.Add(new(x, y, z));
                }
                else if (args[0] == "f")
                {
                    List<int> face = new List<int>();
                    for (int i = 1; i < args.Length; i++)
                    {
                        string[] indexes = args[i].Split('/');
                        face.Add(int.Parse(indexes[0]) - 1);
                    }
                    Faces.Add(face);
                }
            }
        }
    }
}
