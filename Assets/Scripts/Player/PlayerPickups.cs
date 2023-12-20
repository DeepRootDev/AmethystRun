using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UnityEngine.UI;

public enum PickupType
{
    Blue,
    Red,
    Green,
    Any
}

public class PlayerPickups : MonoBehaviour
{
    public PickupType type = PickupType.Any;
    private PlayerInput input = null;
    public GameObject[] toActivate;
    [SerializeField] private GameObject gameController;
    private bool permanentType = false;
    private List<PickupType> types = new List<PickupType>() { PickupType.Blue, PickupType.Red, PickupType.Green };

    private void Awake()
    {
        input = new PlayerInput();
        gameController = GameObject.Find("GameController");
        if(type != PickupType.Any)
        {
            permanentType = true;
        }
    }

    private bool hasPickup = false;

    private void OnEnable()
    {
        input.Enable();

        //subscribe to events
        input.Player.UseItem.performed += UseItem;
    }

    public void pickedUpItem (PickupType sentType)
    {
        type = sentType;
        if (type == PickupType.Any && !hasPickup)
        {
            type = PickRandom(types);
        }
        
        gameController.GetComponent<PickupReceived>().SetColor(type);
        hasPickup = true;
        
    }
    

    private void UseItem(InputAction.CallbackContext value)
    {
        if(hasPickup)
        {
            hasPickup = false;
            gameController.GetComponent<Globals>().ActivateWithType(type);
            gameController.GetComponent<PickupReceived>().BlankOut();
        }
        
    }
    
    PickupType PickRandom<PickupType> (IList<PickupType> options)
    {
        int selectedIndex = Random.Range(0, options.Count);
        return options[selectedIndex];
    }
}
