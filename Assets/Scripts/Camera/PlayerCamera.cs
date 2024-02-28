using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCamera : MonoBehaviour
{
    public CinemachineBrain cinemachineBrain;
    public Transform playerTransform;
    public float rotationSpeed = 5.0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (cinemachineBrain.ActiveVirtualCamera.VirtualCameraGameObject.GetComponent<CinemachineFreeLook>() != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

            // Rotate the player horizontally
            playerTransform.Rotate(Vector3.up * mouseX);

            // Rotate the camera vertically
            CinemachineCore.GetInputAxis = HandleAxisInputDelegate;
            CinemachineCore.GetInputAxis("Mouse Y"); // Update the vertical input axis
        }
    }

    private float HandleAxisInputDelegate(string axisName)
    {
        if (axisName == "Mouse X")
            return Input.GetAxis("Mouse X");
        else if (axisName == "Mouse Y")
            return Input.GetAxis("Mouse Y");
        else
            return 0f;
    }
}