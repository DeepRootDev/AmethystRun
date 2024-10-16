using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Recorder.OutputPath;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInput input = null;
    private Vector2 moveVector = Vector2.zero;
    private Rigidbody rb = null;
    private Vector3 moveDirection,wallRunDirection;
    [SerializeField]
    bool isGliding,isGlidingFinished;

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
    private DetectGround floorSensor;

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
    float NewX, NewZ;
    [SerializeField] private float rechargeDelay = 2.0f;

    private float countRechargeDelay = 2.0f;

    public  bool dashing = false;
    public bool OnWall;
    public Transform WallTransform;
    private bool recharging = false;
    [SerializeField]
    LayerMask GroundLayer;
    [SerializeField]
    float GlideDelay,GlideCounter;
    public float GetDashTimeLeft() { return dashTimeRemaining; }
    public bool GetRecharging() { return recharging; }
    private bool falling = false;
    private bool apexReached;
    [SerializeField]
    Animator ModelAnimator;

    [SerializeField]
    GameObject ShockwavePrefab;


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

    public float DistanceToGround()
    {
        Ray r = new Ray(transform.position,Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(r,out hit, GroundLayer))
        {
            return hit.distance;    
        }
        return 0f;
    }

    private void FixedUpdate()
    {
        Vector3 dir = new Vector3 (moveVector.x, 0, moveVector.y);
        //Look();
        moveDirection = CalculateForward(dir);
        ModelAnimator.SetFloat("Forward", moveDirection.magnitude);
        ModelAnimator.SetBool("GlideStarter", isGliding);
        ModelAnimator.SetBool("WallRun", OnWall);
        ModelAnimator.SetBool("Dash", dashing);
        if (moveDirection.magnitude != 0 && !isGliding && !OnWall)
        {
            Quaternion rot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, 2f * Time.deltaTime);
        }
        
        if (!dashing)
        {
            RechargeDash();
            dashScalar = 0;
            dashParticles.gameObject.SetActive(false);
        }
        else
        {
            
            if (dashTimeRemaining > 0)
            {
                dashTimeRemaining -= Time.deltaTime;
                dashScalar = additionalDashSpeed;
                dashParticles.gameObject.SetActive(true);
            }
            else
            {
                dashTimeRemaining = 0;
                dashScalar = 0;
                dashParticles.gameObject.SetActive(false);
                dashing = false;
                FindObjectOfType<AudioManager>().Stop("Wind");
            }
            
        }
        
        if (isGliding)
        {
            float Roty = 0;
            if (moveVector.y < 0)
            {
                Roty = moveVector.y;
            }
            Quaternion rot = Quaternion.Euler(Roty* 40f,Camera.main.transform.localEulerAngles.y,-moveVector.x * 40f);
            transform.rotation = Quaternion.Lerp(transform.rotation, rot, 5f * Time.deltaTime);
            //rb.velocity = Vector3.zero;
            if (moveVector.magnitude > 0)
            {
                ModelAnimator.SetBool("Gliding", true);
                if (moveVector.y != 0)
                {
                    rb.AddForce((dashScalar + moveSpeed-2f) * transform.forward, ForceMode.Acceleration);
                }
            }
            else
            {
                ModelAnimator.SetBool("Gliding", false);
            }
            
            rb.AddForce((dashScalar + moveSpeed /2) * transform.right * moveVector.x, ForceMode.Acceleration);
            rb.AddForce((-Physics.gravity.y/2) * Vector3.down, ForceMode.Acceleration);
            
            airDrag = 10f;
        }
        else
        {
            if (!OnWall)
            {
                if (floorSensor.IsGroundDetected())
                {
                    
                    if(moveDirection.magnitude > 0)
                        rb.AddForce((moveSpeed + dashScalar) * (moveDirection.normalized) , ForceMode.Acceleration);
                }
                else
                {
                    rb.AddForce((airSpeed + dashScalar) * moveDirection.normalized, ForceMode.Acceleration);
                }
            }
            else
            {
                Vector3 direction = transform.forward;
                direction.y = 0;
                Quaternion LookRot = Quaternion.LookRotation(wallRunDirection);
                Quaternion LookRight = Quaternion.Euler(0,transform.rotation.eulerAngles.y, ((WallTransform.position.x - transform.position.x) * transform.right.x)*40f);
                //Quaternion newRot = LookRight * LookRot;
                transform.rotation = Quaternion.Lerp(transform.rotation, LookRot, 5f * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, LookRight, 5f * Time.deltaTime);
                
                rb.AddForce((moveSpeed + dashScalar) * transform.forward, ForceMode.Acceleration);
            }
            
            airDrag = 1f;

            if (floorSensor.IsGroundDetected() || OnWall)
            {
                if(!OnWall)
                    ModelAnimator.SetBool("Air",false);
                //If we're on the ground, on the previous frame, were we already grounded?
                //No? don't do anything to the jumpCount.
                if (!onGround || OnWall)
                {
                    jumpCount = 0;
                }
                onGround = true;
                rb.drag = groundDrag;
                falling = false;
            }
            else
            {
                ModelAnimator.SetBool("Air", true);
                rb.drag = airDrag;
                onGround = false;

                if (rb.velocity.y < 0 && !OnWall)
                {
                    rb.AddForce(Vector3.down * -Physics.gravity.y * (gravityScale + fallingMagnitude), ForceMode.Acceleration);
                }
                else
                {
                    if(!OnWall)
                        rb.AddForce(Vector3.down * -Physics.gravity.y * gravityScale, ForceMode.Acceleration);
                }
            }
        }
        
        
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && !isGlidingFinished)
        {
            isGliding = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isGlidingFinished = false;
            isGliding = false;
            ModelAnimator.SetBool("Gliding", false);
        }
        if (DistanceToGround() < 0.5f && isGliding){
            //rb.AddForce(Vector3.up * (1), ForceMode.VelocityChange);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && isGliding)
        {
            rb.AddForce(transform.right * moveVector.x * 20f, ForceMode.Impulse);
           
        }
        if (isGliding)
        {
            
            GlideCounter += Time.deltaTime;
            if (GlideCounter >= GlideDelay)
            {
                isGlidingFinished = true;
                isGliding = false;
                GlideCounter = 0;
            }
        }
        else
        {
            GlideCounter = 0;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if (dashTimeRemaining > 0)
            {
                dashing = true;
                Instantiate(ShockwavePrefab,transform.position,Quaternion.identity);
                FindObjectOfType<AudioManager>().Play("Wind");
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            dashing = false;
            FindObjectOfType<AudioManager>().Stop("Wind");
        }
        if (Input.GetKeyDown(KeyCode.C) && onGround)
        {
            ModelAnimator.SetTrigger("Barge");
            Collider[] c = Physics.OverlapSphere(transform.position, 1f);
            foreach (Collider col in c)
            {
                if (col.CompareTag("Player")&& col.gameObject != this.gameObject)
                {
                    col.GetComponent<PlayerMovement>().ModelAnimator.SetTrigger("Tripped");
                }
            }
        }
    }

    private void OnMovementPerformed(InputAction.CallbackContext value)
    {
        moveVector = value.ReadValue<Vector2>();
        //moveVector = Vector2.Lerp(moveVector, value.ReadValue<Vector2>(), 1);
        //moveVector.x /= 2;        
    }
    
    private void OnMovementCancelled(InputAction.CallbackContext value)
    {
        moveVector = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext value)
    {
        if (jumpCount < numberOfJumpsAllowed && !isGliding)
        {
            jumpCount++;
            float additionalJumpForce = 0.0f;
            if (floorSensor.GetComponent<DetectGround>().IsCoyoteEffect())
            {
                additionalJumpForce = coyoteJumpForce;
            }
            if (!OnWall)
            {
                rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
                rb.AddForce(Vector3.up * (jumpForce + additionalJumpForce), ForceMode.VelocityChange);
            }
            else
            {
                rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
                rb.AddForce(transform.up * (jumpForce + additionalJumpForce), ForceMode.VelocityChange);
            }
            ModelAnimator.SetTrigger("Jump");
        }

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts[0].normal != Vector3.up && collision.contacts[0].normal != Vector3.down && collision.collider.CompareTag("Wall"))
        {
            wallRunDirection = collision.contacts[0].normal + transform.forward;
            wallRunDirection.Normalize();
            if (wallRunDirection.x < 0.01f || wallRunDirection.x > 0.05f)
            {
                wallRunDirection.x = 0.0f;
                ModelAnimator.SetTrigger("Wall");
                WallTransform = collision.collider.transform;
                OnWall = true;
                //rb.isKinematic = true;
                rb.useGravity = false;
            }
            
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (OnWall)
        {
            //WallTransform = collision.collider.transform;
            OnWall = false;
            //rb.isKinematic = true;
            rb.useGravity = true;
            //rb.velocity = Vector3.zero;
        }
    }
    private void OnJumpCancelled(InputAction.CallbackContext value)
    {
        falling = true;
    }


    private void OnDashPerformed(InputAction.CallbackContext value)
    {
        

    }
    private void OnDashCancelled(InputAction.CallbackContext value)
    {
        
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
