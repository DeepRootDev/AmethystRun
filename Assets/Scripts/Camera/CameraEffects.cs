using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraEffects : MonoBehaviour
{
    [SerializeField]
    Volume vol;
    Vignette vig ;
    MotionBlur blur;
    PlayerMovement playermovement;
    [SerializeField]
    CinemachineFreeLook Cam;
    float vigValue, BlurValue, FovValue;
    // Start is called before the first frame update
    void Start()
    {
        playermovement = GetComponent<PlayerMovement>();
        vol.profile.TryGet<Vignette>(out vig);
        vol.profile.TryGet<MotionBlur>(out blur);

    }

    // Update is called once per frame
    void Update()
    {
        if (playermovement.dashing)
        {
            vigValue = Mathf.Lerp(vigValue,0.4f,5f*Time.deltaTime);
            BlurValue = Mathf.Lerp(BlurValue,1,5f*Time.deltaTime);
            FovValue = Mathf.Lerp(FovValue,90,5f*Time.deltaTime);
        }
        else
        {
            vigValue = Mathf.Lerp(vigValue, 0, 5f * Time.deltaTime);
            BlurValue = Mathf.Lerp(BlurValue, 0, 5f * Time.deltaTime);
            FovValue = Mathf.Lerp(FovValue, 45, 5f * Time.deltaTime);
        }
        blur.intensity.Override(BlurValue);
        vig.intensity.Override(vigValue);
        Cam.m_Lens.FieldOfView = FovValue;
    }
}
