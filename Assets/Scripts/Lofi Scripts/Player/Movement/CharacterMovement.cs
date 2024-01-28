using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Header("Gravity")]
    public float gravity = 9.8f;
    public bool isGrounded;

    [Header("Movement")]
    public CharacterController characterController;
    public float walkSpeed;
    public float dashSpeed;
    public float movementSmoothing;
    public float rotationSmoothing;
    public bool isDashing;
    public Vector2 inputMovement;
    private Vector3 movement;
    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVelocity;
    private Vector3 movementSpeed;
    private DefaultInput defaultInput;

    private void Start()
    {
        Initalize();
    }

    void Update()
    {
        CalculateMovement();
    }

    private void Initalize()
    {
        //Initialize Input
        defaultInput = new DefaultInput();
        defaultInput.Enable();

        //Initalize WASD
        defaultInput.Character.Movement.performed += e => inputMovement = e.ReadValue<Vector2>();
        defaultInput.Character.Movement.canceled += e => inputMovement = e.ReadValue<Vector2>();
    }

    //Movement
    private void CalculateMovement()
    {
        if (isGrounded)
        {
            //Calculate Movement
            movement = new Vector3((isDashing ? dashSpeed : walkSpeed) * inputMovement.x, 0, (isDashing ? dashSpeed : walkSpeed) * inputMovement.y);
            newMovementSpeed = Vector3.SmoothDamp(newMovementSpeed, movement, ref newMovementSpeedVelocity, movementSmoothing);
            movementSpeed = transform.TransformDirection(newMovementSpeed * Time.deltaTime);
        }

        characterController.Move(movementSpeed);

        /* - faces character, currently broken
         
        Vector3 faceDirection = new Vector3(inputMovement.x, 0, inputMovement.y);
        Quaternion targetRotation = Quaternion.LookRotation(faceDirection, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothing);
        */

        //Set Animation Speed - Needed for once animations are integrated into character. 
        //animationSpeed = characterController.velocity.magnitude / walkSpeed;
    }
}
