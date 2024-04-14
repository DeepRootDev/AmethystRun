using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInput input = null;
    private Vector2 moveVector = Vector2.zero;
    private Rigidbody rb = null;
    private Vector3 moveDirection;


    [SerializeField]
    private float moveSpeed = 5.0f;
    [SerializeField]
    private float airSpeed = 0.5f;

    [SerializeField]
    private float jumpForce = 5.0f;

    [SerializeField]
    private float coyoteJumpForce = 10f;

    [SerializeField]
    private float gravityScale = 1.0f;

    [SerializeField]
    private float fallingMagnitude = 10f;

    [SerializeField]
    private GameObject floorSensor;

    [SerializeField]
    private float groundDrag = 5.0f;

    [SerializeField]
    private float airDrag = 1.0f;

    [SerializeField]
    private bool onGround = true;

    [SerializeField]
    private int numberOfJumpsAllowed = 1;

    [SerializeField]
    private int jumpCount = 0;

    //dashing
    [SerializeField] private ParticleSystem dashParticles;
    [SerializeField] private float maxDashTime = 3f;
    [SerializeField] private float dashTimeRemaining = 3f;

    [SerializeField] private float dashRechargeTime = 2.0f;

    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float additionalDashSpeed = 5f;
    private float dashScalar = 0.0f;

    [SerializeField] private float rechargeDelay = 2.0f;

    private float countRechargeDelay = 2.0f;

    private bool dashing = false;

    private bool recharging = false;


    public float GetDashTimeLeft() { return dashTimeRemaining; }
    public bool GetRecharging() { return recharging; }
    private bool falling = false;
    private bool apexReached;
    private void Awake()
    {
        input = new PlayerInput();
        rb = GetComponent<Rigidbody>();
        countRechargeDelay = rechargeDelay;
        dashTimeRemaining = maxDashTime;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    Vector3 CalculateForward(Vector3 toBeCalculated)
    {
        float currentY = toBeCalculated.y;
        Vector3 CameraForward = Camera.main.transform.forward;
        Vector3 CameraRight = Camera.main.transform.right;
        CameraForward.y = 0;
        CameraRight.y = 0;

        CameraForward.Normalize();
        CameraRight.Normalize();

        Vector3 ForwardProduct = toBeCalculated.z * CameraForward;
        Vector3 RightProduct = toBeCalculated.x * CameraRight;

        Vector3 RotatedVector = ForwardProduct + RightProduct;
        RotatedVector.y = currentY;
        return RotatedVector;
    }
    private void OnEnable()
    {
        input.Enable();

        //subscribe to events
        input.Player.Movement.performed += OnMovementPerformed;
        input.Player.Movement.canceled += OnMovementCancelled;
        input.Player.Jump.performed += OnJumpPerformed;
        input.Player.Jump.canceled += OnJumpCancelled;
        input.Player.Dash.performed += OnDashPerformed;
        input.Player.Dash.canceled += OnDashCancelled;

    }

    private void OnDisable()
    {
        input.Disable();

        //unsubscribe from events
        input.Player.Movement.performed -= OnMovementPerformed;
        input.Player.Movement.canceled -= OnMovementCancelled;
        input.Player.Jump.performed -= OnJumpPerformed;
        input.Player.Jump.canceled -= OnJumpCancelled;
        input.Player.Dash.performed -= OnDashPerformed;
        input.Player.Dash.canceled -= OnDashCancelled;
    }

    private void FixedUpdate()
    {
        Vector3 dir = new Vector3 (moveVector.x, 0, moveVector.y);
        //Look();
        moveDirection = CalculateForward(dir);
        if (moveDirection.magnitude != 0)
        {
            Quaternion rot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, 5f * Time.deltaTime);
        }
        if (!dashing)
        {
            RechargeDash();
            dashScalar = 0;
        }
        else
        {
            dashTimeRemaining -= Time.fixedDeltaTime;
            dashScalar = additionalDashSpeed;
        }


        if (floorSensor.GetComponent<DetectGround>().IsGroundDetected())
        {
            rb.AddForce((moveSpeed + dashScalar) * moveDirection.normalized, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce((airSpeed + dashScalar) * moveDirection.normalized, ForceMode.Acceleration);
        }
        if (floorSensor.GetComponent<DetectGround>().IsGroundDetected())
        {
            //If we're on the ground, on the previous frame, were we already grounded?
            //No? don't do anything to the jumpCount.
            if (!onGround)
            {
                jumpCount = 0;
            }
            onGround = true;
            rb.drag = groundDrag;
            falling = false;
        }
        else
        {
            rb.drag = airDrag;
            onGround = false;

            if (rb.velocity.y < 0)
            {
                rb.AddForce(Vector3.down * -Physics.gravity.y * (gravityScale + fallingMagnitude), ForceMode.Acceleration);
            }
            else
            {
                rb.AddForce(Vector3.down * -Physics.gravity.y * gravityScale, ForceMode.Acceleration);
            }
        }
    }

    private void Update()
    {
        
    }

    private void OnMovementPerformed(InputAction.CallbackContext value)
    {
        moveVector = Vector2.Lerp(moveVector, value.ReadValue<Vector2>(), 1);
        //moveVector.x /= 2;        
    }
    
    private void OnMovementCancelled(InputAction.CallbackContext value)
    {
        moveVector = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext value)
    {
        if (jumpCount < numberOfJumpsAllowed)
        {
            jumpCount++;
            float additionalJumpForce = 0.0f;
            if (floorSensor.GetComponent<DetectGround>().IsCoyoteEffect())
            {
                additionalJumpForce = coyoteJumpForce;
            }

            rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
            rb.AddForce(Vector3.up * (jumpForce + additionalJumpForce), ForceMode.Impulse);
        }

    }

    private void OnJumpCancelled(InputAction.CallbackContext value)
    {
        falling = true;
    }


    private void OnDashPerformed(InputAction.CallbackContext value)
    {
        if (dashTimeRemaining > 0)
        {
            dashing = true;
        }

    }
    private void OnDashCancelled(InputAction.CallbackContext value)
    {
        dashing = false;

    }

    private void RechargeDash()
    {

        //time to recharge
        recharging = true;

        //Start counting down the countRechargeDelay
        if (countRechargeDelay > 0)
        {
            countRechargeDelay -= Time.deltaTime;
        }
        //once that's done, start counting down recharge time
        else if (dashTimeRemaining < maxDashTime)
        {
            dashTimeRemaining += Time.deltaTime;
        }
        else
        {
            countRechargeDelay = rechargeDelay;
        }
    }

    private Vector2 rotationVector = Vector2.zero;

    private float yRotation = 0f;
    [SerializeField] private CinemachineVirtualCamera vc;

    void Look()
    {
        vc.m_Lens.FieldOfView = Mathf.Lerp(vc.m_Lens.FieldOfView, 55 + moveVector.y * 5 + moveVector.x * 4, 0.05f);
        CinemachineBasicMultiChannelPerlin pn = vc.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        pn.m_AmplitudeGain = Mathf.Lerp(pn.m_AmplitudeGain, moveVector.y /2 + 1, 0.2f);

        yRotation += moveVector.x * 4;
        Vector3 faceDirection = new Vector3(0, yRotation, 0);
        Quaternion targetRotation = Quaternion.Euler(faceDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.3f);


    }

}
