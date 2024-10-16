using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class Brain : MonoBehaviour
{
    PlayerMovement movement;
    NavMeshAgent agent;
    [SerializeField]
    Waypoint[] waypoints;
    [SerializeField]
    Waypoint NextWaypoint;
    [SerializeField]
    bool Gliding, OnWall , OnGround,Reached,Jump,Jumped;
    [SerializeField]
    Animator ModelAnimator;
    [SerializeField]
    int counter;
    Rigidbody rb;
    [SerializeField]
    DetectGround detectGround;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        movement = GetComponent<PlayerMovement>();
        movement.enabled = false;
        NextWaypoint = waypoints[0];
        agent.SetDestination(NextWaypoint.transform.position);
    }
    public void SetNewWaypoint()
    {
        
        if (counter < waypoints.Length-1)
        {
            counter++;
        }
        else
        {
            counter = 0;
        }
        NextWaypoint = waypoints[counter];
       
        if (NextWaypoint.Fly)
        {
            OnGround = false;
            rb.useGravity = false;
            agent.enabled = false;
            Gliding = true;
        }
        else if (NextWaypoint.Jump)
        {
            OnGround = false;

            rb.velocity = agent.velocity + (Vector3.up * 2);
            agent.enabled = false;
            rb.useGravity = true;
            Gliding = false;

            StartCoroutine(StartJump());

        }
        else
        {
            
            Gliding = false;
            OnGround = true;
            //agent.enabled = true;
            
        }
        
        Reached = false;
    }
    public void DistanceToWaypoint()
    {
        float distance = Vector3.Distance(transform.position,NextWaypoint.transform.position);
        if (distance <= 5 && !Reached)
        {
            Reached = true;
            SetNewWaypoint();
            
        }
        
    }
    IEnumerator StartJump()
    {
        

        yield return new WaitForSeconds(1);
        Jumped = true;
    }
    // Update is called once per frame
    void Update()
    {
        ModelAnimator.SetFloat("Forward", 1);
        ModelAnimator.SetBool("Gliding", Gliding);
        ModelAnimator.SetBool("GlideStarter", Gliding);
        if (detectGround.IsGroundDetected())
        {
            ModelAnimator.SetBool("Air",false);
        }
        else
        {
            ModelAnimator.SetBool("Air", true);
        }
        DistanceToWaypoint();
    }
    private void FixedUpdate()
    {
        if (NextWaypoint.Jump)
        {
            if (detectGround.IsGroundDetected())
            {
                //+rb.AddForce(transform.up * 5f, ForceMode.Acceleration);
                //rb.AddForce(transform.forward * 10f, ForceMode.Acceleration);
                if (Jumped)
                {
                    SetNewWaypoint();
                    Jumped = false;
                }
            }
        }
        if (Gliding)
        {
            Quaternion lookD = Quaternion.LookRotation(-transform.position, Vector3.up);
            transform.position = Vector3.MoveTowards(transform.position, NextWaypoint.transform.position, 11 * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookD, 5 * Time.deltaTime);
        }
        
        if (OnGround)
        {
            if (!detectGround.IsGroundDetected())
            {
                agent.enabled = false;
                rb.useGravity = true;
                rb.velocity = (transform.forward* 10f)+Physics.gravity;
            }
            else
            {
                agent.enabled = true;
                agent.SetDestination(NextWaypoint.transform.position);
            }
        }
    }
}
