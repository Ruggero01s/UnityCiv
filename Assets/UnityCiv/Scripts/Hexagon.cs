using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

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
