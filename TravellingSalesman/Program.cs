using System.Diagnostics;
using TravellingSalesman;

const string filePath = "TSPLIB/easy25.tsp";

var distanceMatrix = TSPHelper.ParseTSPFile(filePath);

var stopwatch = Stopwatch.StartNew();
Console.WriteLine("Running Ant Colony Optimization for TSP...\n");

// Run the TSP solver
var (path, pathLength) = AntColonyOptimization.RunTSP(distanceMatrix);
stopwatch.Stop();

// Display results
Console.WriteLine($"Optimal Path Length: {pathLength}");
Console.WriteLine($"Optimal Path: {string.Join(" -> ", path)} -> {path[0]}");
Console.WriteLine($"Execution Time: {stopwatch.Elapsed.TotalSeconds} seconds\n");