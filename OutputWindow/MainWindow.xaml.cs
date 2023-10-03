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
    float ScreenWidth { get; set; }
    float ScreenHeight { get; set; }

    float CameraPosX { get; set; }
    float CameraPosY { get; set; }
    float CameraPosZ { get; set; } = 500;
    float CameraFov { get; set; } = (float)(Math.PI / 4);
    float CameraZNear { get; set; } = 1f;
    float CameraZFar { get; set; } = 100.0f;


    float TargetPosX { get; set; }
    float TargerPosY { get; set; }
    float TargetPosZ { get; set; } = 0.0f;

    Object3D object1 = new Object3D();

    Pbgra32Bitmap bitmap;
    Vector4[] windowVertices;



    public MainWindow()
    {
        InitializeComponent();
        //C:\\Users\\admin\\Desktop\\ObjDrawer\\ObjDrawer\\data\\Torque Twister\\Torque Twister.obj
        //"C:\\Users\\admin\\Desktop\\Toilet.obj"
        //"C:\\Users\\admin\\Desktop\\ObjDrawer\\ObjDrawer\\data\\HardshellTransformer\\Hardshell.obj"
        object1.LoadModel("C:\\Users\\admin\\Desktop\\ObjDrawer\\ObjDrawer\\data\\HardshellTransformer\\Hardshell.obj");
        ScreenWidth = (float)this.Width;
        ScreenHeight = (float)this.Height;
        bitmap = new Pbgra32Bitmap((int)ScreenWidth, (int)ScreenHeight);
        MainImage.Source = bitmap.Source;
        TargetPosX = object1.PositionX;
        TargerPosY = object1.PositionY;
        TargetPosZ = object1.PositionZ;
        var modelMatrix = object1.CreateWorldMatrix();
        var viewMatrix = Matrixes.CreateObserverMatrix(new Vector3(CameraPosX, CameraPosY, CameraPosZ), new Vector3(TargetPosX, TargerPosY, TargetPosZ));
        var projectionMatrix = Matrixes.CreateProjectionMatrix(CameraFov, ScreenWidth / ScreenHeight, CameraZNear, CameraZFar);
        var modelViewProjectionMatrix = modelMatrix * viewMatrix * projectionMatrix;
        var viewPortMatrix = Matrixes.CreateViewPortMatrix(ScreenWidth, ScreenHeight);

        windowVertices = new Vector4[object1.Vertexes.Count];
        for (int i = 0; i < windowVertices.Length; i++)
        {
            windowVertices[i] = Vector4.Transform(object1.Vertexes[i], modelViewProjectionMatrix);
            windowVertices[i] /= windowVertices[i].W;
            windowVertices[i] = Vector4.Transform(windowVertices[i], viewPortMatrix);
        }

        DrawModel(windowVertices, object1);
    }


    public void DrawFace(int i)
    {
        for (var j = 0; j < object1.Faces[i].Count; j++)
            {
                var p1Index = object1.Faces[i][j];
                var p2Index = object1.Faces[i][(j + 1) % object1.Faces[i].Count];

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
                    if (xPixel  < 0 || xPixel >= ScreenWidth || yPixel < 0 || yPixel >= ScreenHeight)
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
        Parallel.For(0, obj.Faces.Count, DrawFace);
        bitmap.Source.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
        bitmap.Source.Unlock();
    }


    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.W:
                CameraPosZ -= 5;
                break;
            case Key.S:
                CameraPosZ += 5;
                break;
            case Key.A:
                CameraPosX -= 5;
                break;
            case Key.D:
                CameraPosX += 5;
                break;
            case Key.Q:
                CameraPosY += 5;
                break;
            case Key.E:
                CameraPosY -= 5;
                break;
            case Key.I:
                object1.RotationX += 0.1f;
                break;
            case Key.O:
                object1.RotationY += 0.1f;
                break;
            case Key.P:
                object1.RotationZ += 0.1f;
                break;
            case Key.N:
                object1.ScaleX += 0.1f;
                object1.ScaleY += 0.1f;
                object1.ScaleZ += 0.1f;
                break;
            case Key.M:
                object1.ScaleX -= 0.1f;
                object1.ScaleY -= 0.1f;
                object1.ScaleZ -= 0.1f;
                break;
            case Key.Right:
                TargetPosX += 5;
                break;
            case Key.Left:
                TargetPosX -= 5;
                break;
            case Key.Up:
                TargerPosY += 5;
                break;
            case Key.Down:
                TargerPosY -= 5;
                break;
            default:
                break;
        }

        if (bitmap.PixelWidth != (int)ScreenWidth || bitmap.PixelHeight != (int)ScreenHeight)
        {
            bitmap = new Pbgra32Bitmap((int)ScreenWidth, (int)ScreenHeight);
            MainImage.Source = bitmap.Source;
            ScreenWidth = (float)this.Width;
            ScreenHeight = (float)this.Height;
        }
        var modelMatrix = object1.CreateWorldMatrix();
        var viewMatrix = Matrixes.CreateObserverMatrix(new Vector3(CameraPosX, CameraPosY, CameraPosZ), new Vector3(TargetPosX, TargerPosY, TargetPosZ));
        var projectionMatrix = Matrixes.CreateProjectionMatrix(CameraFov, ScreenWidth / ScreenHeight, CameraZNear, CameraZFar);
        var modelViewProjectionMatrix = modelMatrix * viewMatrix * projectionMatrix;
        var viewPortMatrix = Matrixes.CreateViewPortMatrix(ScreenWidth, ScreenHeight);

        windowVertices = new Vector4[object1.Vertexes.Count];
        for (int i = 0; i < windowVertices.Length; i++)
        {
            windowVertices[i] = Vector4.Transform(object1.Vertexes[i], modelViewProjectionMatrix);
            windowVertices[i] /= windowVertices[i].W;
            windowVertices[i] = Vector4.Transform(windowVertices[i], viewPortMatrix);
        }
        

        DrawModel(windowVertices, object1);
    }
}
