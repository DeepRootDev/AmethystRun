using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public Transform[] possibleCheckpoints;
    

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Bot") && possibleCheckpoints.Length > 0)
        {
            other.GetComponent<AIMovement>().NextTarget(possibleCheckpoints[Random.Range(0, possibleCheckpoints.Length)]);
        }
    }

}
