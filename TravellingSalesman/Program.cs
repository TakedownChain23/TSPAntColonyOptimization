using System;
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
Console.WriteLine($"Optimal Path: {string.Join(" -> ", path)}");
Console.WriteLine($"Execution Time: {stopwatch.Elapsed.TotalSeconds} seconds");

stopwatch = Stopwatch.StartNew();
Console.WriteLine("Running Christofides Algorithm for TSP...\n");

// Run the TSP solver
var (path2, pathLength2) = Christofides.RunAlgorithm(distanceMatrix);
stopwatch.Stop();

// Display results
Console.WriteLine($"Optimal Path Length: {pathLength2}");
Console.WriteLine($"Optimal Path: {string.Join(" -> ", path2)}");
Console.WriteLine($"Execution Time: {stopwatch.Elapsed.TotalSeconds} seconds");