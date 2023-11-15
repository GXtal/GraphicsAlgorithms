﻿using GraphicsAlgorithms;
using Rasterization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OutputWindow;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    bool isMouseDown = false;
    Point oldP = new Point(0, 0);   
    float ScreenWidth { get; set; }
    float ScreenHeight { get; set; }

    Object3D mainModel = new Object3D();
    Camera mainCamera = new Camera();
    Target target = new Target();
    LightSource lightSource = new LightSource(0, 200, 0);

    Pbgra32Bitmap bitmap;
    Vector4[] windowVertices;
    Vector3[] worldVertices;

    float[][] zbuffer;
    bool[] faceCanBeDrawn;
    Vector3 eye = new Vector3(0, 0, 0);
    Vector3[] vn;
    int[] countvn;
    Matrix4x4 modelMatrix;
    bool isLightChange = false;


    public void EvaluateWindowCoords(Object3D model) {
        modelMatrix = model.CreateWorldMatrix();
        var viewMatrix = mainCamera.CreateObserverMatrix(target.GetPosition());
        var projectionMatrix = mainCamera.CreateProjectionMatrix(ScreenWidth / ScreenHeight);
        var modelViewProjectionMatrix = modelMatrix * viewMatrix * projectionMatrix;
        var viewPortMatrix = Matrixes.CreateViewPortMatrix(ScreenWidth, ScreenHeight);

        windowVertices = new Vector4[model.Vertexes.Count];
        worldVertices = new Vector3[model.Vertexes.Count];
        eye = mainCamera.getCameraPosition(target.GetPosition());
        for (int i = 0; i < windowVertices.Length; i++)
        {
            var tmp = Vector4.Transform(model.Vertexes[i], modelMatrix);
            worldVertices[i] = new Vector3(tmp.X, tmp.Y, tmp.Z);
            windowVertices[i] = Vector4.Transform(model.Vertexes[i], modelViewProjectionMatrix);
            windowVertices[i] /= windowVertices[i].W;
            windowVertices[i] = Vector4.Transform(windowVertices[i], viewPortMatrix);
        }
    }

    public bool ZbufferCanBeDrawn(int i, Vector3 eye)
    {
        var aDot = worldVertices[mainModel.Faces[i][0]];
        var bDot = worldVertices[mainModel.Faces[i][1]];
        var cDot = worldVertices[mainModel.Faces[i][2]];

        var CA = cDot - bDot;
        var BA = aDot - bDot;
        var denominator = Vector3.Cross(CA, BA);
        var eyeFromTarget = eye - target.GetPosition();
        if (Vector3.Dot(denominator, eyeFromTarget) < 0)
        {
            faceCanBeDrawn[i] = false;
        }
        else
        {
            faceCanBeDrawn[i] = true;
        }

        return faceCanBeDrawn[i];
    }


    public void RasterizationFace(int i)
    {
        var aDot = windowVertices[mainModel.Faces[i][0]];
        var bDot = windowVertices[mainModel.Faces[i][1]];
        var cDot = windowVertices[mainModel.Faces[i][2]];

        var minX = (int)Math.Min(Math.Min(aDot.X, bDot.X), cDot.X);
        var maxX = (int)Math.Max(Math.Max(aDot.X, bDot.X), cDot.X);
        var minY = (int)Math.Min(Math.Min(aDot.Y, bDot.Y), cDot.Y);
        var maxY = (int)Math.Max(Math.Max(aDot.Y, bDot.Y), cDot.Y);


        var CA = cDot - bDot;
        var BA = aDot - bDot;


        var normA = Vector3.Normalize(vn[mainModel.Faces[i][0]]);
        var normB = Vector3.Normalize(vn[mainModel.Faces[i][1]]);
        var normC = Vector3.Normalize(vn[mainModel.Faces[i][2]]);



        var totalCount = 0;
        var RightCount = 0;
        var denominator = Math.Abs((CA.X * BA.Y - CA.Y * BA.X));
        for(var y = minY; y <= maxY; ++y)
        {
            for (var x = minX; x <= maxX; ++x)
            {
                totalCount++;
                if (x < 0 || x >= (int)ScreenWidth || y < 0 || y >= (int)ScreenHeight)
                {
                    continue;
                }
                var P = new Vector4(x, y, 0, 1);
                var AP = aDot - P;
                var BP = bDot - P;
                var CP = cDot - P;

                var v = (BP.X * AP.Y - BP.Y * AP.X) / denominator;
                var u = (AP.X * CP.Y - AP.Y * CP.X) / denominator;
                var w = 1 - u - v;
                if (v < 0 || u < 0 || w < 0)
                {
                    continue;
                }

                RightCount++;

                //Check z-buffer
                var depth = aDot.Z * w + bDot.Z * u + cDot.Z * v;
                var nPoint = normA * w + normB * u + normC * v; 
                if (depth < zbuffer[y][x])
                {
                    var lightVector = lightSource.getLightPosition(new Vector3(0,0,0));
                    lightVector = Vector3.Normalize(-(lightVector));
                    var lightA = Vector3.Dot(nPoint, lightVector);
                    if (lightA < 0) lightA = 0;
                    var light = lightA;
                    bitmap.SetPixel(x, y, new Vector3(lightSource.Color[0] *  light, lightSource.Color[1] * light, lightSource.Color[2] * light));
                    zbuffer[y][x] = depth;
                }
                
            } 
        }

        Console.WriteLine(0);
    }

    public void DrawModel(Vector4[] windowVertices, Object3D obj)
    {
        for (var i = 0; i < bitmap.PixelHeight; ++i)
        {
            for (var j = 0; j < bitmap.PixelWidth; ++j)
            {
                bitmap.ClearPixel(j, i);
            }
        }

        bitmap.Source.Lock();
        for (var i = 0; i < zbuffer.Length; i++)
        {
            for (var j = 0; j < (int)ScreenWidth; ++j)
            {
                zbuffer[i][j] = int.MaxValue;
            }
        }
        for (var i = 0; i < mainModel.Faces.Count; ++i)
        {
            if (ZbufferCanBeDrawn(i, eye))
            {
                RasterizationFace(i);
            }
        }

        //Parallel.For(0, obj.Faces.Count, RasterizationFace); //was changed
        bitmap.Source.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
        bitmap.Source.Unlock();
    }

    public MainWindow()
    {
        InitializeComponent();
        //C:\\Users\\admin\\Desktop\\ObjDrawer\\ObjDrawer\\data\\Torque Twister\\Torque Twister.obj
        //"C:\\Users\\admin\\Desktop\\Toilet.obj"
        //"C:\\Users\\admin\\Desktop\\ObjDrawer\\ObjDrawer\\data\\HardshellTransformer\\Hardshell.obj"
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.I:
                mainModel.RotationX += 0.1f;
                break;
            case Key.O:
                mainModel.RotationY += 0.1f;
                break;
            case Key.P:
                mainModel.RotationZ += 0.1f;
                break;
            case Key.N:
                mainModel.ScaleX += 0.1f;
                mainModel.ScaleY += 0.1f;
                mainModel.ScaleZ += 0.1f;
                break;
            case Key.M:
                mainModel.ScaleX -= 0.1f;
                mainModel.ScaleY -= 0.1f;
                mainModel.ScaleZ -= 0.1f;
                break;
            case Key.Right:
                target.PositionX += 5;
                break;
            case Key.Left:
                target.PositionX -= 5;
                break;
            case Key.Up:
                target.PositionY += 5;
                break;
            case Key.Down:
                target.PositionY -= 5;
                break;
            default:
                break;
        }

        EvaluateWindowCoords(mainModel);
        DrawModel(windowVertices, mainModel);
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        mainModel.LoadModel("C:\\Users\\admin\\Desktop\\ObjDrawer\\ObjDrawer\\data\\HardshellTransformer\\Hardshell.obj");
        //mainModel.LoadModel("C:\\Users\\admin\\Desktop\\sadds\\plane.obj");
        ScreenWidth = (float)MainGrid.ActualWidth;
        ScreenHeight = (float)MainGrid.ActualHeight;
        bitmap = new Pbgra32Bitmap((int)ScreenWidth, (int)ScreenHeight);
        MainImage.Source = bitmap.Source;
        MainImage.Source = bitmap.Source;
        target.PositionX = mainModel.PositionX;
        target.PositionY = mainModel.PositionY;
        target.PositionZ = mainModel.PositionZ;
        faceCanBeDrawn = new bool[mainModel.Faces.Count];

        vn = new Vector3[mainModel.Vertexes.Count];
        for (var i = 0; i < mainModel.Faces.Count; ++i)
        {
            var aDotWorld = mainModel.Vertexes[mainModel.Faces[i][0]];
            var bDotWorld = mainModel.Vertexes[mainModel.Faces[i][1]];
            var cDotWorld = mainModel.Vertexes[mainModel.Faces[i][2]];

            var CAWorld = cDotWorld - aDotWorld;
            var BAWorld = bDotWorld - aDotWorld;

            var CBWorld = cDotWorld - bDotWorld;
            var ABWorld = aDotWorld - bDotWorld;

            var BCWorld = bDotWorld - cDotWorld;
            var ACWorld = aDotWorld - cDotWorld;

            var normA = Vector3.Normalize(Vector3.Cross(CAWorld, BAWorld));
            var normB = Vector3.Normalize(Vector3.Cross(ABWorld, CBWorld));
            var normC = Vector3.Normalize(Vector3.Cross(BCWorld, ACWorld));

            vn[mainModel.Faces[i][0]] += normA;
            vn[mainModel.Faces[i][1]] += normB;
            vn[mainModel.Faces[i][2]] += normC;
        }

        for (var i = 0; i < vn.Length; i++)
        {
            vn[i] = Vector3.Normalize(vn[i]);
        }

        zbuffer = new float[(int)ScreenHeight][];
        for (var i = 0; i < zbuffer.Length; i++)
        {
            zbuffer[i] = new float[(int)ScreenWidth];
            for (var j = 0; j < (int)ScreenWidth; ++j)
            {
                zbuffer[i][j] = float.MaxValue;
            }   
        }
        EvaluateWindowCoords(mainModel);
        DrawModel(windowVertices, mainModel);
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        ScreenWidth = (float)MainGrid.ActualWidth;
        ScreenHeight = (float)MainGrid.ActualHeight;
        bitmap = new Pbgra32Bitmap((int)ScreenWidth, (int)ScreenHeight);
        MainImage.Source = bitmap.Source;
        target.PositionX = mainModel.PositionX;
        target.PositionY = mainModel.PositionY;
        target.PositionZ = mainModel.PositionZ;
        zbuffer = new float[(int)ScreenHeight][];
        for (var i = 0; i < zbuffer.Length; i++)
        {
            zbuffer[i] = new float[(int)ScreenWidth];
            for (var j = 0; j < (int)ScreenWidth; ++j)
            {
                zbuffer[i][j] = float.MaxValue;
            }
        }
        EvaluateWindowCoords(mainModel);
        DrawModel(windowVertices, mainModel);
    }

    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle)
        {
            isLightChange = (isLightChange) ? false : true;
        }

        isMouseDown = true;
        var p = e.GetPosition(MainImage);
        oldP = p; 
    }

    private void Window_MouseUp(object sender, MouseButtonEventArgs e)
    {
        isMouseDown = false;
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {

        if (isMouseDown)
        {
            var p = e.GetPosition(MainImage);
            var delta = p - oldP;
            if (!isLightChange)
            {
                mainCamera.changeAngles(MathF.PI / 300 * (float)delta.X, MathF.PI / 300 * (float)delta.Y);
            }
            else
            {
                lightSource.changeAngles(MathF.PI / 300 * (float)delta.X, MathF.PI / 300 * (float)delta.Y);
            }
            EvaluateWindowCoords(mainModel);
            DrawModel(windowVertices, mainModel);
            oldP = p;
        }
    }

    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (!isLightChange)
        {
            mainCamera.changeDistance(e.Delta / 10f);
        }
        else
        {
            lightSource.changeDistance(e.Delta / 10f);
        }
        
        EvaluateWindowCoords(mainModel);
        DrawModel(windowVertices, mainModel);
    }
}
