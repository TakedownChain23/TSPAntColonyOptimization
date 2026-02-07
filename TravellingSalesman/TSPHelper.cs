namespace TravellingSalesman;

public static class TSPHelper
{
    public static double[,] ParseTSPFile(string filePath)
    {
        var lines = File.ReadAllLines(filePath);
        
        int dimension = 0;
        string edgeWeightType = "";
        var coordinates = new List<(int id, double x, double y)>();
        bool inNodeCoordSection = false;

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();

            if (trimmedLine.StartsWith("DIMENSION"))
            {
                var parts = trimmedLine.Split(':');
                dimension = int.Parse(parts[1].Trim());
            }
            else if (trimmedLine.StartsWith("EDGE_WEIGHT_TYPE"))
            {
                var parts = trimmedLine.Split(':');
                edgeWeightType = parts[1].Trim();
            }
            else if (trimmedLine == "NODE_COORD_SECTION")
            {
                inNodeCoordSection = true;
            }
            else if (trimmedLine == "EOF")
            {
                break;
            }
            else if (inNodeCoordSection && !string.IsNullOrWhiteSpace(trimmedLine))
            {
                var parts = trimmedLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3)
                {
                    int id = int.Parse(parts[0]);
                    double x = double.Parse(parts[1]);
                    double y = double.Parse(parts[2]);
                    coordinates.Add((id, x, y));
                }
            }
        }

        if (edgeWeightType != "EUC_2D")
        {
            throw new InvalidOperationException($"Unsupported EDGE_WEIGHT_TYPE: {edgeWeightType}. Only EUC_2D is supported.");
        }

        var distanceMatrix = new double[dimension, dimension];

        for (int i = 0; i < dimension; i++)
        {
            for (int j = 0; j < dimension; j++)
            {
                if (i == j)
                {
                    distanceMatrix[i, j] = 0;
                }
                else
                {
                    double dx = coordinates[i].x - coordinates[j].x;
                    double dy = coordinates[i].y - coordinates[j].y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    distanceMatrix[i, j] = distance;
                }
            }
        }

        return distanceMatrix;
    }

    public static double CalculatePathLength(double[,] distanceMatrix, int[] path)
    {
        double length = distanceMatrix[path[^1], path[0]];
        for (int i = 0; i < path.Length - 1; i++)
        {
            length += distanceMatrix[path[i], path[i + 1]];
        }

        return length;
    }
}