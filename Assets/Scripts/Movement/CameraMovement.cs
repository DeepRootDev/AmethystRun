using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.Windows;

public class CameraMovement : MonoBehaviour
{
    public Transform playerBody;
    private PlayerInput input = null;
    private Vector2 rotationVector = Vector2.zero;

    private float xRotation = 0f;

    [SerializeField]
    private float sens = 50f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Awake()
    {
        input = new PlayerInput();

    }

    private void OnEnable()
    {
        input.Enable();

        //subscribe to events
    }

    private void OnDisable()
    {
        input.Disable();

        //unsubscribe from events

    }

    private void OnCameraMove(InputAction.CallbackContext value)
    {
        rotationVector = value.ReadValue<Vector2>();

        float mouseX = rotationVector.x * Time.deltaTime * sens;
        float mouseY = rotationVector.y * Time.deltaTime * sens;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);


        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);


    }
}
