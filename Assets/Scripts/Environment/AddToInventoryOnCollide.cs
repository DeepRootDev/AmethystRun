using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddToInventoryOnCollide : MonoBehaviour
{
    public Color activated = Color.green;
    public Color unactivated = Color.red;
    public PickupType typeToSpawn = PickupType.Any;
    public string playerTag = "Player";
    public string botTag = "Bot";

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.gameObject.CompareTag(playerTag))
        {
            other.gameObject.GetComponent<PlayerPickups>().pickedUpItem(typeToSpawn);
            TurnOnMaterial();
        }
        else if (other.gameObject.CompareTag(botTag))
        {
            other.gameObject.GetComponent<BotPickups>().PickedUpItem(typeToSpawn);
            TurnOffMaterial();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(playerTag) || other.gameObject.CompareTag(botTag));
        {
            TurnOffMaterial();
        }
    }

    private void TurnOnMaterial()
    {
        GetComponent<Renderer>().material.color = activated;
    }

    private void TurnOffMaterial()
    {
        GetComponent<Renderer>().material.color = unactivated;
    }
}
