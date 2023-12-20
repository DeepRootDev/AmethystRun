using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectGround : MonoBehaviour
{
    public Color groundDetectedColor = Color.green;
    public Color groundNotDetectedColor = Color.red;

    [SerializeField]
    private bool isGrounded = false;

    [SerializeField]
    private bool addCoyoteEffect = false;

    [SerializeField]
    private bool debugMode = false;

    
    private void Update()
    {
        //For debugging only.
        if(debugMode)
        {
            if(!GetComponent<Renderer>().enabled)
            {
                GetComponent<Renderer>().enabled = true;
            }

            if (isGrounded)
                GetComponent<Renderer>().material.color = groundDetectedColor;
            else
                GetComponent<Renderer>().material.color = groundNotDetectedColor;
        }
        else
            GetComponent<Renderer>().enabled = false;
        
    }

    public bool IsGroundDetected()
    {
        return isGrounded;
    }

    public bool IsCoyoteEffect()
    {
        return addCoyoteEffect;
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        if(other.gameObject.CompareTag("Coyote"))
        {
            addCoyoteEffect = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
        if (other.gameObject.CompareTag("Coyote"))
        {
            addCoyoteEffect = false;
        }
    }
}
