using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexagonGame : MonoBehaviour
{
    Color startColor;
    Renderer rend;

    public Vector2Int coordinates;
    public GameObject hexType;

    public Vector3 rawPosition;
    float lightenFactor = 1.5f;

    public int gCost;
    public int hCost;
    public int fCost;

    public HexagonGame cameFromHex;

    // Add ownership
    public Player owner; // Player or Faction controlling the hex

    private GameObject overlay; // Reference to the overlay object

    public override string ToString()
    {
        return coordinates.x + "," + coordinates.y;
    }

    void Start()
    {
        rawPosition = transform.position;
        rend = GetComponent<Renderer>();
        startColor = rend.material.color;

        // Create and setup overlay
        CreateOverlay();
        RestoreHexColor();
    }

    void Update()
    {
        // You can update ownership or handle visuals here if needed
    }

    public void CalcFCost()
    {
        fCost = gCost + hCost;
    }

    void OnMouseEnter()
    {
        if(!EventSystem.current.IsPointerOverGameObject())
        {
            DarkenTexture();
        }
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
        RestoreHexColor();
    }

    // Function to create the overlay
    void CreateOverlay()
    {
        overlay = Instantiate(GridController.ownerOverlay);
        overlay.transform.SetParent(transform);
        overlay.transform.localPosition = new Vector3(0, 0.22f, 0); // Slightly above the hex

        // Set the scale to match the hex size
        overlay.transform.localScale = new Vector3(0.7f, 0.002f, 0.7f); // Adjust based on hex size
        overlay.transform.rotation = Quaternion.Euler(0, 0, 0); // Rotate to lay flat

        // Set the overlay material
        Renderer overlayRenderer = overlay.GetComponent<Renderer>();
        overlayRenderer.material.color = new Color(1, 1, 1, 0); // Initially transparent
        overlay.SetActive(false); // Hide overlay by default
    }


    // Function to change ownership of the hex
    public void ClaimHex(Player newOwner)
    {
        // If the hex is already owned, remove it from the previous owner’s list
        if (owner != null)
        {
            owner.RemoveControlledHex(this);
        }

        // Assign new ownership
        owner = newOwner;
        owner.AddControlledHex(this);

        // Update the hex color and overlay visibility
        UpdateHexColorBasedOnOwner();
    }

    // Restore the hex to its original color
    void RestoreHexColor()
    {
        rend.material.color = startColor; // Restore original color
    }

    // Update the hex color and overlay based on the current owner
    public void UpdateHexColorBasedOnOwner()
    {
        if (owner != null)
        {
            Color ownerColor = owner.ownedColor; // Assuming Player has a property for color
            overlay.GetComponent<Renderer>().material.color = new Color(ownerColor.r, ownerColor.g, ownerColor.b, 0.2f); // Set overlay color with some transparency
            overlay.SetActive(true); // Show overlay
        }
        else
        {
            overlay.SetActive(false); // No owner means restore to original
        }
    }
}
