using NUnit.Framework;
using System.Collections.Generic;
using TravellingSalesman;

namespace TravellingSalesmanTest
{
    [TestFixture]
    public class ChristofidesTests
    {
        [Test]
        public void ComputeMinimumSpanningTreeReturnsSquareMatrix()
        {
            var distances = TestGraphs.SquareGraph();
            var mst = Christofides.ComputeMinimumSpanningTree(distances);

            Assert.That(mst.GetLength(0), Is.EqualTo(4));
            Assert.That(mst.GetLength(1), Is.EqualTo(4));
        }

        [Test]
        public void ComputeMinimumSpanningTreeHasExactlyNMinusOneEdges()
        {
            var distances = TestGraphs.SquareGraph();
            var mst = Christofides.ComputeMinimumSpanningTree(distances);

            var edgeCount = 0;
            for (var i = 0; i < 4; i++)
            for (var j = i + 1; j < 4; j++)
                edgeCount += mst[i, j];

            Assert.That(edgeCount, Is.EqualTo(3));
        }

        [Test]
        public void FindOddDegreeVerticesReturnsEvenNumberOfVertices()
        {
            var distances = TestGraphs.SquareGraph();
            var mst = Christofides.ComputeMinimumSpanningTree(distances);
            var oddVertices = Christofides.FindOddDegreeVertices(mst);

            Assert.That(oddVertices.Count % 2, Is.EqualTo(0));
        }

        [Test]
        public void FindOddDegreeVerticesAllReturnedVerticesHaveOddDegree()
        {
            var distances = TestGraphs.SquareGraph();
            var mst = Christofides.ComputeMinimumSpanningTree(distances);
            var oddVertices = Christofides.FindOddDegreeVertices(mst);

            foreach (var u in oddVertices)
            {
                var degree = 0;
                for (var v = 0; v < mst.GetLength(0); v++)
                    degree += mst[u, v];

                Assert.That(degree % 2, Is.EqualTo(1));
            }
        }

        [Test]
        public void RunBlossomMatchesAllVerticesExactlyOnce()
        {
            var distances = TestGraphs.SquareGraph();
            var vertices = new List<int> { 0, 1, 2, 3 };

            var matching = Christofides.RunBlossom(vertices, distances);

            var seen = new HashSet<int>();
            foreach (var (u, v) in matching)
            {
                Assert.That(seen, Does.Not.Contain(u));
                Assert.That(seen, Does.Not.Contain(v));
                seen.Add(u);
                seen.Add(v);
            }

            Assert.That(vertices.Count, Is.EqualTo(seen.Count));
        }

        [Test]
        public void FindEulerianTourStartsAndEndsAtSameVertex()
        {
            var distances = TestGraphs.SquareGraph();
            var mst = Christofides.ComputeMinimumSpanningTree(distances);
            var odd = Christofides.FindOddDegreeVertices(mst);
            var matching = Christofides.RunBlossom(odd, distances);

            Christofides.AddMatchingToGraph(mst, matching);
            var tour = Christofides.FindEulerianTour(mst);

            Assert.That(tour.Count, Is.GreaterThan(1));
            Assert.That(tour[0], Is.EqualTo(tour[^1]));
        }

        [Test]
        public void ShortcutEulerianTourVisitsAllVerticesExactlyOnce()
        {
            var distances = TestGraphs.RandomMetricGraph(10);
            var (tour, _) = Christofides.RunAlgorithm(distances);

            // Asserts full cycle, and that hashset (which won't have duplicate values) has the same count, so every vertex is present once
            var uniqueVertices = new HashSet<int>(tour);
            Assert.That(distances.GetLength(0) + 1, Is.EqualTo(tour.Length));
            Assert.That(tour[0], Is.EqualTo(tour[^1]));
            Assert.That(distances.GetLength(0), Is.EqualTo(uniqueVertices.Count));
        }

        [Test]
        public void ShortcutEulerianTourReturnsFinitePositiveCost()
        {
            var distances = TestGraphs.RandomMetricGraph(10);
            var (_, cost) = Christofides.RunAlgorithm(distances);

            // Assert cost is a valid value
            Assert.That(cost, Is.GreaterThan(0));
            Assert.That(double.IsNaN(cost), Is.False);
            Assert.That(double.IsInfinity(cost), Is.False);
        }

        [Test]
        public void RunAlgorithmReturnsValidHamiltonianCycle()
        {
            var distances = TestGraphs.RandomMetricGraph(20);

            var (tour, length) = Christofides.RunAlgorithm(distances);

            // Assert first and last vertex are the same, the tour length is all vertices + the return trip, and length is greater than 0
            Assert.That(tour[0], Is.EqualTo(tour[^1]));
            Assert.That(distances.GetLength(0) + 1, Is.EqualTo(tour.Length));
            Assert.That(length, Is.GreaterThan(0));
        }

        [Test]
        public void RunAlgorithmIsDeterministicForSameInput()
        {
            var distances = TestGraphs.RandomMetricGraph(15);

            // Run Christofides twice with same input
            var result1 = Christofides.RunAlgorithm(distances);
            var result2 = Christofides.RunAlgorithm(distances);

            // Verify the path and cost are the same for both
            Assert.That(result1.length, Is.EqualTo(result2.length));
            Assert.That(result1.tour, Is.EquivalentTo(result2.tour));
        }
    }
}