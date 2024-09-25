using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexagonGame : MonoBehaviour
{

    Color startColor;

    Renderer rend;

    public Vector2Int coordinates;
    public GameObject hexType;

    public Vector3 rawPosition;

    public List<Hexagon> neighbors;
    float lightenFactor = 1.5f; 

    public int gCost;
    public int hCost;
    public int fCost;

    public HexagonGame cameFromHex;

    public override string ToString()
    {
        return coordinates.x+","+coordinates.y;
    }
    void Start()
    {
        rawPosition = transform.position;
        rend = GetComponent<Renderer>();
        startColor = rend.material.color;
    }


    // Update is called once per frame
    void Update()
    {
        
    }


    public void CalcFCost()
    {
        fCost = gCost+hCost;
    }

    void OnMouseEnter()
    {
        DarkenTexture();
    }

    void OnMouseExit()
    {
        RestoreTexture();
    }

    void DarkenTexture()
    {
        Color highlightColor = Color.white * lightenFactor;
        rend.material.color = highlightColor;
    }

    void RestoreTexture()
    {
        rend.material.color = startColor;
    }
    


}
