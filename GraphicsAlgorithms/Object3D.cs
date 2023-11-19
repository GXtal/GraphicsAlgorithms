using System.Numerics;
using static System.Globalization.CultureInfo;

namespace GraphicsAlgorithms;

public class Object3D
{
    public List<Vector3> Vertexes { get; } = new();
    public List<List<int>> Faces { get; } = new();
    public List<List<float>> FacesColor { get; } = new();

    private Random _randomizer = new Random();
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public float RotationX { get; set; }
    public float RotationY { get; set; }
    public float RotationZ { get; set; }
    public float ScaleX { get; set; } = 1;
    public float ScaleY { get; set; } = 1;
    public float ScaleZ { get; set; } = 1;

    private Matrix4x4 CreateObserverMatrixB(Vector3 cameraPosition)
    {
        Vector3 target = new Vector3(0f, 0f, 0f);
        Vector3 up = new Vector3(0f, 1f, 0f);
        Vector3 zAxis = Vector3.Normalize(cameraPosition - target);
        Vector3 xAxis = Vector3.Normalize(Vector3.Cross(up, zAxis));
        Vector3 yAxis = up;

        return Matrix4x4.Transpose(new Matrix4x4(
            xAxis.X, xAxis.Y, xAxis.Z, -Vector3.Dot(xAxis, cameraPosition),
            yAxis.X, yAxis.Y, yAxis.Z, -Vector3.Dot(yAxis, cameraPosition),
            zAxis.X, zAxis.Y, zAxis.Z, -Vector3.Dot(zAxis, cameraPosition),
            0f, 0f, 0f, 1f));
    }

    public Matrix4x4 CreateWorldMatrix()
    {

        Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(PositionX, PositionY, PositionZ);

        Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(ScaleX, ScaleY, ScaleZ);

        Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationX(RotationX) *
                                   Matrix4x4.CreateRotationY(RotationY) *
                                   Matrix4x4.CreateRotationZ(RotationZ);

        Matrix4x4 worldMatrix = scaleMatrix * rotationMatrix * translationMatrix;


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
                ScaleX, 0f, 0f, 0f,
                0f, ScaleY, 0f, 0f,
                0f, 0f, ScaleZ, 0f,
                0f, 0f, 0f, 1.0f));

        Matrix4x4 rotationMatrix = Matrix4x4.CreateRotationX(RotationX) *
                                   Matrix4x4.CreateRotationY(RotationY) *
                                   Matrix4x4.CreateRotationZ(RotationZ);

        Matrix4x4 worldMatrix = translationMatrix * scaleMatrix * rotationMatrix;

        return worldMatrix;
    }

    public void LoadModel(string fileName)
    {
        var colorIndex = 0;
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

                    /*Faces.Add(face);
                    FacesColor.Add(new List<float>());
                    FacesColor[colorIndex].Add(0.1f);
                    FacesColor[colorIndex].Add(1f);
                    FacesColor[colorIndex].Add(0.0f);
                    colorIndex++;*/

                    List<List<int>> triangulated = EarClipping(face);

                    foreach (var polygon in triangulated)
                    {
                        FacesColor.Add(new List<float>());
                        FacesColor[colorIndex].Add(0.1f);
                        FacesColor[colorIndex].Add(1f);
                        FacesColor[colorIndex].Add(0.0f);
                        colorIndex++;
                    }

                    Faces.AddRange(triangulated);
                }
            }
        }
    }

    private bool IsCounterClockwise(List<int> polygon)
    {
        float signedArea = 0;
        for (int i = 0; i < polygon.Count; i++)
        {
            Vector3 current = Vertexes[polygon[i]];
            Vector3 next = Vertexes[polygon[(i + 1) % polygon.Count]];
            signedArea += (next.X - current.X) * (next.Y + current.Y);
        }

        if (signedArea < 0)
        {
            return false;
        }

        return true;
    }

    private List<List<int>> EarClipping(List<int> polygon)
    {
        List<List<int>> triangles = new List<List<int>>();

        while (polygon.Count > 3)
        {
            int earIndex = FindEar(polygon);
            if (earIndex == -1)
            {
                break;
            }

            List<int> triangle = new List<int>
            {
                polygon[(earIndex - 1 + polygon.Count) % polygon.Count],
                polygon[earIndex],
                polygon[(earIndex + 1) % polygon.Count]
            };

            triangles.Add(triangle);

            // Remove the ear vertex
            polygon.RemoveAt(earIndex);
        }

        // Add the last triangle
        triangles.Add(polygon);

        return triangles;
    }

    private bool IsEar(List<int> polygon, int earIndex)
    {
        int numVertices = polygon.Count;
        int prevIndex = (earIndex - 1 + numVertices) % numVertices;
        int nextIndex = (earIndex + 1) % numVertices;

        // Check if the angle at the ear candidate is convex
        Vector3 prevVector = Vertexes[polygon[earIndex]] - Vertexes[polygon[prevIndex]];
        Vector3 nextVector = Vertexes[polygon[nextIndex]] - Vertexes[polygon[earIndex]];



        Vector3 crossProduct = Vector3.Cross(prevVector, nextVector);

        return (crossProduct.Z >= 0 && !IsCounterClockwise(polygon))
            || (crossProduct.Z <= 0 && IsCounterClockwise(polygon)); // Assuming Z is up in your coordinate system
    }

    private int FindEar(List<int> polygon)
    {
        for (int i = 0; i < polygon.Count; i++)
        {
            if (IsEar(polygon, i))
            {
                return i;
            }
        }
        return -1;
    }
}
