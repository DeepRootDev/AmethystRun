using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RotatingBlade : ActiveObject
{
    public Vector3 rotationAxis = Vector3.zero;
    public float rotationSpeed = 100f;
    public bool activeRotation = false;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void Activate()
    {
        
        activeRotation = !activeRotation;
    }

    public void FixedUpdate()
    {
        if(activeRotation)
        {
            Quaternion newRotation = Quaternion.Euler(rotationAxis * rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(rb.rotation * newRotation);
        }
        
    }
}
