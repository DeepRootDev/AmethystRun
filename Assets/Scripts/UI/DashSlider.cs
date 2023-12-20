using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DashSlider : MonoBehaviour
{
    public Slider dashSlider;
    public PlayerMovement player;



    // Update is called once per frame
    void Update()
    {
        dashSlider.value = player.GetDashTimeLeft();
    }
}
