namespace TravellingSalesman;

public class Christofides
{
    public class Edge
    {
        public readonly int PointA;
        public readonly int PointB;
        public readonly double Weight;
        public Edge(int a, int b, double weight)
        {
            PointA = a;
            PointB = b;
            Weight = weight;
        }
    }
    public static (int[] path, double length) RunAlgorithm(double[,] nodeDistances)
    {
        var n = nodeDistances.GetLength(0);

        // Compute MST
        var mst = ComputeMinimumSpanningTree(nodeDistances, n);

        // Find odd-degree vertices
        var oddDegreeVerticesOutput = FindOddDegreeVertices(mst, n);
        var adjacency = oddDegreeVerticesOutput.Item1;
        var oddVertices = oddDegreeVerticesOutput.Item2;
        
        // Compute minimum-weight perfect matching
        var matching = ComputeMinimumWeightMatching(oddVertices, nodeDistances);

        // Combine MST + matching → Eulerian multigraph
        var eulerianGraph = CombineGraphs(mst, matching);

        // Find Eulerian tour
        var eulerianTour = FindEulerianTour(adjacency, n);

        // Shortcut repeated vertices → Hamiltonian TSP tour
        return ShortcutEulerianTour(eulerianTour, eulerianGraph);
    }

    public static List<Edge> ComputeMinimumSpanningTree(double[,] distances, int n)
    {
        // Basic Prim's Algorithm
        var inMst = new bool[n];
        var key = new double[n];      // min edge weight to MST
        var parent = new int[n];      // MST parent
        var mst = new List<Edge>();

        for (var i = 0; i < n; i++) key[i] = double.MaxValue;
        key[0] = 0; parent[0] = -1;

        for (var count = 0; count < n; count++)
        {
            // Pick vertex with minimum key not in MST
            var min = double.MaxValue; 
            var u = -1;
            for (var v = 0; v < n; v++)
                if (!inMst[v] && key[v] < min)
                { min = key[v]; u = v; }

            inMst[u] = true;

            // Add edge to MST if not root
            if (parent[u] != -1)
                mst.Add(new Edge(parent[u], u, distances[parent[u], u]));

            // Update key values for neighbors
            for (var v = 0; v < n; v++)
                if (!inMst[v] && distances[u, v] < key[v])
                {
                    key[v] = distances[u, v];
                    parent[v] = u;
                }
        }

        return mst;
    }

    public static (int[,], List<int>) FindOddDegreeVertices(List<Edge> mst, int n)
    {
        var vertexDegrees = new int[n, n];
        foreach (var e in mst)
        {
            vertexDegrees[e.PointA, e.PointB]++;
            vertexDegrees[e.PointB, e.PointA]++;
        }

        var oddVertices = new List<int>();
        for (var u = 0; u < n; u++)
        {
            var degree = 0;

            for (var v = 0; v < n; v++)
            {
                degree += vertexDegrees[u, v];
            }

            if (degree % 2 == 1)
                oddVertices.Add(u);
        }

        return (vertexDegrees,  oddVertices);
    }

    public static List<Edge> ComputeMinimumWeightMatching(List<int> oddVertices, double[,] nodeDistances)
    {
        // Need Blossom Algorithm Implementation
        return null;
    }

    public static List<Edge> CombineGraphs(List<Edge> mst, List<Edge> matching)
    {
        var combined = new List<Edge>(mst.Count + matching.Count);
        combined.AddRange(mst);
        combined.AddRange(matching);
        return combined;    
    }

    public static List<int> FindEulerianTour(int[,] adjacency, int n) // Using Hierholzer's algorithm
    {
        var currentPath = new List<int> { 0 };
        var graphCircuit = new List<int>();
        while (currentPath.Count > 0)
        {
            var currentNode = currentPath.Last();

            if (adjacency.GetLength(currentNode) > 0)
            {
                var nextNode = adjacency[currentNode, adjacency.GetLength(currentNode) - 1];
                adjacency[currentNode, adjacency.GetLength(currentNode) - 1] = 0;
                
                currentPath.Add(nextNode);
            }
            else
            {
                graphCircuit.Add(currentPath.Last());
                currentPath.RemoveAt(currentPath.Count - 1);
            }
        }
        graphCircuit.Reverse();
        return graphCircuit;
    }

    public static (int[], double) ShortcutEulerianTour(List<int> eulerianTour, List<Edge> eulerianGraph)
    {
        var visited = new HashSet<int>();
        var tour = new List<int>();
        var tourLength = 0.00;
        var lastPoint = 1;

        foreach (var v in eulerianTour)
        {
            if (visited.Contains(v))
            {
                continue;
            }
            var weight = eulerianGraph.Where(x => x.PointA == lastPoint && x.PointB == v).FirstOrDefault().Weight;
            if (weight == 0)
            {
                throw new Exception("Not good");
            }
            tour.Add(v);
            visited.Add(v);
            tourLength += weight;
            lastPoint = v;
        }

        // Close the cycle
        tour.Add(tour[0]);
        var returnDistance = eulerianGraph.Where(x => x.PointA == lastPoint && x.PointB == 1).FirstOrDefault().Weight;
        if (returnDistance == 0)
        {
            throw new Exception("Not good");
        }
        
        tourLength += returnDistance;
        
        return (tour.ToArray(), tourLength);
    }
}