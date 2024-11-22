using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings")]
    private Rigidbody2D rb;
    private float xAxis;
    [Header("Ground Check Settings")]
    [SerializeField] private float walkSpeed = 1;
    [SerializeField] private float JumpForce = 45;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;

    public static playerController Instance;

    Animator anim;
    // Start is called before the first frame update
    private void awake()
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
    }

    // Update is called once per frame
    void Update()
    {
        GetInputs();
        Move();
        Jump();
        flip();
    }
    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
    }
    void flip()
    {
        if(xAxis < 0)
        {
            transform.localScale = new Vector2(-1,transform.localScale.y);
        }
        else if(xAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
        }
    }

    private void Move()
    {
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
        bool isWalking = Mathf.Abs(rb.velocity.x) > 0.1f && Grounded(); // Threshold set to 0.1f
        anim.SetBool("Walking", isWalking);
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
        // Jump logic
        if (Input.GetButtonDown("Jump") && Grounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, JumpForce); // Use Vector2
        }

        // Cut jump short if "Jump" button is released
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f); // Reduce upward velocity
        }
        anim.SetBool("Jumping",!Grounded());
    }
}
