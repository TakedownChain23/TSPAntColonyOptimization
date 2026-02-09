namespace TravellingSalesman;

public static class AntColonyOptimization
{
    readonly static int ColonySize = 400;
    readonly static int Iterations = 400;

    readonly static double Alpha = 1;
    readonly static double Beta = 2;

    readonly static double Rho = 0.2;

    readonly static double PheremoneDepositWeight = 1;

    readonly static double MinScalingFactor = 0.001;

    public static (int[] nodes, double length) RunTSP(double[,] distanceMatrix)
    {
        var nodeCount = distanceMatrix.GetLength(0);
        var pheremoneMatrix = new double[nodeCount, nodeCount];

        (int[] nodes, double length) result = GetInitialPathResult(distanceMatrix);
        UpdatePheremones(pheremoneMatrix, result.length);

        for (int i = 0; i < Iterations; i++)
        {
            var paths = new List<(int[] nodes, double length)>();

            Parallel.For(0, ColonySize, _ =>
            {
                var pathNodes = ConstructPath(distanceMatrix, pheremoneMatrix);
                paths.Add((pathNodes, GetPathLength(distanceMatrix, pathNodes)));
            });

            var shortestPath = paths.MinBy(p => p.length);
            if (shortestPath.length < result.length)
            {
                result = shortestPath;
                Console.WriteLine($"Iteration: {i}, New Length: {shortestPath.length}");
            }

            AddPheremones(pheremoneMatrix, shortestPath.nodes, shortestPath.length);
            UpdatePheremones(pheremoneMatrix, result.length);
        }

        return result;
    }

    static (int[] path, double length) GetInitialPathResult(double[,] distanceMatrix)
    {
        var nodes = Enumerable.Range(0, distanceMatrix.GetLength(0)).ToArray();
        return (nodes, GetPathLength(distanceMatrix, nodes));
    }

    static double GetPathLength(double[,] distanceMatrix, int[] nodes)
    {
        var length = distanceMatrix[nodes[^1], nodes[0]];
        for (int i = 0; i < nodes.Length - 1; i++)
        {
            length += distanceMatrix[nodes[i], nodes[i + 1]];
        }

        return length;
    }

    static int[] ConstructPath(double[,] distanceMatrix, double[,] pheremoneMatrix)
    {
        var nodeCount = distanceMatrix.GetLength(0);
        var nextNode = new Random().Next(nodeCount);
        var pathNodes = new int[nodeCount];
        pathNodes[0] = nextNode;

        var remainingNodes = Enumerable.Range(0, distanceMatrix.GetLength(0)).ToList();
        remainingNodes.Remove(nextNode);

        for (int i = 1; i < nodeCount; i++)
        {
            nextNode = GetNextNode(distanceMatrix, pheremoneMatrix, nextNode, remainingNodes);
            pathNodes[i] = nextNode;

            remainingNodes.Remove(nextNode);
        }

        return pathNodes;
    }

    static int GetNextNode(double[,] distanceMatrix, double[,] pheremoneMatrix, int currentNode, List<int> remainingNodes)
    {
        var probabilities = remainingNodes.Select(n => CalculateRelativeProbability(distanceMatrix, pheremoneMatrix, currentNode, n)).ToArray();

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

    static void AddPheremones(double[,] pheremoneMatrix, int[] pathNodes, double pathLength)
    {
        var pheremoneDeposit = PheremoneDepositWeight / (pathLength + 0.05);

        for (int i = 0; i < pathNodes.Length - 1; i++)
        {
            pheremoneMatrix[pathNodes[i], pathNodes[i + 1]] += pheremoneDeposit;
            pheremoneMatrix[pathNodes[i + 1], pathNodes[i]] += pheremoneDeposit;
        }

        pheremoneMatrix[pathNodes[^1], pathNodes[0]] += pheremoneDeposit;
        pheremoneMatrix[pathNodes[0], pathNodes[^1]] += pheremoneDeposit;
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
}
