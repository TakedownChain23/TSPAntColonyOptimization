namespace TravellingSalesman;

public class HeldKarp
{
    Tuple<int, List<int>> RunAlgorithm(List<List<int>> nodeDistances)
    {
        var nodeCount = nodeDistances.Count;
        var subsetCount = 1 << nodeCount;
        var badResultPlaceholder = int.MaxValue / 4; // Represents an incredibly suboptimal path so any new results will be chosen as best
        
        var optimalResults = new List<List<int>>(subsetCount);
        var tour = new List<List<int>>(subsetCount);
        
        for (var i = 0; i < subsetCount; i++)
        {
            optimalResults.Add(Enumerable.Repeat(badResultPlaceholder, nodeCount).ToList()); // Initialise results table with bad paths
            tour.Add(Enumerable.Repeat(-1, nodeCount).ToList()); // Initialise all paths as invalid
        }

        optimalResults[1][0] = 0; // Base case -> takes no distance to get to the start node

        for (var mask = 1; mask < subsetCount; mask++)
        {
            // Does this subset include the starting city

            for (int j = 1; j < nodeCount; j++) // Look through other paths
            {
                // Is the city j in the subset?
                
                // What is the previous subset if we end at city j
                var previousMask = mask ^ (1 << j);

                for (int k = 0; k < nodeCount; k++) // Look through previous paths
                {
                    // Was city k visited in the previous subset?
                    
                    // Is the path via city k cheaper than what we already know?
                    
                    // If yes then record new cost and path
                }
            }
        }

        return null;
    }
}