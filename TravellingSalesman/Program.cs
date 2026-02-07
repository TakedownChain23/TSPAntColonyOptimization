using System.Diagnostics;
using TravellingSalesman;

const string FilePath = "TSPLIB/eil51.tsp";

var distanceMatrix = TSPHelper.ParseTSPFile(FilePath);

var stopwatch = Stopwatch.StartNew();
Console.WriteLine("Running Ant Colony Optimization for TSP...\n");

// Run the TSP solver
var (path, pathLength) = AntColonyOptimization.RunTSP(distanceMatrix);

// Display results
Console.WriteLine($"Optimal Path Length: {pathLength}");
Console.WriteLine($"Optimal Path: {string.Join(" -> ", path)}");
Console.WriteLine($"Execution Time: {stopwatch.Elapsed.TotalSeconds} seconds");