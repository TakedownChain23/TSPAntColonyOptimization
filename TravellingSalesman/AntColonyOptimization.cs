namespace TravellingSalesman;

public static class AntColonyOptimization
{
    readonly static int ColonySize = 400;
    readonly static int Iterations = 400;

    // Weights for pheromone and distance heuristic
    readonly static double Alpha = 1;
    readonly static double Beta = 3;

    readonly static double PheromoneEvaporationRate = 0.2;
    readonly static double PheromoneDepositWeight = 1;

    // Min value for pheromone deposit relative to max
    readonly static double MinScalingFactor = 0.001;

    public static (int[] nodes, double length) RunTSP(double[,] distanceMatrix, int seed = 42)
    {
        var nodeCount = distanceMatrix.GetLength(0);
        var pheromoneMatrix = new double[nodeCount, nodeCount];

        (int[] nodes, double length) result = GetInitialResult(distanceMatrix);
        UpdatePheromones(pheromoneMatrix, result.length);

        for (int i = 0; i < Iterations; i++)
        {
            var paths = new (int[] nodes, double length)[ColonySize];

            // Construct path for each ant in parallel
            Parallel.For(0, ColonySize, j =>
            {
                var pathNodes = ConstructPath(distanceMatrix, pheromoneMatrix, new(GenerateSeed(i, j, seed)));
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
            AddPheromones(pheromoneMatrix, shortestPath.nodes, shortestPath.length);
            UpdatePheromones(pheromoneMatrix, result.length);
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
    static int[] ConstructPath(double[,] distanceMatrix, double[,] pheromoneMatrix, Random randomGenerator)
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
            nextNode = GetNextNode(distanceMatrix, pheromoneMatrix, nextNode, remainingNodes, randomGenerator);
            pathNodes[i] = nextNode;

            remainingNodes.Remove(nextNode);
        }

        return pathNodes;
    }

    // Get the attractiveness of each remaining node, and choose one with roulette wheel selection
    static int GetNextNode(double[,] distanceMatrix, double[,] pheromoneMatrix, int currentNode, List<int> remainingNodes, Random randomGenerator)
    {
        var probabilities = remainingNodes.Select(n => CalculateRelativeProbability(distanceMatrix, pheromoneMatrix, currentNode, n)).ToArray();

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
    static double CalculateRelativeProbability(double[,] distanceMatrix, double[,] pheromoneMatrix, int currentNode, int nextNode)
    {
        return Math.Pow(pheromoneMatrix[currentNode, nextNode], Alpha) * Math.Pow(1.0 / (distanceMatrix[currentNode, nextNode] + 0.001), Beta);
    }

    // Add pheromones on the edges of a path
    static void AddPheromones(double[,] pheromoneMatrix, int[] pathNodes, double pathLength)
    {
        var pheromoneDeposit = PheromoneDepositWeight / (pathLength + 0.001);

        for (int i = 0; i < pathNodes.Length - 1; i++)
        {
            pheromoneMatrix[pathNodes[i], pathNodes[i + 1]] += pheromoneDeposit;
            pheromoneMatrix[pathNodes[i + 1], pathNodes[i]] += pheromoneDeposit;
        }

        pheromoneMatrix[pathNodes[^1], pathNodes[0]] += pheromoneDeposit;
        pheromoneMatrix[pathNodes[0], pathNodes[^1]] += pheromoneDeposit;
    }

    // Evaporate pheromones and clamp with min and max bounds
    static void UpdatePheromones(double[,] pheromoneMatrix, double shortestPathLength)
    {
        var max = PheromoneDepositWeight / shortestPathLength;
        var min = max * MinScalingFactor;
        
        for (int i = 0; i < pheromoneMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < pheromoneMatrix.GetLength(1); j++)
            {
                pheromoneMatrix[i, j] = Math.Clamp(pheromoneMatrix[i, j] * (1 - PheromoneEvaporationRate), min, max);
            }
        }
    }
}
