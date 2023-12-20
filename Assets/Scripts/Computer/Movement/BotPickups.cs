using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class BotPickups : MonoBehaviour
{
    public PickupType currentType = PickupType.Any;
    private bool hasPickup = false;
    private List<PickupType> types = new List<PickupType>() { PickupType.Blue, PickupType.Red, PickupType.Green };
    [SerializeField] private GameObject gameController;
    private float countTime = 0.0f;
    public float useItemTime = 1.0f;

    private void Awake()
    { 
        gameController = GameObject.Find("GameController");
    }

    public void PickedUpItem(PickupType type)
    {
        currentType = type;

        if(currentType == PickupType.Any && !hasPickup)
        {
            currentType = PickRandom(types);
        }

        hasPickup = true;
    }

    private void Update()
    {
        if (hasPickup)
        {
            countTime += Time.deltaTime;
        }

        if (countTime >= useItemTime)
        {
            UseItem();
            countTime = 0.0f;
        }
    }

    private void UseItem()
    {
        hasPickup = false;
        gameController.GetComponent<Globals>().ActivateWithType(currentType);
    }

    PickupType PickRandom<PickupType>(IList<PickupType> options)
    {
        int selectedIndex = Random.Range(0, options.Count);
        return options[selectedIndex];
    }
}
