namespace TravellingSalesmanTest;

public static class TestGraphs
{
    // Simple metric graph: square + diagonal
    public static double[,] SquareGraph()
    {
        return new double[,]
        {
            { 0, 1, Math.Sqrt(2), 1 },
            { 1, 0, 1, Math.Sqrt(2) },
            { Math.Sqrt(2), 1, 0, 1 },
            { 1, Math.Sqrt(2), 1, 0 }
        };
    }

    // Small random metric graph
    public static double[,] RandomMetricGraph(int n, int seed = 42)
    {
        var rand = new Random(seed);
        var points = new (double x, double y)[n];

        for (var i = 0; i < n; i++)
            points[i] = (rand.NextDouble() * 100, rand.NextDouble() * 100);

        var dist = new double[n, n];
        for (var i = 0; i < n; i++)
        for (var j = 0; j < n; j++)
            dist[i, j] = Math.Sqrt(
                Math.Pow(points[i].x - points[j].x, 2) +
                Math.Pow(points[i].y - points[j].y, 2));

        return dist;
    }
}
