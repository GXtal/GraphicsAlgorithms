using GraphicsAlgorithms;
using Rasterization;
using System;
using System.Collections.Generic;
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

    Pbgra32Bitmap bitmap;
    Vector4[] windowVertices;
    //Vector3[] projectionVertices;

    float[][] zbuffer;
    bool[] faceCanBeDrawn;
    Vector3 eye = new Vector3(0, 0, 0);


    public void EvaluateWindowCoords(Object3D model) {
        var modelMatrix = model.CreateWorldMatrix();
        var viewMatrix = mainCamera.CreateObserverMatrix(target.GetPosition());
        var projectionMatrix = mainCamera.CreateProjectionMatrix(ScreenWidth / ScreenHeight);
        var modelViewProjectionMatrix = modelMatrix * viewMatrix * projectionMatrix;
        var viewPortMatrix = Matrixes.CreateViewPortMatrix(ScreenWidth, ScreenHeight);

        windowVertices = new Vector4[model.Vertexes.Count];
       // projectionVertices = new Vector3[model.Vertexes.Count];
        eye = mainCamera.getCameraPosition(target.GetPosition());
        var eyeTmp = Vector4.Transform(eye, modelViewProjectionMatrix);
        eyeTmp /= eyeTmp.W;
        eyeTmp = Vector4.Transform(eyeTmp, viewPortMatrix);
        eye.X = eyeTmp.X;
        eye.Y = eyeTmp.Y;
        eye.Z = eyeTmp.Z;
        for (int i = 0; i < windowVertices.Length; i++)
        {
            windowVertices[i] = Vector4.Transform(model.Vertexes[i], modelViewProjectionMatrix);
            windowVertices[i] /= windowVertices[i].W;
            //projectionVertices[i].X = windowVertices[i].X;
            //projectionVertices[i].Y = windowVertices[i].Y;
            //projectionVertices[i].Z = windowVertices[i].Z;
            windowVertices[i] = Vector4.Transform(windowVertices[i], viewPortMatrix);
        }
    }

    public bool ZbufferCanBeDrawn(int i, Vector3 eye)
    {
        var aDot = windowVertices[mainModel.Faces[i][0]];
        var bDot = windowVertices[mainModel.Faces[i][1]];
        var cDot = windowVertices[mainModel.Faces[i][2]];

        var CA = cDot - bDot;
        var BA = aDot - bDot;
        var denominator = (BA.X * CA.Y - BA.Y * CA.X);
        var eyeFromTarget = eye - target.GetPosition();
        if (denominator * eyeFromTarget.Z < 0)
        {
            faceCanBeDrawn[i] = false;
        }
        else
        {
            faceCanBeDrawn[i] = true;
        }

        return faceCanBeDrawn[i];
    }


    public float getNonLinearZValue(float depth)
    {
        return (1f / depth - 1f / mainCamera.Znear) / (1f / mainCamera.Zfar - 1f / mainCamera.Znear);
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

        var denominator = (BA.X * CA.Y - BA.Y * CA.X);
        for(var y = minY; y <= maxY; ++y)
        {
            for (var x = minX; x <= maxX; ++x)
            {
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

                //Check z-buffer
                var depth = aDot.Z * w + bDot.Z * u + cDot.Z * v;
                var nonLinearDepth = getNonLinearZValue(depth);
                if (nonLinearDepth < zbuffer[y][x])
                {
                    bitmap.SetPixel(x, y, new Vector3(mainModel.FacesColor[i][0], mainModel.FacesColor[i][1], mainModel.FacesColor[i][2]));
                    zbuffer[y][x] = nonLinearDepth;
                }
                
            } 
        }
    }


    public void DrawFace(int i)
    {
        for (var j = 0; j < mainModel.Faces[i].Count; j++)
        {
            var p1Index = mainModel.Faces[i][j];
            var p2Index = mainModel.Faces[i][(j + 1) % mainModel.Faces[i].Count];

            var dx = windowVertices[p2Index].X - windowVertices[p1Index].X;
            var dy = windowVertices[p2Index].Y - windowVertices[p1Index].Y;
            var steps = (Math.Abs(dx) > Math.Abs(dy)) ? Math.Abs(dx) : Math.Abs(dy);
            var xInc = dx / steps;
            var yInc = dy / steps;

            var x = windowVertices[p1Index].X;
            var y = windowVertices[p1Index].Y;

            for (var k = 0; k <= steps; k++)
            {

                int xPixel = (int)x;
                int yPixel = (int)y;
                if (xPixel < 0 || xPixel >= (int)ScreenWidth || yPixel < 0 || yPixel >= (int)ScreenHeight)
                {
                    continue;
                }
                bitmap.SetPixel(xPixel, yPixel, new Vector3(0, 0, 0));
                x += xInc;
                y += yInc;
            }
        }
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
        ScreenWidth = (float)MainGrid.ActualWidth;
        ScreenHeight = (float)MainGrid.ActualHeight;
        bitmap = new Pbgra32Bitmap((int)ScreenWidth, (int)ScreenHeight);
        MainImage.Source = bitmap.Source;
        MainImage.Source = bitmap.Source;
        target.PositionX = mainModel.PositionX;
        target.PositionY = mainModel.PositionY;
        target.PositionZ = mainModel.PositionZ;
        faceCanBeDrawn = new bool[mainModel.Faces.Count];
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
            mainCamera.changeAngles(MathF.PI / 300 * (float)delta.X, MathF.PI / 300 * (float)delta.Y);
            EvaluateWindowCoords(mainModel);
            DrawModel(windowVertices, mainModel);
            oldP = p;
        }
    }

    private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        mainCamera.changeDistance(e.Delta / 10f);
        EvaluateWindowCoords(mainModel);
        DrawModel(windowVertices, mainModel);
    }
}
