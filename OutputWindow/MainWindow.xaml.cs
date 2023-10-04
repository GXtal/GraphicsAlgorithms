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

    Object3D mainModel = new Object3D();
    Camera mainCamera = new Camera();
    Target target = new Target();

    Pbgra32Bitmap bitmap;
    Vector4[] windowVertices;

    public void EvaluateWindowCoords(Object3D model) {
        var modelMatrix = model.CreateWorldMatrix();
        var viewMatrix = mainCamera.CreateObserverMatrix(target.GetPosition());
        var projectionMatrix = mainCamera.CreateProjectionMatrix(ScreenWidth / ScreenHeight);
        var modelViewProjectionMatrix = modelMatrix * viewMatrix * projectionMatrix;
        var viewPortMatrix = Matrixes.CreateViewPortMatrix(ScreenWidth, ScreenHeight);

        windowVertices = new Vector4[model.Vertexes.Count];
        for (int i = 0; i < windowVertices.Length; i++)
        {
            windowVertices[i] = Vector4.Transform(model.Vertexes[i], modelViewProjectionMatrix);
            windowVertices[i] /= windowVertices[i].W;
            windowVertices[i] = Vector4.Transform(windowVertices[i], viewPortMatrix);
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
        Parallel.For(0, obj.Faces.Count, DrawFace);
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
            case Key.W:
                mainCamera.PositionZ -= 5;
                break;
            case Key.S:
                mainCamera.PositionZ += 5;
                break;
            case Key.A:
                mainCamera.PositionX -= 5;
                break;
            case Key.D:
                mainCamera.PositionX += 5;
                break;
            case Key.Q:
                mainCamera.PositionY += 5;
                break;
            case Key.E:
                mainCamera.PositionY -= 5;
                break;
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
        EvaluateWindowCoords(mainModel);

        DrawModel(windowVertices, mainModel);
    }
}
