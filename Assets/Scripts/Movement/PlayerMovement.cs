using System.Collections;
using Cinemachine;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class PlayerMovement : MonoBehaviour
{
    private PlayerInput input;
    private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float airSpeed = 0.5f;
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float coyoteJumpForce = 10f;
    [SerializeField] private float gravityScale = 1.0f;
    [SerializeField] private float groundDrag = 5.0f;
    [SerializeField] private float airDrag = 1.0f;

    [Header("Ground Check")]
    [SerializeField] private DetectGround floorSensor;
    [SerializeField] private LayerMask GroundLayer;

    [Header("Jump")]
    [SerializeField] private int numberOfJumpsAllowed = 1;
    private int jumpCount = 0;
    private bool onGround = true;

    [Header("Dash")]
    [SerializeField] private ParticleSystem dashParticles;
    [SerializeField] private float maxDashTime = 3f;
    [SerializeField] private float dashRechargeTime = 2.0f;
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float additionalDashSpeed = 5f;
    [SerializeField] private float rechargeDelay = 2.0f;
    [SerializeField] private GameObject ShockwavePrefab;

    private float dashTimeRemaining;
    private float dashScalar = 0.0f;
    private float countRechargeDelay;
    private bool recharging = false;
    public bool dashing = false;

    [Header("Wall Run")]
    public bool OnWall;
    public Transform WallTransform;
    private Vector3 wallRunDirection;

    [Header("Gliding")]
    public bool isGliding, isGlidingFinished,GlideTrigger;
    [SerializeField] private float GlideDelay, GlideCounter;

    [Header("Slide")]
    [SerializeField] private float slideSpeed = 10f;
    [SerializeField] private float slideDuration = 0.8f;
    [SerializeField] private float slideCooldown = 1.0f;
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float normalHeight = 2.0f;
    private bool isSliding = false;
    private bool canSlide = true;
    private Coroutine slideCoroutine;

    [Header("Animation")]
    [SerializeField] private Animator ModelAnimator;

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera vc;

    [Header("Ui Scale")]
    [SerializeField]Transform UIPlayer;

    private Vector2 moveVector;
    private Vector3 moveDirection;
    private float vertical, horizontal;
    private float yRotation;

    private void Awake()
    {
        input = new PlayerInput();
        rb = GetComponent<Rigidbody>();
        dashTimeRemaining = maxDashTime;
        countRechargeDelay = rechargeDelay;
        UIPlayer.localScale = transform.localScale+new Vector3(2,2,2);
    }

    private void Update()
    {
        HandleInput();
        HandleGlide();
        HandleDash();
        HandleJump();
        HandleSlide();
        RechargeDash();
    }

    private void FixedUpdate()
    {
        UpdateAnimator();
        HandleRotation();
        HandleMovement();
    }

    private void HandleInput()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        moveVector = new Vector2(horizontal, vertical);
    }

    private void HandleRotation()
    {
        if (moveVector.sqrMagnitude > 0 && !isGliding && !OnWall)
        {
            Quaternion targetRotation = Quaternion.LookRotation(CalculateForward(new Vector3(moveVector.x, 0, moveVector.y)));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
        }
    }

    private void HandleMovement()
    {
        moveDirection = CalculateForward(new Vector3(moveVector.x, 0, moveVector.y));
        if(GlideTrigger && moveDirection.magnitude != 0){
            isGliding= true;
        }else{
            isGliding = false;
        }
        if (isGliding)
        {
            rb.useGravity = false;
            rb.AddForce((moveSpeed + dashScalar - 2f) * transform.forward, ForceMode.Acceleration);
            rb.AddForce(moveSpeed / 2 * transform.right * moveVector.x, ForceMode.Acceleration);
             float pitch = -moveVector.y * 30f; 
             float roll = -moveVector.x * 30f; 
            Quaternion targetRotation = Quaternion.Euler(pitch, transform.rotation.eulerAngles.y, roll);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.fixedDeltaTime);
            rb.AddForce(Vector3.down * (-Physics.gravity.y * 0.5f), ForceMode.Acceleration);
        }
        else if (OnWall)
        {
            Quaternion targetRotation = Quaternion.LookRotation(wallRunDirection);
            Quaternion wallTilt = Quaternion.Euler(0, 0, (WallTransform.position.x - transform.position.x) * transform.right.x * 40f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation * wallTilt, 5f * Time.fixedDeltaTime);
            rb.AddForce((moveSpeed + dashScalar) * wallRunDirection, ForceMode.Acceleration);
        }
        else
        {
            rb.useGravity = true;
            Vector3 force = floorSensor.IsGroundDetected()
                ? (moveSpeed + dashScalar) * moveDirection.normalized
                : (airSpeed + dashScalar) * moveDirection.normalized;

            rb.AddForce(force, ForceMode.Acceleration);
            if (floorSensor.IsGroundDetected()) jumpCount = 0;
        }
    }

    private void HandleGlide()
    {
        if (Input.GetMouseButton(0) && isGlidingFinished)
        {
            isGliding = true;
            isGlidingFinished = false;
            GlideTrigger = true;
        }

        if (Input.GetMouseButtonUp(0) || (isGliding && DistanceToGround() < 0.5f))
        {
            isGliding = false;
            isGlidingFinished = true;
            GlideTrigger = false;
        }

        if (GlideTrigger)
        {
            GlideCounter += Time.deltaTime;
            if (GlideCounter >= GlideDelay)
            {
                isGliding = false;
                isGlidingFinished = true;
                GlideTrigger = false;
                GlideCounter = 0;
            }
        }
        else
        {
            GlideCounter = 0;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && isGliding)
        {
            Vector3 glideBoost = transform.right * Mathf.Sign(moveVector.x) * 20f;
            rb.AddForce(glideBoost, ForceMode.Impulse);
        }
    }

    private void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashTimeRemaining > 0)
        {
            dashing = true;
            Instantiate(ShockwavePrefab, transform.position, Quaternion.identity);
            FindObjectOfType<AudioManager>().Play("Wind");
            dashTimeRemaining -= 1;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            dashing = false;
            FindObjectOfType<AudioManager>().Stop("Wind");
        }
    }

    private void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < numberOfJumpsAllowed && !isGliding)
        {
            jumpCount++;
            float jumpPower = jumpForce + (floorSensor.IsCoyoteEffect() ? coyoteJumpForce : 0f);
            rb.velocity = new Vector3(rb.velocity.x, 0.0f, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
            ModelAnimator.SetTrigger("Jump");
        }
    }

    private void HandleSlide()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && floorSensor.IsGroundDetected() && canSlide)
        {
            StartSlide();
        }
    }

    private void StartSlide()
    {
        if (isSliding) return;

        isSliding = true;
        canSlide = false;
        ModelAnimator.SetBool("Sliding", true);
        transform.localScale = new Vector3(transform.localScale.x, crouchHeight, transform.localScale.z);
        rb.AddForce(moveDirection * slideSpeed, ForceMode.VelocityChange);

        slideCoroutine = StartCoroutine(EndSlide());
    }

    private IEnumerator EndSlide()
    {
        yield return new WaitForSeconds(slideDuration);
        isSliding = false;
        ModelAnimator.SetBool("Sliding", false);
        transform.localScale = new Vector3(transform.localScale.x, normalHeight, transform.localScale.z);
        yield return new WaitForSeconds(slideCooldown);
        canSlide = true;
    }

    private void RechargeDash()
    {
        recharging = true;
        if (countRechargeDelay > 0)
        {
            countRechargeDelay -= Time.deltaTime;
        }
        else if (dashTimeRemaining < maxDashTime)
        {
            dashTimeRemaining += Time.deltaTime;
        }
        else
        {
            countRechargeDelay = rechargeDelay;
        }
    }

    private void UpdateAnimator()
    {
        ModelAnimator.SetFloat("Forward", rb.velocity.magnitude);
        ModelAnimator.SetBool("GlideStarter", GlideTrigger);
        ModelAnimator.SetBool("Gliding", isGliding);
        ModelAnimator.SetBool("WallRun", OnWall);
        ModelAnimator.SetBool("Dash", dashing);
        ModelAnimator.SetBool("Air", !floorSensor.IsGroundDetected());
    }

    private Vector3 CalculateForward(Vector3 direction)
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;
        camForward.y = 0;
        camRight.y = 0;
        return (direction.z * camForward.normalized + direction.x * camRight.normalized);
    }

    private float DistanceToGround()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, GroundLayer))
            return hit.distance;
        return 0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            Vector3 wallNormal = collision.contacts[0].normal;
            if (wallNormal != Vector3.up && wallNormal != Vector3.down)
            {
                float wallAngle = Vector3.Angle(Vector3.up, wallNormal);
                if (wallAngle > 85f && wallAngle < 95f)
                {
                    wallRunDirection = Vector3.ProjectOnPlane(transform.forward, wallNormal).normalized;
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
            OnWall = false;
            rb.useGravity = true;
        }
    }
}