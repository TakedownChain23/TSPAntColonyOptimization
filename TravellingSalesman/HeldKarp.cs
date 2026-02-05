namespace TravellingSalesman;

public class HeldKarp
{
    Tuple<int, List<int>> RunAlgorithm(List<List<int>> nodeDistances, int startNode)
    {
        var nodeCount = nodeDistances.Count;

        object memoTable = "2D table of size n by 2^n";
        
        Setup(nodeDistances, memoTable, startNode, nodeCount);
        Solve(nodeDistances, memoTable, startNode, nodeCount);
        
        var minimumCost = FindMinimumCost(nodeDistances, memoTable, startNode, nodeCount);
        var optimalTour = FindOptimalTour(nodeDistances, memoTable, startNode, nodeCount);
        return (minimumCost, optimalTour).ToTuple();
    }

    void Setup(List<List<int>> distances, object memoTable, int startNode, int nodeCount)
    {
    }

    void Solve(List<List<int>> distances, object memoTable, int startNode, int nodeCount)
    {
    }
    
    int FindMinimumCost(List<List<int>> distances, object memoTable, int startNode, int nodeCount)
    {
        return 0;
    }

    List<int> FindOptimalTour(List<List<int>> distances, object memoTable, int startNode, int nodeCount)
    {
        return null;
    }
}