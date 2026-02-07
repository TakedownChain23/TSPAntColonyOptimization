namespace TravellingSalesman;

public static class AntColonyOptimization
{
    readonly static int ColonySize = 600;
    readonly static int Iterations = 800;

    readonly static double Alpha = 1;
    readonly static double Beta = 2;

    readonly static double Rho = 0.2;

    readonly static double ElitistWeight = 2;

    public static (int[] path, double length) RunTSP(double[,] distanceMatrix)
    {
        var nodeCount = distanceMatrix.GetLength(0);
        var pheremoneMatrix = new double[nodeCount, nodeCount];
        for (int i = 0; i < nodeCount; i++)
        {
            for (int j = 0; j < nodeCount; j++)
            {
                pheremoneMatrix[i, j] = 1.0;
            }
        }

        (int[] path, double length) result = ([], double.MaxValue / 2);

        for (int i = 0; i < Iterations; i++)
        {
            var pathLengths = new List<(int[] path, double length)>();

            Parallel.For(0, ColonySize, _ =>
            {
                var path = ConstructPath(distanceMatrix, pheremoneMatrix);
                lock (pathLengths)
                {
                    pathLengths.Add((path, GetPathLength(distanceMatrix, path)));
                }
            });

            var shortestPath = pathLengths.MinBy(p => p.length);
            if (shortestPath.length < result.length)
            {
                result = shortestPath;
                Console.WriteLine($"New Optimal Path Length: {shortestPath.length}");
            }

            AddPheremones(pheremoneMatrix, shortestPath.path, shortestPath.length, true);
            foreach (var pathLength in pathLengths.Where(p => p != shortestPath))
            {
                AddPheremones(pheremoneMatrix, pathLength.path, pathLength.length, false);
            }

            EvaporatePheremones(pheremoneMatrix);
        }

        return result;
    }

    static void EvaporatePheremones(double[,] pheremoneMatrix)
    {
        for (int i = 0; i < pheremoneMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < pheremoneMatrix.GetLength(1); j++)
            {
                pheremoneMatrix[i, j] *= 1 - Rho;
            }
        }
    }

    static void AddPheremones(double[,] pheremoneMatrix, int[] path, double pathLength, bool isElitist)
    {
        var pheremoneDeposit = isElitist ? ElitistWeight / (pathLength + 0.05) : 1.0 / (pathLength + 0.05);

        for (int i = 0; i < path.Length - 1; i++)
        {
            pheremoneMatrix[path[i], path[i + 1]] += pheremoneDeposit;
            pheremoneMatrix[path[i + 1], path[i]] += pheremoneDeposit;
        }

        pheremoneMatrix[path[^1], path[0]] += pheremoneDeposit;
        pheremoneMatrix[path[0], path[^1]] += pheremoneDeposit;
    }

    static int[] ConstructPath(double[,] distanceMatrix, double[,] pheremoneMatrix)
    {
        var nodeCount = distanceMatrix.GetLength(0);
        var path = new List<int> { new Random().Next(nodeCount) };

        for (int i = 0; i < nodeCount - 1; i++)
        {
            var nextNode = GetNextNode(distanceMatrix, pheremoneMatrix, path);
            path.Add(nextNode);
        }

        return [.. path];
    }

    static int GetNextNode(double[,] distanceMatrix, double[,] pheremoneMatrix, List<int> visitedNodes)
    {
        var remainingNodes = Enumerable.Range(0, distanceMatrix.GetLength(0)).Where(node => !visitedNodes.Contains(node)).ToArray();
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

    static double CalculateRelativeProbability(double[,] distanceMatrix, double[,] pheremoneMatrix, int currentNode, int nextNode)
    {
        return Math.Pow(pheremoneMatrix[currentNode, nextNode], Alpha) * Math.Pow(1.0 / (distanceMatrix[currentNode, nextNode] + 0.05), Beta);
    }

    static double GetPathLength(double[,] distanceMatrix, int[] path)
    {
        var length = distanceMatrix[path[^1], path[0]];
        for (int i = 0; i < path.Length - 1; i++)
        {
            length += distanceMatrix[path[i], path[i + 1]];
        }

        return length;
    }
}
