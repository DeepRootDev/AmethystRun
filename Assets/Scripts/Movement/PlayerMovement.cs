using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Recorder.OutputPath;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInput input = null;
    private Vector2 moveVector = Vector2.zero;
    private Rigidbody rb = null;
    private Vector3 moveDirection, wallRunDirection;
    [SerializeField]
    public bool isGliding, isGlidingFinished;

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

    public bool dashing = false;
    public bool OnWall;
    public Transform WallTransform;
    private bool recharging = false;
    [SerializeField]
    LayerMask GroundLayer;
    [SerializeField]
    float GlideDelay, GlideCounter;
    public float GetDashTimeLeft() { return dashTimeRemaining; }
    public bool GetRecharging() { return recharging; }
    public bool falling = false;
    private bool apexReached;
    [SerializeField]
    public Animator ModelAnimator;

    [SerializeField]
    GameObject ShockwavePrefab;
    
    float Vertical,Horizontal;

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


    public float DistanceToGround()
    {
        Ray r = new Ray(transform.position, Vector3.down);
        RaycastHit hit;
        if (Physics.Raycast(r, out hit, GroundLayer))
        {
            return hit.distance;
        }
        return 0f;
    }

    private void FixedUpdate()
    {
        Vector3 dir = new Vector3(moveVector.x, 0, moveVector.y);
        moveDirection = CalculateForward(dir);

        ModelAnimator.SetFloat("Forward", rb.velocity.magnitude);
        ModelAnimator.SetBool("GlideStarter", isGliding);
        ModelAnimator.SetBool("WallRun", OnWall);
        ModelAnimator.SetBool("Dash", dashing);
        ModelAnimator.SetBool("Air",!floorSensor.IsGroundDetected());

        // Smooth Movement Rotation
        if (moveDirection.magnitude > 0 && !isGliding && !OnWall)
        {
            Quaternion rot = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.fixedDeltaTime);
        }

        // Handle Dashing
        if (dashing)
        {
            dashTimeRemaining -= Time.deltaTime;
            dashScalar = Mathf.Lerp(dashScalar, 0, 0.1f);
            dashParticles.gameObject.SetActive(true);
        }
        else
        {
            dashScalar = 0;
            dashParticles.gameObject.SetActive(false);
        }

        // Gliding Logic
        if (isGliding)
        {
            Quaternion glideRotation = Quaternion.Euler(moveVector.y * 40f, Camera.main.transform.eulerAngles.y, -moveVector.x * 40f);
            transform.rotation = Quaternion.Slerp(transform.rotation, glideRotation, 10f * Time.fixedDeltaTime);
            if (moveVector.magnitude > 0)
            {
                ModelAnimator.SetBool("Gliding", true);
                rb.AddForce((moveSpeed + dashScalar - 2f) * transform.forward * Mathf.Abs(moveVector.y), ForceMode.Acceleration);
            }
            else
            {
                ModelAnimator.SetBool("Gliding", false);
            }
            //rb.AddForce((airSpeed + dashScalar) * transform.forward, ForceMode.Acceleration);
            rb.AddForce(moveSpeed / 2 * transform.right * moveVector.x, ForceMode.Acceleration);
            rb.AddForce(Vector3.down * (-Physics.gravity.y * 0.5f), ForceMode.Acceleration);

            airDrag = 10f;
        }
        else
        {
            
            if (!OnWall)
            {
                if (floorSensor.IsGroundDetected())
                {
                    if (moveDirection.magnitude > 0){
                        if(dashing){
                            rb.AddForce((moveSpeed + dashScalar + additionalDashSpeed) * moveDirection.normalized, ForceMode.Acceleration);
                        }else{
                            rb.AddForce((moveSpeed + dashScalar) * moveDirection.normalized, ForceMode.Acceleration);
                        }
                    }
                    jumpCount = 0;

                }
                else
                {
                    rb.AddForce((airSpeed + dashScalar) * moveDirection.normalized, ForceMode.Acceleration);
                }
            }
            else
            {
                Quaternion targetRotation = Quaternion.LookRotation(wallRunDirection);
Quaternion wallTilt = Quaternion.Euler(0, 0, (WallTransform.position.x - transform.position.x) * transform.right.x * 40f);
transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation * wallTilt, 5f * Time.fixedDeltaTime);

Vector3 wallRunForce = (moveSpeed + dashScalar) * wallRunDirection;
rb.AddForce(wallRunForce, ForceMode.Acceleration);

            }

            airDrag = 2f;
        }
    }


    private void Update()
    {
        RechargeDash();
        if (Input.GetMouseButton(0) && isGlidingFinished)
        {
            print("Glider Equipped");
            isGliding = true;
            isGlidingFinished = false;
            ModelAnimator.SetBool("Gliding", true);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isGliding = false;
            isGlidingFinished = true;
            ModelAnimator.SetBool("Gliding", false);
        }

        // Cancel gliding if too close to ground
        if (isGliding && DistanceToGround() < 0.5f)
        {
            isGliding = false;
            isGlidingFinished = true;
            ModelAnimator.SetBool("Gliding", false);
        }

        // Handle Glide Timer
        if (isGliding)
        {
            rb.useGravity  = false;
            GlideCounter += Time.deltaTime;
            if (GlideCounter >= GlideDelay)
            {
                isGlidingFinished = true;
                isGliding = false;
                GlideCounter = 0;
                ModelAnimator.SetBool("Gliding", false);
            }
        }
        else
        {
            rb.useGravity = true;
            GlideCounter = 0;
        }
        Vertical = Input.GetAxis("Vertical");
        Horizontal =Input.GetAxis("Horizontal");
        moveVector.x = Horizontal;
        moveVector.y = Vertical;
        // Apply Glide Boost (Left Control)
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGliding)
        {
            Vector3 glideBoost = transform.right * Mathf.Sign(moveVector.x) * 20f;
            rb.AddForce(glideBoost, ForceMode.Impulse);
        }

        // Handle Dash (Left Shift)
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashTimeRemaining > 0)
        {
            dashing = true;
            Instantiate(ShockwavePrefab, transform.position, Quaternion.identity);
            FindObjectOfType<AudioManager>().Play("Wind");
            dashTimeRemaining -= 1;  // Reduce dash time
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            dashing = false;
            FindObjectOfType<AudioManager>().Stop("Wind");
        }

        // Barge Attack (C Key)
        if (Input.GetKeyDown(KeyCode.C) && floorSensor.IsGroundDetected())
        {
            ModelAnimator.SetTrigger("Barge");

            Collider[] c = Physics.OverlapSphere(transform.position, 1f);
            foreach (Collider col in c)
            {
                if (col.CompareTag("Player") && col.gameObject != this.gameObject)
                {
                    // Check if the player is in front
                    Vector3 directionToPlayer = (col.transform.position - transform.position).normalized;
                    float dotProduct = Vector3.Dot(transform.forward, directionToPlayer);

                    if (dotProduct > 0) // Player is in front
                    {
                        col.GetComponent<PlayerMovement>().ModelAnimator.SetTrigger("Tripped");
                    }
                }
            }
        }
        if(Input.GetKeyDown(KeyCode.Space)){
            if (jumpCount < numberOfJumpsAllowed && !isGliding)
        {
            jumpCount++;
            float additionalJumpForce = 0.0f;
            if (floorSensor.IsCoyoteEffect())
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

    }

    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Vector3 wallNormal = collision.contacts[0].normal;

            // Ensure it's not the ground or ceiling
            if (wallNormal != Vector3.up && wallNormal != Vector3.down)
            {
                float wallAngle = Vector3.Angle(Vector3.up, wallNormal);
                if (wallAngle > 85f && wallAngle < 95f) // Ensuring it's a near-vertical wall
                {
                    // Get proper wall run direction
                    wallRunDirection = Vector3.ProjectOnPlane(transform.forward, wallNormal);
                    wallRunDirection.Normalize();

                    // Activate wall running
                    ModelAnimator.SetTrigger("Wall");
                    WallTransform = collision.collider.transform;
                    OnWall = true;
                    rb.useGravity = false;
                }
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

        pn.m_AmplitudeGain = Mathf.Lerp(pn.m_AmplitudeGain, moveVector.y / 2 + 1, 0.2f);

        yRotation += moveVector.x * 4;
        Vector3 faceDirection = new Vector3(0, yRotation, 0);
        Quaternion targetRotation = Quaternion.Euler(faceDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 0.3f);


    }

}
