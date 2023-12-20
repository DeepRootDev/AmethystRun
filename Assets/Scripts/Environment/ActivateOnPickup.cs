using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateOnPickup : MonoBehaviour
{
    public Color activated = Color.green;
    public Color unactivated = Color.red;
    public GameObject[] toActivate;

    [SerializeField] private GameObject gameController;

    private void Awake()
    {
        gameController = GameObject.Find("GameController");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GetComponent<Renderer>().material.color = activated;

            //We want this to first be randomized to activate any color (need to make animations/movement scripts for the other components tho).
            gameController.GetComponent<Globals>().ActivateBlue();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GetComponent<Renderer>().material.color = unactivated;
        }
    }
}
