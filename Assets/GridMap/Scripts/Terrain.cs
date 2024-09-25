using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terrain : MonoBehaviour
{
    public int type;
    public Vector3 position;

    void Start()
    {
        position = transform.position;
    }
}
