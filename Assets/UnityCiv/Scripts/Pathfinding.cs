using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    List<HexagonGame> openList;  // Nodes to evaluate
    List<HexagonGame> closedList; // Evaluated nodes

    HexagonGame[,] hexes; // 2D grid of hexes
    int xMax; // Max width of the grid
    int yMax; // Max height of the grid

    const int MOVE_COST = 1; // Cost per move

    GridController gridController; // Reference to the grid controller

    public Pathfinding(GridController gridController, HexagonGame[,] hexGrid, int xMax, int yMax)
    {
        this.gridController = gridController;
        hexes = hexGrid;
        this.xMax = xMax;
        this.yMax = yMax;
    }

    public List<HexagonGame> FindPath(HexagonGame startNode, HexagonGame endNode)
    {
        openList = new List<HexagonGame> { startNode }; // Initialize open list
        closedList = new List<HexagonGame>(); // Initialize closed list

        // Set initial costs for all hexes
        for (int x = 0; x < xMax; x++)
        {
            for (int y = 0; y < yMax; y++)
            {
                HexagonGame hex = hexes[x, y];
                hex.gCost = int.MaxValue;
                hex.CalcFCost();
                hex.cameFromHex = null;
            }
        }

        // Set costs for the start node
        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalcFCost();

        while (openList.Count > 0)
        {
            HexagonGame currentNode = GetLowestFCostNode(openList); // Get node with lowest fCost
            if (currentNode == endNode)
                return CalculatePath(endNode); // Path found

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (HexagonGame neighbour in gridController.GetGameNeighbors(currentNode))
            {
                if (neighbour != null)
                {
                    // Handle special cases for occupied nodes
                    if ((neighbour.CompareTag("Occupied") || neighbour.CompareTag("PlayerCity")) && neighbour == endNode)
                        return CalculatePath(currentNode);

                    if (neighbour.CompareTag("MovableTerrain") && !closedList.Contains(neighbour))
                    {
                        int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbour);
                        if (tentativeGCost < neighbour.gCost)
                        {
                            neighbour.cameFromHex = currentNode;
                            neighbour.gCost = tentativeGCost;
                            neighbour.hCost = CalculateDistanceCost(neighbour, endNode);
                            neighbour.CalcFCost();

                            if (!openList.Contains(neighbour))
                                openList.Add(neighbour); // Add to open list if not already present
                        }
                    }
                }
            }
        }

        return null; // No path found
    }

    private List<HexagonGame> CalculatePath(HexagonGame endNode)
    {
        List<HexagonGame> path = new() { endNode };
        HexagonGame currentNode = endNode;
        while (currentNode.cameFromHex != null)
        {
            path.Add(currentNode.cameFromHex);
            currentNode = currentNode.cameFromHex;
        }
        path.Reverse(); // Reverse to get path from start to end
        return path;
    }

    private HexagonGame GetLowestFCostNode(List<HexagonGame> nodeList)
    {
        HexagonGame lowestFCostNode = nodeList[0];
        foreach (HexagonGame node in nodeList)
        {
            if (node.fCost < lowestFCostNode.fCost)
                lowestFCostNode = node; // Find node with lowest fCost
        }
        return lowestFCostNode;
    }

    private int CalculateDistanceCost(HexagonGame start, HexagonGame end)
    {
        return Mathf.RoundToInt(MOVE_COST * Vector3.Distance(start.rawPosition, end.rawPosition)); // Calculate distance cost
    }
}
