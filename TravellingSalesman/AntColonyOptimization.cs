namespace TravellingSalesman;

public static class AntColonyOptimization
{
    readonly static int ColonySize = 400;
    readonly static int Iterations = 400;

    readonly static double Alpha = 1;
    readonly static double Beta = 3;

    readonly static double Rho = 0.22;

    readonly static double PheremoneDepositWeight = 1;

    readonly static double MinScalingFactor = 0.0001;

    public static (int[] path, double length) RunTSP(double[,] distanceMatrix)
    {
        var nodeCount = distanceMatrix.GetLength(0);
        var pheremoneMatrix = new double[nodeCount, nodeCount];

        (int[] path, double length) result = GetInitialResult(distanceMatrix);
        UpdatePheremones(pheremoneMatrix, result.length);

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
                Console.WriteLine($"Iteration: {i}, New Length: {shortestPath.length}");
            }

            AddPheremones(pheremoneMatrix, shortestPath.path, shortestPath.length);
            UpdatePheremones(pheremoneMatrix, result.length);
        }

        return result;
    }

    static void UpdatePheremones(double[,] pheremoneMatrix, double shortestPathLength)
    {
        var max = PheremoneDepositWeight / shortestPathLength;
        var min = max * MinScalingFactor;
        
        for (int i = 0; i < pheremoneMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < pheremoneMatrix.GetLength(1); j++)
            {
                pheremoneMatrix[i, j] = Math.Clamp(pheremoneMatrix[i, j] * (1 - Rho), min, max);
            }
        }
    }

    static void AddPheremones(double[,] pheremoneMatrix, int[] path, double pathLength, double weight = 1)
    {
        var pheremoneDeposit = PheremoneDepositWeight * weight / (pathLength + 0.05);

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
        var nextNode = new Random().Next(nodeCount);
        var path = new int[nodeCount];
        path[0] = nextNode;

        var remainingNodes = Enumerable.Range(0, distanceMatrix.GetLength(0)).ToList();
        remainingNodes.Remove(nextNode);

        for (int i = 1; i < nodeCount; i++)
        {
            nextNode = GetNextNode(distanceMatrix, pheremoneMatrix, nextNode, remainingNodes);
            path[i] = nextNode;

            remainingNodes.Remove(nextNode);
        }

        return [.. path];
    }

    static int GetNextNode(double[,] distanceMatrix, double[,] pheremoneMatrix, int currentNode, List<int> remainingNodes)
    {
        var probabilities = remainingNodes.Select(node => CalculateRelativeProbability(distanceMatrix, pheremoneMatrix, currentNode, node)).ToArray();

        var randomValue = new Random().NextDouble() * probabilities.Sum();
        var cumulativeValue = 0.0;
        for (int i = 0; i < remainingNodes.Count - 1; i++)
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

    static (int[] path, double length) GetInitialResult(double[,] distanceMatrix)
    {
        var path = Enumerable.Range(0, distanceMatrix.GetLength(0)).ToArray();
        return (path, GetPathLength(distanceMatrix, path));
    }
}
