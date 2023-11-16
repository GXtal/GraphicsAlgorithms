using GraphicsAlgorithms;
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
using System.Windows.Threading;

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
    LightSource lightSource = new LightSource();

    Pbgra32Bitmap bitmap;
    Vector4[] windowVertices;
    Vector3[] worldVertices;

    float[][] zbuffer;
    bool[] faceCanBeDrawn;
    Vector3 eye = new Vector3(0, 0, 0);
    Vector3[] vn;
    int[] countvn;
    Matrix4x4 modelMatrix;
    Matrix4x4 modelMatrixInvert;
    Matrix4x4 viewMatrixInvert;
    Matrix4x4 pojectionInvert;
    Matrix4x4 viewPortInvert;
    bool isLightChange = false;

    Stopwatch stopWatch = new Stopwatch();
    private int frameCount = 0;
    private int fps = 0;
    private DispatcherTimer fpsTimer = new DispatcherTimer();


    public void EvaluateWindowCoords(Object3D model) {
        modelMatrix = model.CreateWorldMatrix();
        var viewMatrix = mainCamera.CreateObserverMatrix(target.GetPosition());
        Matrix4x4.Invert(modelMatrix, out modelMatrixInvert);
        Matrix4x4.Invert(viewMatrix, out viewMatrixInvert);
        var projectionMatrix = mainCamera.CreateProjectionMatrix(ScreenWidth / ScreenHeight);
        var modelViewProjectionMatrix = modelMatrix * viewMatrix * projectionMatrix;
        var viewPortMatrix = Matrixes.CreateViewPortMatrix(ScreenWidth, ScreenHeight);
        Matrix4x4.Invert(projectionMatrix, out viewPortInvert);
        Matrix4x4.Invert(viewPortMatrix, out viewPortInvert);


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
        var aDot = windowVertices[mainModel.Faces[i][0]];
        var bDot = windowVertices[mainModel.Faces[i][1]];
        var cDot = windowVertices[mainModel.Faces[i][2]];

        var CA = cDot - bDot;
        var BA = aDot - bDot;
        //var denominator = Vector3.Cross(CA, BA);
        var denominator = CA.X * BA.Y - CA.Y * BA.X;
       // var eyeFromTarget = eye - target.GetPosition();
        if (denominator > 0)
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


        var normA = Vector3.Normalize(Vector3.Transform(vn[mainModel.Faces[i][0]], modelMatrix));
        var normB = Vector3.Normalize(Vector3.Transform(vn[mainModel.Faces[i][1]], modelMatrix));
        var normC = Vector3.Normalize(Vector3.Transform(vn[mainModel.Faces[i][2]], modelMatrix));


        var denominator = Math.Abs((CA.X * BA.Y - CA.Y * BA.X));
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
                P.Z = depth;
                var nPoint = normA * w + normB * u + normC * v; 
                if (depth < zbuffer[y][x])
                {
                    var fragPos = Vector4.Transform(P, viewPortInvert * pojectionInvert * viewMatrixInvert);

                    var lightVector = lightSource.getLightPosition(new Vector3(0,0,0));
                    var FragPos3 = new Vector3(fragPos.X, fragPos.Y, fragPos.Z);
                    lightVector = Vector3.Normalize(lightVector - FragPos3);
                    var eyeFromFrag = Vector3.Normalize(eye - FragPos3);
                    var LN = Vector3.Dot(nPoint, lightVector);
                    var defferedLightVector = lightVector - 2 * LN * nPoint;
                    var RV = Vector3.Dot(defferedLightVector, eyeFromFrag);
                    if (RV < 0 ) RV = 0.0f;
                    var lightIntVector = lightSource.GetResultColor(LN, RV);
                    bitmap.SetPixel(x, y, new Vector3(mainModel.FacesColor[i][0] * lightIntVector.X, mainModel.FacesColor[i][1] * lightIntVector.Y, mainModel.FacesColor[i][2] * lightIntVector.Z));
                    zbuffer[y][x] = depth;
                }
                
            } 
        }

        Console.WriteLine(0);
    }

    public void DrawModel(Vector4[] windowVertices, Object3D obj)
    {

        stopWatch.Restart();
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
                zbuffer[i][j] = float.MaxValue;
            }
        }
        for (var i = 0; i < mainModel.Faces.Count; ++i)
        {
            if (ZbufferCanBeDrawn(i, eye))
            {
                RasterizationFace(i);
            }
        }

        stopWatch.Stop();
        //Parallel.For(0, obj.Faces.Count, RasterizationFace); //was changed
        bitmap.Source.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
        bitmap.Source.Unlock();
    }

    private void FpsTimer_Tick(object sender, EventArgs e)
    {
        // Update the text block with FPS value
        fpsTextBlock.Text = $"Render Time: {fps}";

        // Reset frame count
        frameCount = 0;
    }

    private void CompositionTarget_Rendering(object sender, EventArgs e)
    {
        // Update frame count
        frameCount++;

        // Calculate FPS
        double elapsedMilliSeconds = stopWatch.Elapsed.TotalMilliseconds;
        fps = (int)(elapsedMilliSeconds);
        // Reset stopwatch
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
            case Key.U:
                lightSource.AmbientIntensity += 0.05f;
                if (lightSource.AmbientIntensity > 1f) lightSource.AmbientIntensity = 1.0f;
                break;
            case Key.I:
                lightSource.AmbientIntensity -= 0.05f;
                if (lightSource.AmbientIntensity < 0f) lightSource.AmbientIntensity = 0.0f;
                break;
            case Key.O:
                lightSource.DiffuseIntensity += 0.05f;
                if (lightSource.DiffuseIntensity > 1f) lightSource.DiffuseIntensity = 1.0f;
                break;
            case Key.P:
                lightSource.DiffuseIntensity -= 0.05f;
                if (lightSource.DiffuseIntensity < 0f) lightSource.DiffuseIntensity = 0.0f;
                break;
            case Key.H:
                lightSource.SpecularAlpha += 1f;
                //if (lightSource.SpecularAlpha > 0f) lightSource.DiffuseIntensity = 0.0f;
                break;
            case Key.J:
                lightSource.SpecularAlpha -= 1f;
                if (lightSource.SpecularAlpha < 1f) lightSource.SpecularAlpha = 1.0f;
                break;
            case Key.K:
                lightSource.SpecularIntensity += 0.05f;
                if (lightSource.SpecularIntensity > 1f) lightSource.SpecularIntensity = 1.0f;
                break;
            case Key.L:
                lightSource.SpecularIntensity -= 0.05f;
                if (lightSource.SpecularIntensity < 0f) lightSource.SpecularIntensity = 0.0f;
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
        //mainModel.LoadModel("C:\\Users\\admin\\Desktop\\ObjDrawer\\ObjDrawer\\data\\HardshellTransformer\\Hardshell.obj");
        mainModel.LoadModel("C:\\Users\\admin\\Desktop\\ObjDrawer\\ObjDrawer\\data\\Torque Twister\\Torque Twister.obj");
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

            //var CBWorld = cDotWorld - bDotWorld;
            //var ABWorld = aDotWorld - bDotWorld;

            //var BCWorld = bDotWorld - cDotWorld;
            //var ACWorld = aDotWorld - cDotWorld;

            var normA = Vector3.Normalize(Vector3.Cross(CAWorld, BAWorld));
           // var normB = Vector3.Normalize(Vector3.Cross(ABWorld, CBWorld));
           // var normC = Vector3.Normalize(Vector3.Cross(BCWorld, ACWorld));

            vn[mainModel.Faces[i][0]] += normA;
            vn[mainModel.Faces[i][1]] += normA;
            vn[mainModel.Faces[i][2]] += normA;
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
        
       
        CompositionTarget.Rendering += CompositionTarget_Rendering;

        // Set up the FPS timer (for updating the text block)
        DispatcherTimer fpsTimer = new DispatcherTimer();
        fpsTimer.Tick += FpsTimer_Tick;
        fpsTimer.Interval = TimeSpan.FromSeconds(1);
        fpsTimer.Start();

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
        if (e.ChangedButton == MouseButton.Right)
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
