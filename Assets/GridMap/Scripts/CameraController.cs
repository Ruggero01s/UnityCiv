using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform cameraTransform;

    private float maxZoom = 1000f;
    private float minZoom = 10f;
    public float normalSpeed=0.4f;
    public float fastSpeed=1f;
    public float moveSpeed;
    public float moveTime=5f;
    public float rotationAmount=0.5f;
    public Vector3 newPosition;
    public Quaternion newRotation;

    public Vector3 zoomAmount=new Vector3(1,1,1);
    public Vector3 newZoom;
    // Start is called before the first frame update
    void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseInput();
        HandleMovementInput();
    }


    void HandleMovementInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            moveSpeed = fastSpeed;
        else 
            moveSpeed = normalSpeed;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            newPosition += (transform.forward * moveSpeed);
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            newPosition += (transform.forward * -moveSpeed);
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            newPosition += (transform.right * moveSpeed);
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            newPosition += (transform.right * -moveSpeed);
        
        if (Input.GetKey(KeyCode.Q)) 
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        if (Input.GetKey(KeyCode.E))
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
        
        if (Input.GetKey(KeyCode.R))
            newZoom += zoomAmount;
        if (Input.GetKey(KeyCode.F))
            newZoom -= zoomAmount;

        

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime*moveTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime*moveTime);
        
        if (newZoom[1]<minZoom)
            newZoom = new Vector3(0,minZoom, -minZoom);
        else  if (newZoom[1]>maxZoom)
            newZoom = new Vector3(0,maxZoom, -maxZoom);
        else 
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime*moveTime);
    }

    void HandleMouseInput(){
        if (Input.mouseScrollDelta.y != 0)
            newZoom += Input.mouseScrollDelta.y * zoomAmount;
    }
}
