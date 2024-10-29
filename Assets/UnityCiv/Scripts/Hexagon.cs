using System.Collections.Generic;
using UnityEngine;

// Class to be used in generation before hexTypes are assigned, represents a simple hex
public class Hexagon
{

    public Vector2Int coordinates;
    public GameObject hexType;

    public Vector3 rawPosition;

    public List<Hexagon> neighbors;
    public Hexagon(Vector2Int coords)
    {
        coordinates = coords;
    }
}
