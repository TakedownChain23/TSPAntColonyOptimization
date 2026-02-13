namespace TravellingSalesman;

public static class Christofides
{
    public static (int[] tour, double length) RunAlgorithm(double[,] distances)
    {
        var nodeCount = distances.GetLength(0);

        // 1. Minimum Spanning Tree (as adjacency matrix)
        var mstEdgeCount = ComputeMinimumSpanningTree(distances);

        // 2. Find odd-degree vertices in MST
        var oddVertices = FindOddDegreeVertices(mstEdgeCount);

        // 3. Greedy matching on odd vertices (Greedy Blossom implementation)
        var matching = RunBlossom(oddVertices, distances);

        // 4. Combine MST + matching -> Eulerian multigraph
        AddMatchingToGraph(mstEdgeCount, matching);

        // 5. Eulerian tour (Hierholzer)
        var eulerianTour = FindEulerianTour(mstEdgeCount);

        // 6. Shortcut revisited vertices in Eulerian tour -> Hamiltonian cycle
        return ShortcutEulerianTour(eulerianTour, distances);
    }

    # region MST

    public static int[,] ComputeMinimumSpanningTree(double[,] distances)
    {
        var n = distances.GetLength(0);
        var edgeCount = new int[n, n];

        var inMst = new bool[n];
        var key = new double[n];
        var parent = new int[n];

        Array.Fill(key, double.MaxValue);
        key[0] = 0;
        parent[0] = -1;

        for (var i = 0; i < n; i++)
        {
            var u = SelectMinKeyVertex(inMst, key);
            inMst[u] = true;

            if (parent[u] != -1)
            {
                edgeCount[u, parent[u]]++;
                edgeCount[parent[u], u]++;
            }

            UpdateKeys(u, distances, inMst, key, parent);
        }

        return edgeCount;
    }

    private static int SelectMinKeyVertex(bool[] inMst, double[] key)
    {
        var minIndex = -1;
        var minValue = double.MaxValue;

        for (var i = 0; i < key.Length; i++)
        {
            if (!inMst[i] && key[i] < minValue)
            {
                minValue = key[i];
                minIndex = i;
            }
        }

        return minIndex;
    }

    private static void UpdateKeys(int u, double[,] distances, bool[] inMst, double[] key, int[] parent)
    {
        var n = distances.GetLength(0);

        for (var v = 0; v < n; v++)
        {
            if (!inMst[v] && distances[u, v] < key[v])
            {
                key[v] = distances[u, v];
                parent[v] = u;
            }
        }
    }
    
    # endregion

    # region OddDegree
    public static List<int> FindOddDegreeVertices(int[,] edgeCount)
    {
        var n = edgeCount.GetLength(0);
        var oddVertices = new List<int>();

        for (var u = 0; u < n; u++)
        {
            var degree = 0;
            for (var v = 0; v < n; v++)
                degree += edgeCount[u, v];

            if (degree % 2 == 1)
                oddVertices.Add(u);
        }

        return oddVertices;
    }

    # endregion
    
    # region Blossom

    // Using a less complex greedy blossom algorithm instead of Edmonds Blossom
    public static List<(int u, int v)> RunBlossom(List<int> vertices, double[,] distances)
    {
        var matching = new List<(int u, int v)>();
        var matched = new HashSet<int>();

        // Generate all edges with weights
        var edges = new List<(int u, int v, double weight)>();
        for (var i = 0; i < vertices.Count; i++)
        {
            for (var j = i + 1; j < vertices.Count; j++)
            {
                var u = vertices[i];
                var v = vertices[j];
                edges.Add((u, v, distances[u, v]));
            }
        }

        // Sort edges by weight ascending
        edges.Sort((a, b) => a.weight.CompareTo(b.weight));

        // Greedily pick edges where neither vertex is already matched
        foreach (var (u, v, w) in edges)
        {
            if (!matched.Contains(u) && !matched.Contains(v))
            {
                matching.Add((u, v));
                matched.Add(u);
                matched.Add(v);
            }
        }

        return matching;
    }
    
    # endregion
    
    # region AddGraphs

    public static void AddMatchingToGraph(int[,] edgeCount, List<(int u, int v)> matching)
    {
        // Combine MST and Matching from Blossom
        foreach (var (u, v) in matching)
        {
            edgeCount[u, v]++;
            edgeCount[v, u]++;
        }
    }

    # endregion
    
    # region EulerianTour

    public static List<int> FindEulerianTour(int[,] edgeCount)
    {
        var n = edgeCount.GetLength(0);
        var stack = new Stack<int>();
        var circuit = new List<int>();

        stack.Push(0);

        while (stack.Count > 0)
        {
            var u = stack.Peek();
            var v = FindNextNeighbor(u, edgeCount);

            if (v != -1)
            {
                edgeCount[u, v]--;
                edgeCount[v, u]--;
                stack.Push(v);
            }
            else
            {
                circuit.Add(stack.Pop());
            }
        }
        
        // Current circuit will back backwards
        circuit.Reverse();
        return circuit;
    }

    private static int FindNextNeighbor(int u, int[,] edgeCount)
    {
        var n = edgeCount.GetLength(0);
        for (var v = 0; v < n; v++)
        {
            if (edgeCount[u, v] > 0)
            {
                return v;
            }
        }

        return -1;
    }

    # endregion
    
    # region Shortcut

    public static (int[] tour, double length) ShortcutEulerianTour(List<int> eulerianTour, double[,] distances)
    {
        var visited = new bool[distances.GetLength(0)];
        var tour = new List<int>();
        double cost = 0;

        var previous = eulerianTour[0];
        tour.Add(previous);
        visited[previous] = true;

        foreach (var v in eulerianTour)
        {
            if (!visited[v])
            {
                cost += distances[previous, v];
                previous = v;
                tour.Add(v);
                visited[v] = true;
            }
        }

        cost += distances[previous, tour[0]];
        tour.Add(tour[0]);

        return (tour.ToArray(), cost);
    }
    
    # endregion
}