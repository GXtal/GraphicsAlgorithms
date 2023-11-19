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

    int ScreenIntWidth { get; set; }
    int ScreenIntHeight { get; set; }

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

    public bool ZbufferCanBeDrawn(List<int> polygon)
    {
        var aDot = windowVertices[polygon[0]];
        var bDot = windowVertices[polygon[1]];
        var cDot = windowVertices[polygon[2]];

        //was fixed
        var CA = cDot - aDot;
        var BA = bDot - aDot;
        //var denominator = Vector3.Cross(CA, BA);
        var denominator = CA.X * BA.Y - CA.Y * BA.X;
       // var eyeFromTarget = eye - target.GetPosition();
        if (denominator < 0)
        {
            return false;
        }

        return true;
    }
    
    //x and y viewport coordinates
    private List<Vector3> GetModelColor(List<int> polygon, string materialString, float x, float y)
    {
        var Ia = new Vector3();
        var Is = new Vector3();
        var Id = new Vector3();

        var material = mainModel.materials[materialString];

        if (material.TextureParts.MapKa != null)
            Ia = material.TextureParts.GetKaFragment(y,x);
        if (material.TextureParts.MapKs != null)
            Is = material.TextureParts.GetKsFragment(y, x);
        if (material.TextureParts.MapKd != null)
            Id = material.TextureParts.GetKdFragment(y, x);

        return new List<Vector3>() { Ia, Id, Is };
    }

    public void RasterizationFace(List<int> polygon, List<int> textPolygon, string materialString)
    {
        var aTextDot = mainModel.TextVertexes[textPolygon[0]];
        var bTextDot = mainModel.TextVertexes[textPolygon[1]];
        var cTextDot = mainModel.TextVertexes[textPolygon[2]];

        var aDot = windowVertices[polygon[0]];
        var bDot = windowVertices[polygon[1]];
        var cDot = windowVertices[polygon[2]];

        var minX = (int)Math.Min(Math.Min(aDot.X, bDot.X), cDot.X);
        var maxX = (int)Math.Max(Math.Max(aDot.X, bDot.X), cDot.X);
        var minY = (int)Math.Min(Math.Min(aDot.Y, bDot.Y), cDot.Y);
        var maxY = (int)Math.Max(Math.Max(aDot.Y, bDot.Y), cDot.Y);


        var CA = cDot - aDot;
        var BA = bDot - aDot;   

        //for lab3
        var normA = Vector3.Normalize(Vector3.Transform(vn[polygon[0]], modelMatrix));
        var normB = Vector3.Normalize(Vector3.Transform(vn[polygon[1]], modelMatrix));
        var normC = Vector3.Normalize(Vector3.Transform(vn[polygon[2]], modelMatrix));

        //for lab2
        //var aDotWorld = worldVertices[mainModel.Faces[i][0]];
        //var bDotWorld = worldVertices[mainModel.Faces[i][1]];
        //var cDotWorld = worldVertices[mainModel.Faces[i][2]];

        //var CAWorld = cDotWorld - aDotWorld;
        //var BAWorld = bDotWorld - aDotWorld;

        //var norm = Vector3.Normalize(Vector3.Cross(CAWorld, BAWorld));

        //Coodinates weren't fixed. Only after polygons were fixed
        var denominator = Math.Abs((CA.X * BA.Y - CA.Y * BA.X));
        for(var y = minY; y <= maxY; ++y)
        {
            for (var x = minX; x <= maxX; ++x)
            {
                if (x < 0 || x >= ScreenIntWidth || y < 0 || y >= ScreenIntHeight)
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
                //for lab3
                if (depth < zbuffer[y][x])
                {
                    //for lab3
                    var textVector = aTextDot * w + (bTextDot * u) + cTextDot * v;
                    var fragPos = Vector4.Transform(P, viewPortInvert * pojectionInvert * viewMatrixInvert);
                    var nPoint = normA * w + normB * u + normC * v;
                    if (mainModel.materials[materialString].TextureParts.MapNormals != null)
                    {
                        nPoint = mainModel.materials[materialString].TextureParts.GetNormalFragment(textVector.Y, textVector.X);
                        // do something
                    }
                    var lightVector = lightSource.getLightPosition(new Vector3(0,0,0));
                    var FragPos3 = new Vector3(fragPos.X, fragPos.Y, fragPos.Z);
                    lightVector = Vector3.Normalize(lightVector - FragPos3);
                    var eyeFromFrag = Vector3.Normalize(eye - FragPos3);
                    var LN = Vector3.Dot(nPoint, lightVector);
                    var defferedLightVector = lightVector - 2 * LN * nPoint;
                    var RV = Vector3.Dot(defferedLightVector, eyeFromFrag);
                    if (RV < 0 ) RV = 0.0f;
                    if (LN < 0) LN = 0.0f;
                    
                    
                    var partsColors = GetModelColor(polygon, materialString, textVector.X, textVector.Y);

                    var lightIntVector = lightSource.GetResultColor(LN, RV, partsColors, mainModel.materials[materialString].Alpha, mainModel.materials[materialString]);
                    bitmap.SetPixel(x, y, new Vector3(lightIntVector.X, lightIntVector.Y, lightIntVector.Z));
                    zbuffer[y][x] = depth;

                    //for lab2
                    //var lightVector = lightSource.getLightPosition(new Vector3(0, 0, 0));
                    //lightVector = Vector3.Normalize(-lightVector);
                    //var light = Vector3.Dot(norm, lightVector);
                    //if (light < 0) light = 0;
                    //bitmap.SetPixel(x, y, new Vector3(light, light, light));
                    //zbuffer[y][x] = depth;
                }
                
            } 
        }
    }

    public void DrawModel(Vector4[] windowVertices, Object3D obj)
    {

       // stopWatch.Restart();
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
            for (var j = 0; j < ScreenIntWidth; ++j)
            {
                zbuffer[i][j] = float.MaxValue;
            }
        }

        foreach (var materialString in mainModel.materials.Keys)
        {
            var polygons = mainModel.materials[materialString].Faces;
            var textPolygons = mainModel.materials[materialString].TextFaces;

            for (var i = 0; i < polygons.Count; ++i)
            {
                if (ZbufferCanBeDrawn(polygons[i]))
                {
                    RasterizationFace(polygons[i], textPolygons[i], materialString);
                }
            }
        }

        Console.WriteLine();


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
            //case Key.U:
            //    lightSource.AmbientIntensity += 0.05f;
            //    if (lightSource.AmbientIntensity > 1f) lightSource.AmbientIntensity = 1.0f;
            //    break;
            //case Key.I:
            //    lightSource.AmbientIntensity -= 0.05f;
            //    if (lightSource.AmbientIntensity < 0f) lightSource.AmbientIntensity = 0.0f;
            //    break;
            //case Key.O:
            //    lightSource.DiffuseIntensity += 0.05f;
            //    if (lightSource.DiffuseIntensity > 1f) lightSource.DiffuseIntensity = 1.0f;
            //    break;
            //case Key.P:
            //    lightSource.DiffuseIntensity -= 0.05f;
            //    if (lightSource.DiffuseIntensity < 0f) lightSource.DiffuseIntensity = 0.0f;
            //    break;
            //case Key.H:
            //    lightSource.SpecularAlpha += 1f;
            //    //if (lightSource.SpecularAlpha > 0f) lightSource.DiffuseIntensity = 0.0f;
            //    break;
            //case Key.J:
            //    lightSource.SpecularAlpha -= 1f;
            //    if (lightSource.SpecularAlpha < 1f) lightSource.SpecularAlpha = 1.0f;
            //    break;
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
        mainModel.LoadModel("C:\\Users\\admin\\Desktop\\ObjDrawer\\ObjDrawer\\data\\HardshellTransformer\\Hardshell.obj");
        //mainModel.LoadModel("C:\\Users\\admin\\Desktop\\ObjDrawer\\ObjDrawer\\data\\Torque Twister\\Torque Twister.obj");
        ScreenWidth = (float)MainGrid.ActualWidth;
        ScreenHeight = (float)MainGrid.ActualHeight;
        ScreenIntHeight = (int)MainGrid.ActualHeight;
        ScreenIntWidth = (int)MainGrid.ActualWidth;
        bitmap = new Pbgra32Bitmap((int)ScreenWidth, (int)ScreenHeight);
        MainImage.Source = bitmap.Source;
        MainImage.Source = bitmap.Source;
        target.PositionX = mainModel.PositionX;
        target.PositionY = mainModel.PositionY;
        target.PositionZ = mainModel.PositionZ;

        vn = new Vector3[mainModel.Vertexes.Count];
        foreach(var materialString in mainModel.materials.Keys)
        {
            var polygons = mainModel.materials[materialString].Faces;
            foreach(var polygon in polygons)
            {
                var aDotWorld = mainModel.Vertexes[polygon[0]];
                var bDotWorld = mainModel.Vertexes[polygon[1]];
                var cDotWorld = mainModel.Vertexes[polygon[2]];

                var CAWorld = cDotWorld - aDotWorld;
                var BAWorld = bDotWorld - aDotWorld;

                //var CBWorld = cDotWorld - bDotWorld;
                //var ABWorld = aDotWorld - bDotWorld;

                //var BCWorld = bDotWorld - cDotWorld;
                //var ACWorld = aDotWorld - cDotWorld;

                var normA = Vector3.Normalize(Vector3.Cross(CAWorld, BAWorld));
                // var normB = Vector3.Normalize(Vector3.Cross(ABWorld, CBWorld));
                // var normC = Vector3.Normalize(Vector3.Cross(BCWorld, ACWorld));

                vn[polygon[0]] += normA;
                vn[polygon[1]] += normA;
                vn[polygon[2]] += normA;
            }
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
        ScreenIntHeight = (int)MainGrid.ActualHeight;
        ScreenIntWidth = (int)MainGrid.ActualWidth;
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
