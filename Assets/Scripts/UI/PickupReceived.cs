using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PickupReceived : MonoBehaviour
{
    //we can easily change this later on, but for now I'll just use colors.
    public Color blue;
    public Color green;
    public Color red;

    public Image pickupImage;

    public void SetColor(PickupType type)
    {
        Debug.Log(type);
        pickupImage.enabled = true;
        switch (type)
        {
            case PickupType.Blue: pickupImage.color = blue; break;
            case PickupType.Green: pickupImage.color = green; break;    
            case PickupType.Red: pickupImage.color = red; break;
            default: break;
        }
    }

    public void BlankOut()
    {
        pickupImage.enabled = false;
    }
}
