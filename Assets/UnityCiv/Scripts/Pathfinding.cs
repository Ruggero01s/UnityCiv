using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;
public class Pathfinding
{
    List<HexagonGame> openList;
    List<HexagonGame> closedList;

    HexagonGame[,] hexes;
    int xMax;
    int yMax;

    int MOVE_COST = 1;

    GridController gridController;

    public Pathfinding(GridController gridController, HexagonGame[,] hexGrid, int xMax, int yMax)
    {
        this.gridController = gridController;
        hexes = hexGrid;
        this.xMax = xMax;
        this.yMax = yMax;
    }

    public List<HexagonGame> FindPath(HexagonGame startNode, HexagonGame endNode)
    {
        openList = new List<HexagonGame> { startNode };
        closedList = new List<HexagonGame>();

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

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalcFCost();

        while (openList.Count > 0)
        {
            HexagonGame currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode)
                return CalculatePath(endNode);

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            foreach (HexagonGame neighbour in gridController.GetGameNeighbors(currentNode))
            {
                if (neighbour != null)
                {
                    if (neighbour.CompareTag("Occupied") && neighbour == endNode)
                        return CalculatePath(currentNode);
                    else
                    {
                        if (neighbour.CompareTag("MovableTerrain"))
                        {

                            if (closedList.Contains(neighbour)) continue;

                            int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbour);
                            if (tentativeGCost < neighbour.gCost)
                            {
                                neighbour.cameFromHex = currentNode;
                                neighbour.gCost = tentativeGCost;
                                neighbour.hCost = CalculateDistanceCost(neighbour, endNode);
                                neighbour.CalcFCost();

                                if (!openList.Contains(neighbour))
                                    openList.Add(neighbour);
                            }
                        }
                    }
                }
            }
        }

        // out of nodes on open list, no path
        return null;
    }

    private List<HexagonGame> CalculatePath(HexagonGame endNode)
    {
        List<HexagonGame> path = new List<HexagonGame>();
        path.Add(endNode);
        HexagonGame currentNode = endNode;
        while (currentNode.cameFromHex != null)
        {
            path.Add(currentNode.cameFromHex);
            currentNode = currentNode.cameFromHex;
        }
        path.Reverse();
        return path;
    }

    private HexagonGame GetLowestFCostNode(List<HexagonGame> nodeList)
    {
        HexagonGame lowestFCostNode = nodeList[0];
        for (int i = 0; i < nodeList.Count; i++)
        {
            if (nodeList[i].fCost < lowestFCostNode.fCost)
                lowestFCostNode = nodeList[i];
        }
        return lowestFCostNode;
    }

    private int CalculateDistanceCost(HexagonGame start, HexagonGame end)
    {

        return Mathf.RoundToInt(MOVE_COST * Vector3.Distance(start.rawPosition, end.rawPosition));
    }
}
