using GraphicsAlgorithms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using static System.Globalization.CultureInfo;

namespace OutputWindow.GraphicsAlgoritms;

public class DynamicObject
{
    public Object3D exportedObject = new Object3D();

    Dictionary<int, List<Vector3>> vertexCombinations;

    int currentCombination = 0;
    int requiredGapInMs = 34;
    TimeSpan lastChange = TimeSpan.Zero;
    DateTime lastChangeTime;

    public void LoadFolder(string folderName, string baseFileName)
    {
        if (Directory.Exists(folderName))
        {
            vertexCombinations = new Dictionary<int, List<Vector3>>();
            // Get all files in the directory with the specified criteria
            string[] files = Directory.GetFiles(folderName, $"{baseFileName}*.obj");

            foreach (string file in files)
            {
                Console.WriteLine(file);
            }

            foreach (string file in files)
            {
                var temp = Path.GetFileNameWithoutExtension(file);
                // Get the part of the file name that comes after "Lal"
                int fileNumber = Int32.Parse(temp.Substring(temp.LastIndexOf(baseFileName) + baseFileName.Length));

                var vertexList = new List<Vector3>();

                foreach (string line in File.ReadLines(file))
                {
                    string[] args = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (args.Length > 0)
                    {
                        switch (args[0])
                        {
                            case "v":
                                float x = float.Parse(args[1], InvariantCulture);
                                float y = float.Parse(args[2], InvariantCulture);
                                float z = float.Parse(args[3], InvariantCulture);
                                vertexList.Add(new(x, y, z));
                                break;
                        }
                    }
                }

                vertexCombinations.Add(fileNumber * requiredGapInMs, vertexList);
            }

            currentCombination = 0;
            exportedObject.LoadModel($"{folderName}\\{baseFileName}{currentCombination}.obj");
            lastChangeTime = DateTime.Now;
        }
    }

    public void NextCombination()
    {
        if (vertexCombinations == null)
        {
            return;
        }

        var now = DateTime.Now;
        TimeSpan gap = DateTime.Now - lastChangeTime;
        lastChangeTime = now;

        Trace.WriteLine(gap.TotalMilliseconds);

        currentCombination = (currentCombination + (int)gap.TotalMilliseconds) % (vertexCombinations.Count * requiredGapInMs);

        var nextVertexes = new List<Vector3>();

        if (vertexCombinations.TryGetValue(currentCombination, out nextVertexes))
        {
            exportedObject.Vertexes.Clear();
            exportedObject.Vertexes.AddRange(nextVertexes);
        }
        else
        {
            int closestBot = (currentCombination / requiredGapInMs) * requiredGapInMs;
            int next = closestBot + requiredGapInMs;
            if (next >= vertexCombinations.Count * requiredGapInMs)
            {
                next = 0;
            }

            double k = (currentCombination % requiredGapInMs) / (double)requiredGapInMs;

            nextVertexes = Interpolate(vertexCombinations[closestBot], vertexCombinations[next], k);
            exportedObject.Vertexes.Clear();
            exportedObject.Vertexes.AddRange(nextVertexes);
        }
    }

    private List<Vector3> Interpolate(List<Vector3> prev, List<Vector3> next, double k)
    {
        var result = new List<Vector3>();
        for (int i = 0; i < prev.Count; i++)
        {
            result.Add(prev[i] + (next[i] - prev[i]) * (float)k);
        }

        return result;
    }
}
