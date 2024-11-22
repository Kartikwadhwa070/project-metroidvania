using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings")]
    private Rigidbody2D rb;
    private float xAxis;
    private float gravity;
    [Header("Ground Check Settings")]
    [SerializeField] private float walkSpeed = 1;
    [SerializeField] private float JumpForce = 45;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;

    Animator anim;

    [Header("vertical movement settings")]
    public static playerController Instance;
    PlayerStateList pState;

    private int jumpBufferCounter = 0;
    [SerializeField] private int jumpBufferFrames = 0;

    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;

    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;

    private bool hasAirDashed = false;
    private bool canDash = true;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;   
    private bool dashed;
    [SerializeField] GameObject dashEffect;

    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        pState = GetComponent<PlayerStateList>();
        gravity = rb.gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        updateJumpVariables();
        if (pState.dashing) return;
        flip();
        Move();
        Jump();
        StartDash();
    }
    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
    }
    void flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
    }

    private void Move()
    {
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
        bool isWalking = Mathf.Abs(rb.velocity.x) > 0.1f && Grounded();
        anim.SetBool("Walking", isWalking);
    }
    void StartDash()
    {
        if (Input.GetButtonDown("Dash") && canDash)
        {
            StartCoroutine(Dash());
        }
    }
    IEnumerator Dash()
    {
        canDash = false;  
        if (!Grounded() && hasAirDashed)
        {
            canDash = true;
            yield break;
        }
        if (!Grounded())
            hasAirDashed = true;
        pState.dashing = true;
        anim.SetBool("IsDashing", true);
        rb.gravityScale = 0;
        rb.velocity = new Vector2(transform.localScale.x * dashSpeed, 0);
        if (Grounded()) Instantiate(dashEffect, transform);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = gravity;
        pState.dashing = false;
        anim.SetBool("IsDashing", false);
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    public bool Grounded()
    {
        if (Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckY, whatIsGround) || Physics2D.Raycast(groundCheck.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround) || Physics2D.Raycast(groundCheck.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    void Jump()
    {
        if (!pState.jumping)
        {
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, JumpForce);
                pState.jumping = true;
            }
            else if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
            {
                pState.jumping = true;
                airJumpCounter++;
                rb.velocity = new Vector2(rb.velocity.x, JumpForce);
            }
        }
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            pState.jumping = false;
        }
        anim.SetBool("Jumping", !Grounded());
    }
    void updateJumpVariables()
    {
        if (Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
            hasAirDashed = false;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferFrames;
        }
        else
        {
            jumpBufferCounter--;
        }
    }
}