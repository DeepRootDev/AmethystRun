using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMovement : MonoBehaviour
{
    //Using a waypoint system for this

    [SerializeField]
    private Transform target;

    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private Vector3 moveDirection;

    [SerializeField]
    private float rotateSpeed = 10f;

    [SerializeField]
    private Quaternion lookRotation;

    [SerializeField]
    private float moveSpeed;

    [SerializeField]
    private GameObject floorSensor;

    [SerializeField]
    private bool onGround = true;

    [SerializeField]
    private float groundDrag = 5.0f;

    [SerializeField]
    private float airDrag = 1.0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    { 
        //Orient towards the target (at least on a plane)
        moveDirection = (target.position - rb.position).normalized;
        lookRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed);

        //push "forward".
        rb.AddForce(moveSpeed * moveDirection, ForceMode.Acceleration);
    }

    private void Update()
    {
        if (floorSensor.GetComponent<DetectGround>().IsGroundDetected())
        {
            //If we're on the ground, on the previous frame, were we already grounded?
            //No? don't do anything to the jumpCount.
            /*
            if (!onGround)
            {
                jumpCount = 0;
                numberOfJumpsAllowed = 1;
            }*/
            onGround = true;
            rb.drag = groundDrag;

        }
        else
        {
            rb.drag = airDrag;
            onGround = false;

        }
    }

    public void NextTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
