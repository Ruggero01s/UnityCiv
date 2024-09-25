using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public int hp;
    public int strength;
    private Animator animator;
    public List<HexagonGame> path = new List<HexagonGame>();

    public Vector2Int coordinates;
    public Vector3 rawPosition;

    public float speed=20f;
    public float rotationSpeed = 700f; // Speed at which the character rotates

    public bool isMoving = false;

    public int movementUnits = 3;

    void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        animator.SetBool("isMoving", isMoving);

        if(path.Count > 0){
            TravelPath();
        }else{
            isMoving = false;
        }
    }

    public void TravelPath(){
        isMoving = true;
        if (Vector3.Distance(transform.position, path[0].rawPosition) < 2.37f)
        {
            coordinates = path[0].coordinates;
            path.Remove(path[0]);
        }
        else MoveTo(path[0]);
    }

    public void MoveTo(HexagonGame terrain)
    {
        Vector3 newPosition = terrain.rawPosition;
        newPosition[1] = newPosition[1]+2.36f;
        var step =  speed * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, newPosition, step);
        Vector3 direction = (newPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }
}