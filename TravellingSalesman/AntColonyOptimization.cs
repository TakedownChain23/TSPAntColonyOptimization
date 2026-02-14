namespace TravellingSalesman;

public static class AntColonyOptimization
{
    readonly static int ColonySize = 400;
    readonly static int Iterations = 400;

    // Weights for pheromone and distance heuristic
    readonly static double Alpha = 1;
    readonly static double Beta = 3;

    readonly static double PheremoneEvaporationRate = 0.2;
    readonly static double PheremoneDepositWeight = 1;

    // Min value for pheremone deposit relative to max
    readonly static double MinScalingFactor = 0.001;

    public static (int[] nodes, double length) RunTSP(double[,] distanceMatrix, int seed = 42)
    {
        var nodeCount = distanceMatrix.GetLength(0);
        var pheremoneMatrix = new double[nodeCount, nodeCount];

        (int[] nodes, double length) result = GetInitialResult(distanceMatrix);
        UpdatePheremones(pheremoneMatrix, result.length);

        for (int i = 0; i < Iterations; i++)
        {
            var paths = new (int[] nodes, double length)[ColonySize];

            // Construct path for each ant in parallel
            Parallel.For(0, ColonySize, j =>
            {
                var pathNodes = ConstructPath(distanceMatrix, pheremoneMatrix, new(GenerateSeed(i, j, seed)));
                var pathLength = GetPathLength(distanceMatrix, pathNodes);

                paths[j] = (pathNodes, pathLength);
            });

            // Set result to new shortest path
            var shortestPath = paths.MinBy(p => p.length);
            if (shortestPath.length < result.length)
            {
                result = shortestPath;
                Console.WriteLine($"New Length: {shortestPath.length:F3} (Iteration: {i})");
            }

            // Deposit pheromone on the interations best path and evaporate globally
            AddPheremones(pheremoneMatrix, shortestPath.nodes, shortestPath.length);
            UpdatePheremones(pheremoneMatrix, result.length);
        }

        return result;
    }

    // Generate a seed given 2 loop indices
    static int GenerateSeed(int i, int j, int baseSeed)
    {
        unchecked
        {
            return (baseSeed * 73856093) ^ (i * 19349663) ^ (j * 83492791);
        }
    }

    // Get random starting path
    static (int[] nodes, double length) GetInitialResult(double[,] distanceMatrix)
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

    // Build a path with each node once
    static int[] ConstructPath(double[,] distanceMatrix, double[,] pheremoneMatrix, Random randomGenerator)
    {
        var nodeCount = distanceMatrix.GetLength(0);
        var nextNode = randomGenerator.Next(nodeCount);
        var pathNodes = new int[nodeCount];
        pathNodes[0] = nextNode;

        var remainingNodes = Enumerable.Range(0, distanceMatrix.GetLength(0)).ToList();
        remainingNodes.Remove(nextNode);

        for (int i = 1; i < nodeCount; i++)
        {
            // Select the next node using heuristic
            nextNode = GetNextNode(distanceMatrix, pheremoneMatrix, nextNode, remainingNodes, randomGenerator);
            pathNodes[i] = nextNode;

            remainingNodes.Remove(nextNode);
        }

        return pathNodes;
    }

    // Get the attractiveness of each remaining node, and choose one with roulette wheel selection
    static int GetNextNode(double[,] distanceMatrix, double[,] pheremoneMatrix, int currentNode, List<int> remainingNodes, Random randomGenerator)
    {
        var probabilities = remainingNodes.Select(n => CalculateRelativeProbability(distanceMatrix, pheremoneMatrix, currentNode, n)).ToArray();

        var randomValue = randomGenerator.NextDouble() * probabilities.Sum();
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

    // Pheromone and distance heuristic for edge attractiveness
    static double CalculateRelativeProbability(double[,] distanceMatrix, double[,] pheremoneMatrix, int currentNode, int nextNode)
    {
        return Math.Pow(pheremoneMatrix[currentNode, nextNode], Alpha) * Math.Pow(1.0 / (distanceMatrix[currentNode, nextNode] + 0.001), Beta);
    }

    // Add pheremones on the edges of a path
    static void AddPheremones(double[,] pheremoneMatrix, int[] pathNodes, double pathLength)
    {
        var pheremoneDeposit = PheremoneDepositWeight / (pathLength + 0.001);

        for (int i = 0; i < pathNodes.Length - 1; i++)
        {
            pheremoneMatrix[pathNodes[i], pathNodes[i + 1]] += pheremoneDeposit;
            pheremoneMatrix[pathNodes[i + 1], pathNodes[i]] += pheremoneDeposit;
        }

        pheremoneMatrix[pathNodes[^1], pathNodes[0]] += pheremoneDeposit;
        pheremoneMatrix[pathNodes[0], pathNodes[^1]] += pheremoneDeposit;
    }

    // Evaporate pheromones and clamp with min and max bounds
    static void UpdatePheremones(double[,] pheremoneMatrix, double shortestPathLength)
    {
        var max = PheremoneDepositWeight / shortestPathLength;
        var min = max * MinScalingFactor;
        
        for (int i = 0; i < pheremoneMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < pheremoneMatrix.GetLength(1); j++)
            {
                pheremoneMatrix[i, j] = Math.Clamp(pheremoneMatrix[i, j] * (1 - PheremoneEvaporationRate), min, max);
            }
        }
    }
}
