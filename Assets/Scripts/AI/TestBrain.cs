using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestBrain : MonoBehaviour
{
    public GhostData ghostData;
    [SerializeField]
    private GhostPath selectedPath;
    public float speed = 3f,DefSpeed;
    public float waypointThreshold = 2f;
    private int currentWaypointIndex = 0;
    Rigidbody rb;
    [SerializeField]
    Animator modelAnimator;
    [SerializeField]
    DetectGround floorSensor;
    void Start()
    {
        floorSensor = GetComponentInChildren<DetectGround>();
        modelAnimator = GetComponentInChildren<Animator>();
        DefSpeed = speed;
        // Load the ghost data before starting
        GhostRecorder recorder = FindObjectOfType<GhostRecorder>();
        recorder.LoadGhostData();
        ghostData = recorder.ghostData;
        SelectPathBasedOnCondition();
        rb = GetComponent<Rigidbody>();
    }
    void SelectPathBasedOnCondition()
    {
        
        selectedPath = ghostData.paths.OrderBy(path => path.frames.Count).FirstOrDefault();

    }
    void Update()
    {
        if (selectedPath == null || currentWaypointIndex >= selectedPath.frames.Count)
            return;
        ;
        
        GhostFrame target = selectedPath.frames[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position).normalized;
        
        if (!AvoidObstacles(ref direction))
        {
            CorrectPath(ref direction);
            
            
        }
        rb.AddForce(direction * speed, ForceMode.Acceleration);
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        
        modelAnimator.SetFloat("Forward", direction.magnitude);
        modelAnimator.SetBool("GlideStarter", selectedPath.frames[currentWaypointIndex].isGliding);
        if (selectedPath.frames[currentWaypointIndex].isGliding)
        {
            modelAnimator.SetBool("Gliding", true);
        }
        else
        {
            modelAnimator.SetBool("Gliding", false);
        }
        if (floorSensor.IsGroundDetected())
            modelAnimator.SetBool("Air", false);
        else
            modelAnimator.SetBool("Air", true);
        if (Vector3.Distance(transform.position, target.position) < waypointThreshold)
        {
            currentWaypointIndex++;
        }
    }
    int FindNearestWaypointIndex(Vector3 position)
    {
        int nearestIndex = 0;
        float shortestDistance = float.MaxValue;

        for (int i = 0; i < selectedPath.frames.Count; i++)
        {
            float distance = Vector3.Distance(position, selectedPath.frames[i].position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }
    void CorrectPath(ref Vector3 direction)
    {
        
        int nearestIndex = FindNearestWaypointIndex(transform.position);

        Vector3 pathCorrectionDirection = (selectedPath.frames[nearestIndex].position - transform.position).normalized;

        direction = Vector3.Lerp(direction, pathCorrectionDirection, Time.deltaTime * 2f);
    }
    bool AvoidObstacles(ref Vector3 direction)
    {
        float detectionDistance = 2f;
        Vector3[] rayDirections =
        {
        transform.forward,                 
        transform.forward + transform.right, 
        transform.forward - transform.right, 
        
    };
        foreach (var rayDir in rayDirections)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, rayDir.normalized, out hit, detectionDistance))
            {
                speed = Mathf.Lerp(speed, DefSpeed/2, Time.deltaTime);
                Debug.DrawRay(transform.position, rayDir * detectionDistance, Color.red);
                Vector3 dodgeDirection = Vector3.Cross(Vector3.up, hit.normal).normalized;
                direction = dodgeDirection;
                
                return true;
            }
            else
            {
                speed = Mathf.Lerp(speed, DefSpeed, Time.deltaTime);
            }
        }
        return false; 
    }
}
