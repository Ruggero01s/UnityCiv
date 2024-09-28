using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public Player owner; // The player or faction that controls this unit
    public int hp;
    public int strength;
    private Animator animator;
    
    // The path the unit will follow (set externally when a destination is selected)
    public List<HexagonGame> path = new List<HexagonGame>();

    public Vector2Int coordinates;  // Current hex grid coordinates
    public Vector3 rawPosition;     // Actual position on the map
    
    public float speed = 20f;        // Movement speed
    public float rotationSpeed = 700f; // Rotation speed for smooth turning

    public bool isMoving = false;   // Whether the unit is currently moving

    // Movement-related variables
    public int movementUnits = 3;   // Maximum movement units per turn
    public int movementExpended = 0;// Movement points used in the current turn

    private bool destinationReached = false; // Flag for destination status

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Update animation based on movement status
        animator.SetBool("isMoving", isMoving);

        // If there's a path and movement points left, continue moving
        if (path.Count > 0 && movementExpended < movementUnits)
        {
            TravelPath();
        }
        else
        {
            // If no more path or movement points left, stop moving
            isMoving = false;
            path.Clear();
        }
    }

    // Function to move along the path
    public void TravelPath()
    {
        isMoving = true;

        // Move toward the next tile if it's not reached
        if (Vector3.Distance(transform.position, path[0].rawPosition) < 2.37f)
        {
            // Once a tile is reached, update the unit's current coordinates and expend movement
            coordinates = path[0].coordinates;
            path[0].ClaimHex(owner);
            movementExpended++;    // Reduce movement units as you move
            
            // Remove the first tile from the path (as it is now reached)
            path.RemoveAt(0);

            // If the path is empty after removal, the destination is reached
            if (path.Count == 0)
            {
                destinationReached = true;
                isMoving = false;
            }
        }
        else
        {
            // If not reached, move towards the next tile
            MoveTo(path[0]);
        }
    }

    // Moves the unit towards a specific tile (HexagonGame)
    public void MoveTo(HexagonGame targetTile)
    {
        Vector3 targetPosition = targetTile.rawPosition;
        targetPosition[1] = targetPosition[1] + 2.36f; // Adjust height for 3D

        // Smooth movement
        var step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        // Smooth rotation towards the target
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    // Called when a new destination is selected
    public void SetDestination(List<HexagonGame> newPath)
    {
        // Reset movement expended for the turn and set new path
        path = newPath;
        isMoving = true;
        destinationReached = false;
    }

    // This function can be called at the start of each new turn to refresh movement points
    public void NewTurn()
    {
        movementExpended = 0;
        isMoving = false;
        destinationReached = false;
    }

    // Check if the unit has reached its destination
    public bool HasReachedDestination()
    {
        return destinationReached;
    }
}
