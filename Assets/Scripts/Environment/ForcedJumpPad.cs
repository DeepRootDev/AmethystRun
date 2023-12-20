using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForcedJumpPad : MonoBehaviour
{
    public float pushForce = 100f;
    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.AddForce(transform.TransformDirection(Vector3.forward) * pushForce, ForceMode.Impulse);
        }
    }

}
