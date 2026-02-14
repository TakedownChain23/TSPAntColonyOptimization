using TravellingSalesman;

namespace TravellingSalesmanTest;

[TestFixture]
public class AntColonyOptimizationTest
{
    [Test]
    public void TestRunTSP_ReturnsValidPath()
    {
        var distanceMatrix = TestGraphs.RandomMetricGraph(25);
        var (nodes, _) = AntColonyOptimization.RunTSP(distanceMatrix);

        Assert.That(nodes, Has.Length.EqualTo(25));
        for (int i = 0; i < 25; i++)
        {
            Assert.That(nodes, Does.Contain(i));
        }
    }

    [Test]
    public void TestRunTSP_ReturnsCorrectLength()
    {
        var distanceMatrix = TestGraphs.RandomMetricGraph(25);
        var (nodes, length) = AntColonyOptimization.RunTSP(distanceMatrix);

        var expectedLength = GetPathLength(distanceMatrix, nodes);

        Assert.That(length, Is.EqualTo(expectedLength).Within(1e-6));
    }

    [Test]
    public void TestRunTSP_ReturnsOptimalPathForSmallGraph()
    {
        var distanceMatrix = TestGraphs.SquareGraph();
        var (_, length) = AntColonyOptimization.RunTSP(distanceMatrix);

        Assert.That(length, Is.EqualTo(4).Within(1e-6));
    }

    [Test]
    public void TestRunTSP_ReturnsPathBetterThanRandom()
    {
        var distanceMatrix = TestGraphs.RandomMetricGraph(25);

        var randomPath = Enumerable.Range(0, 25).ToArray();
        var randomLength = GetPathLength(distanceMatrix, randomPath);

        var (_, antColonyLength) = AntColonyOptimization.RunTSP(distanceMatrix);
        Assert.That(antColonyLength, Is.LessThanOrEqualTo(randomLength));
    }

    [Test]
    public void TestRunTSP_ReturnsPathBetterThanNearestNeighbor()
    {
        var distanceMatrix = TestGraphs.RandomMetricGraph(25);

        var nearestNeighbourNodes = RunNearestNeighbor(distanceMatrix);
        var nearestNeighbourLength = GetPathLength(distanceMatrix, nearestNeighbourNodes);

        var (_, antColonyLength) = AntColonyOptimization.RunTSP(distanceMatrix);

        Assert.That(antColonyLength, Is.LessThanOrEqualTo(nearestNeighbourLength));
    }

    [Test]
    public void TestRunTSP_ReturnsDeterministicResultWithSameSeed()
    {
        var distanceMatrix = TestGraphs.RandomMetricGraph(25);
        
        var (nodes1, _) = AntColonyOptimization.RunTSP(distanceMatrix, seed: 123);
        var (nodes2, _) = AntColonyOptimization.RunTSP(distanceMatrix, seed: 123);

        Assert.That(nodes1, Is.EqualTo(nodes2));
    }

    static int[] RunNearestNeighbor(double[,] distanceMatrix)
    {
        var nextNode = 0;
        var path = new List<int>() { nextNode };
        var remainingNodes = Enumerable.Range(0, distanceMatrix.GetLength(0)).ToList();
        remainingNodes.Remove(nextNode);

        for (int i = 1; i < distanceMatrix.GetLength(0); i++)
        {
            nextNode = remainingNodes.MinBy(n => distanceMatrix[nextNode, n]);
            path.Add(nextNode);
            remainingNodes.Remove(nextNode);
        }

        return [.. path];
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
}