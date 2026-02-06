namespace TravellingSalesman;

public static class AntColonyOptimization
{
    readonly static int ColonySize = 30;
    readonly static int Iterations = 200;

    readonly static int Alpha = 1;
    readonly static int Beta = 3;

    readonly static double Rho = 0.1;

    public static (int, int[]) RunTSP(int[,] distanceMatrix)
    {
        var nodeCount = distanceMatrix.Length;
        var pheremoneMatrix = new double[nodeCount, nodeCount];

        (int length, int[] path) result = (int.MaxValue, []);

        for (int i = 0; i < Iterations; i++)
        {
            var paths = Enumerable.Range(0, ColonySize).Select(_ => ConstructPath(distanceMatrix, pheremoneMatrix));
            foreach (var path in paths)
            {
                var pathLength = GetPathLength(distanceMatrix, path);
                AddPheremones(pheremoneMatrix, path, GetPathLength(distanceMatrix, path));
                
                if (pathLength < result.length)
                {
                    result = (pathLength, path);
                }
            }

            EvaporatePheremones(pheremoneMatrix);
        }

        return result;
    }

    public static void EvaporatePheremones(double[,] pheremoneMatrix)
    {
        for (int i = 0; i < pheremoneMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < pheremoneMatrix.GetLength(1); j++)
            {
                pheremoneMatrix[i, j] *= 1 - Rho;
            }
        }
    }

    public static void AddPheremones(double[,] pheremoneMatrix, int[] path, int pathLength)
    {
        for (int i = 0; i < path.Length - 1; i++)
        {
            pheremoneMatrix[path[i], path[i + 1]] += 1.0 / (pathLength + 0.05);
            pheremoneMatrix[path[i + 1], path[i]] += 1.0 / (pathLength + 0.05);
        }
    }

    public static int[] ConstructPath(int[,] distanceMatrix, double[,] pheremoneMatrix)
    {
        var path = new List<int> { 0 };

        for (int i = 1; i < distanceMatrix.Length; i++)
        {
            var nextNode = GetNextNode(distanceMatrix, pheremoneMatrix, path);
            path.Add(nextNode);
        }

        return path.ToArray();
    }

    public static int GetNextNode(int[,] distanceMatrix, double[,] pheremoneMatrix, ICollection<int> visitedNodes)
    {
        var remainingNodes = Enumerable.Range(0, distanceMatrix.Length).Where(node => !visitedNodes.Contains(node)).ToArray();
        var probabilities = remainingNodes.Select(node => CalculateRelativeProbability(distanceMatrix, pheremoneMatrix, visitedNodes.Last(), node)).ToArray();

        var randomValue = new Random().NextDouble() * probabilities.Sum();
        var cumulativeValue = 0.0;
        for (int i = 0; i < remainingNodes.Length - 1; i++)
        {
            cumulativeValue += probabilities[i];
            if (cumulativeValue > randomValue)
            {
                return remainingNodes[i];
            }
        }

        return remainingNodes.Last();
    }

    public static double CalculateRelativeProbability(int[,] distanceMatrix, double[,] pheremoneMatrix, int currentNode, int nextNode)
    {
        return Math.Pow(pheremoneMatrix[currentNode, nextNode], Alpha) * Math.Pow(1.0f / (distanceMatrix[currentNode, nextNode] + 0.05), Beta);
    }

    public static int GetPathLength(int[,] distanceMatrix, int[] path)
    {
        var length = 0;
        for (int i = 0; i < path.Length - 1; i++)
        {
            length += distanceMatrix[path[i], path[i + 1]];
        }

        return length;
    }
}
