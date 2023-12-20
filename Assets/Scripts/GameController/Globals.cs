using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Globals : MonoBehaviour
{
    [SerializeField] private GameObject[] blueObjects;
    [SerializeField] private GameObject[] greenObjects;
    [SerializeField] private GameObject[] redObjects;

    private void Awake()
    {
        //From the moment the game begins, find obstacles.
        blueObjects = GameObject.FindGameObjectsWithTag("Blue");
        greenObjects = GameObject.FindGameObjectsWithTag("Green");
        redObjects = GameObject.FindGameObjectsWithTag("Red");
    }

    public void ActivateWithType(PickupType type)
    {
        switch (type) 
        {
            case PickupType.Blue: ActivateBlue(); break;
            case PickupType.Green: ActivateGreen(); break;
            case PickupType.Red: ActivateRed(); break;  
            default: break;
        }
    }

    public void ActivateBlue()
    {
        ActivateObstacles(blueObjects);
    }

    public void ActivateGreen() 
    {
        ActivateObstacles(greenObjects);
    }
    public void ActivateRed() 
    {
        ActivateObstacles(redObjects);
    }

    private void ActivateObstacles(GameObject[] objectsToActivate)
    {
        foreach (GameObject objectToActivate in objectsToActivate)
        {
            objectToActivate.GetComponentInChildren<ActiveObject>().Activate();
        }
    }
}
