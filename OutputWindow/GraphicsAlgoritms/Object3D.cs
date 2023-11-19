using System.Numerics;
using static System.Globalization.CultureInfo;
using System.Drawing;
using System.Drawing.Imaging;
using Pfim;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System;
using System.IO;

namespace GraphicsAlgorithms;

public class Object3D
{
    private string pathToFile = @"C:\Users\admin\Desktop\ObjDrawer\ObjDrawer\data\HardshellTransformer";
    

    private string _lastMaterialName = "";
    public string MaterialsPath { get; private set; }
    public Dictionary<String, Material> materials { get; private set; } = new();
    public List<Vector3> Vertexes { get; } = new();
    public List<Vector3> TextVertexes { get; } = new();
    public int FacesCount { get; private set; } = 0;


    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public  float PositionZ { get; set; }
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
            xAxis.X,    xAxis.Y,    xAxis.Z,    -Vector3.Dot(xAxis, cameraPosition),
            yAxis.X,    yAxis.Y,    yAxis.Z,    -Vector3.Dot(yAxis, cameraPosition),
            zAxis.X,    zAxis.Y,    zAxis.Z,    -Vector3.Dot(zAxis, cameraPosition),
            0f,         0f,         0f,         1f));
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

    


    private void LoadPolygonData()
    {
        var curName = "";
        foreach (var line in File.ReadLines(pathToFile + "\\" +MaterialsPath))
        {
            string[] args = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (args.Length > 0)
            {
                if (args[0][0] == '\t')
                {
                    args[0] = args[0].Remove(0, 1);
                }
                switch (args[0])
                {

                    case "newmtl":
                        curName = args[1];
                        break;
                    case "Ka":
                        float x = float.Parse(args[1], InvariantCulture);
                        float y = float.Parse(args[2], InvariantCulture);
                        float z = float.Parse(args[3], InvariantCulture);
                        materials[curName].Ka = new float[] { x, y, z }; 
                        break;
                    case "Kd":
                        x = float.Parse(args[1], InvariantCulture);
                        y = float.Parse(args[2], InvariantCulture);
                        z = float.Parse(args[3], InvariantCulture);
                        materials[curName].Kd = new float[] { x, y, z };
                        break;
                    case "Ks":
                        x = float.Parse(args[1], InvariantCulture);
                        y = float.Parse(args[2], InvariantCulture);
                        z = float.Parse(args[3], InvariantCulture);
                        materials[curName].Ks = new float[] { x, y, z };
                        break;
                    case "Ns":
                        materials[curName].Alpha = float.Parse(args[1], InvariantCulture);
                        break;
                    case "map_Ka":
                        var fileName = pathToFile + "\\" + args[1] ;
                        var tgaImage = Texture.GetBitmapFromFile(fileName);
                        materials[curName].TextureParts.MapKa = tgaImage;
                        break;
                    case "map_Kd":
                        fileName = pathToFile + "\\" + args[1];
                        tgaImage = Texture.GetBitmapFromFile(fileName);
                        materials[curName].TextureParts.MapKd = tgaImage;   
                        break;
                    case "map_Ks":
                        fileName = pathToFile + "\\" + args[1];
                        tgaImage = Texture.GetBitmapFromFile(fileName);
                        materials[curName].TextureParts.MapKs = tgaImage;
                        break;
                    case "map_bump":
                        fileName = pathToFile + "\\" + args[1];
                        tgaImage = Texture.GetBitmapFromFile(fileName);
                        materials[curName].TextureParts.MapNormals = tgaImage;
                        break;
                }
            }

        }
    }
    public void LoadModel(string fileName)
    {
        var colorIndex = 0;
        pathToFile = fileName.Remove(fileName.LastIndexOf('\\'));
        foreach (string line in File.ReadLines(fileName))
        {
            string[] args = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "mtllib":
                        MaterialsPath = args[1];
                        break;
                    case "v":
                        float x = float.Parse(args[1], InvariantCulture);
                        float y = float.Parse(args[2], InvariantCulture);
                        float z = float.Parse(args[3], InvariantCulture);
                        Vertexes.Add(new(x, y, z));
                        break;
                    case "vt":
                        x = float.Parse(args[1], InvariantCulture);
                        y = float.Parse(args[2], InvariantCulture);
                        z = float.Parse(args[3], InvariantCulture);
                        TextVertexes.Add(new(x, y, z));
                        break;
                    case "usemtl":
                        _lastMaterialName = args[1];
                        if (!materials.ContainsKey(_lastMaterialName))
                            materials.Add(_lastMaterialName, new Material());
                        break;
                    case "f":
                        List<int> face = new List<int>();
                        List<int> textFace = new List<int>();

                        for (int i = 1; i < args.Length; i++)
                        {
                            string[] indexes = args[i].Split('/');
                            face.Add(int.Parse(indexes[0]) - 1);
                            textFace.Add(int.Parse(indexes[1]) - 1);
                            FacesCount++;
                        }
                        materials[_lastMaterialName].Faces.Add(face);
                        materials[_lastMaterialName].TextFaces.Add(face);
                        colorIndex++;
                        break;
                }

            }
        }
        LoadPolygonData();
        Console.Write(0);
    }
}
