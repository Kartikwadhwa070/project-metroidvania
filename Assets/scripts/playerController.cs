using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class playerController : MonoBehaviour
{
    [Header("Horizontal Movement Settings")]
    [HideInInspector] public PlayerStateList pState;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private float xAxis, yAxis;
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

    [Header("Attacking")]
    bool attack = false;
    float timeBetweenAttack, timeSinceAttack;
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask EnemyLayer;
    [SerializeField] float damage;
    [SerializeField] GameObject slashEffect;

    bool restoreTime;
    float restoreTimeSpeed;

    [Header("Recoil Settings")]
    [SerializeField] int recoilXsteps = 5;
    [SerializeField] int recoilYsteps = 5;
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;
    int stepsXRecoiled, stepsYRecoiled;

    [Header("Health Settings")]
    public int maxHealth;
    [SerializeField] GameObject bloodSpurt;
    [SerializeField] float hitFlashSpeed = 5f;
    public delegate void OnHealthChangedDelegate();
    [HideInInspector] public OnHealthChangedDelegate onHealthChangedCallback;

    float healTimer;
    [SerializeField] float timeToHeal;


    [Header("Mana Settings")]
    [SerializeField] UnityEngine.UI.Image manaStorage;

    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;

    [Header("Spell Settings")]

    [Header("Spell Settings")]

    [SerializeField] float manaSpellCost = 0.3f;
    [SerializeField] float timeBetweenCast = 0.5f;
    float timeSinceCast;
    [SerializeField] float spellDamage;
    [SerializeField] float downSpellForce;
    [SerializeField] GameObject sideSpellFireball;
    [SerializeField] GameObject upSpellExplosion;
    [SerializeField] GameObject downSpellFireball;
    float castOrHealTimer;

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
        DontDestroyOnLoad(gameObject);

    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        pState = GetComponent<PlayerStateList>();
        gravity = rb.gravityScale;
        sr = GetComponent<SpriteRenderer>();
        sr.material = new Material(sr.material);
        Health = maxHealth;
        Mana = mana;
        manaStorage.fillAmount = Mana;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
    }

    // Update is called once per frame
    void Update()
    {
        if (pState.cutscene) return;
        GetInputs();
        UpdateJumpVariables();

        if (pState.dashing) return;
        RestoreTimeScale();
        FlashWhileInvincible();
        Move();
        Heal();
        CastSpell();
        if (pState.healing) return;
        Flip();
        Jump();
        StartDash();
        Attack();
        
    }


    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.GetComponent<Enemy>() != null && pState.casting)
        {
            _other.GetComponent<Enemy>().EnemyHit(spellDamage, (_other.transform.position - transform.position).normalized, -recoilYSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (pState.dashing || pState.healing || pState.cutscene) return;
        Recoil();
    }


    void GetInputs()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");
        attack = Input.GetButtonDown("Attack");
        if (Input.GetButton("Cast/Heal"))
        {
            castOrHealTimer += Time.deltaTime;
        }
        else
        {
            castOrHealTimer = 0;
        }
    }
    void Flip()
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(-1, transform.localScale.y);
            pState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(1, transform.localScale.y);
            pState.lookingRight = true;
        }
    }

    private void Move()
    {
        if (pState.healing) rb.velocity = new Vector2(0, 0);
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
        int _dir = pState.lookingRight ? 1 : -1;
        rb.velocity = new Vector2(_dir * dashSpeed, 0);

        if (Grounded())
            Instantiate(dashEffect, transform);

        float dashTimer = 0;
        while (dashTimer < dashTime && pState.dashing)
        {
            dashTimer += Time.deltaTime;
            yield return null;
        }

        if (pState.dashing)
            StopDash();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (pState.dashing && collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            StopDash();
        }
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if (attack && timeSinceAttack >= timeBetweenAttack)
        {
            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");

            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, recoilXSpeed);
                Instantiate(slashEffect, SideAttackTransform);
            }
            else if (yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, recoilYSpeed);
                slashEffectAtAngle(slashEffect, 90, UpAttackTransform);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, recoilYSpeed);
                slashEffectAtAngle(slashEffect, -90, DownAttackTransform);
            }
        }
    }

    void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilDir, float _recoilStrength)
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, EnemyLayer);
        if (objectsToHit.Length > 0)
        {
            _recoilDir = true;
        }
        for (int i = 0; i < objectsToHit.Length; i++)
        {
            Enemy enemy = objectsToHit[i].GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.EnemyHit(damage, (transform.position - objectsToHit[i].transform.position).normalized, _recoilStrength);
                if (objectsToHit[i].CompareTag("Enemy"))
                {
                    Mana += manaGain;
                }
            }
        }
    }

    void slashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform)
    {
        _slashEffect = Instantiate(_slashEffect, _attackTransform);
        _slashEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
        _slashEffect.transform.localScale = new Vector2(transform.localScale.x, _attackTransform.localScale.y);
    }

    void Recoil()
    {
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                rb.velocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                rb.velocity = new Vector2(recoilXSpeed, 0);
            }
        }
        if (pState.recoilingY)
        {
            rb.gravityScale = 0;
            if (yAxis < 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, recoilYSpeed);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, -recoilYSpeed);
            }
            airJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        if (pState.recoilingX && stepsXRecoiled < recoilXsteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }

        if (pState.recoilingY && stepsYRecoiled < recoilYsteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }

        if (Grounded())
        {
            StopRecoilY();
        }
    }

    void Heal()
    {
        if (Input.GetButton("Cast/Heal") && castOrHealTimer > 0.05f && Health < maxHealth && Mana > 0 && Grounded() && !pState.dashing)
        {
            pState.healing = true;
            anim.SetBool("healing", true);
            healTimer += Time.deltaTime;
            if (healTimer >= timeToHeal)
            {
                Health++;
                healTimer = 0;
            }
            Mana -= Time.deltaTime * manaDrainSpeed;
        }
        else
        {
            pState.healing = false;
            anim.SetBool("healing", false);
            healTimer = 0;
        }
    }

    float Mana
    {
        get
        {
            return mana;
        }
        set
        {
            if (mana != value)
            {
                Debug.Log($"Mana changing from {mana} to {value}");
                mana = Mathf.Clamp(value, 0, 1);
                manaStorage.fillAmount = mana;
                Debug.Log($"New fillAmount: {manaStorage.fillAmount}");
            }
        }
    }


    void CastSpell()
    {
        if (Input.GetButtonUp("Cast/Heal") && castOrHealTimer <= 0.05f && timeSinceCast >= timeBetweenCast && Mana >= manaSpellCost)
        {
            pState.casting = true;
            timeSinceCast = 0;
            StartCoroutine(CastCoroutine());
        }
        else
        {
            timeSinceCast += Time.deltaTime;
        }
        if (Grounded())
        {
            downSpellFireball.SetActive(false);
        }
        if (downSpellFireball.activeInHierarchy)
        {
            rb.velocity += downSpellForce * Vector2.down;
        }
    }


    IEnumerator CastCoroutine()
    {
        anim.SetBool("casting", true);
        yield return new WaitForSeconds(0.15f);

        if (yAxis == 0 || (yAxis < 0 && Grounded()))
        {
            GameObject _fireBall = Instantiate(sideSpellFireball, SideAttackTransform.position, Quaternion.identity);
            if (pState.lookingRight)
            {
                _fireBall.transform.eulerAngles = Vector3.zero;
            }
            else
            {
                _fireBall.transform.eulerAngles = new Vector2(_fireBall.transform.eulerAngles.x, 180);
            }
            pState.recoilingX = true;
        }
        else if (yAxis > 0)
        {
            Instantiate(upSpellExplosion, transform);
            rb.velocity = Vector2.zero;
        }
        else if (yAxis < 0 && !Grounded())
        {
            downSpellFireball.SetActive(true);
        }

        Mana -= manaSpellCost;
        yield return new WaitForSeconds(0.35f);
        anim.SetBool("casting", false);
        pState.casting = false;
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

        void StopRecoilX()
        {
            stepsXRecoiled = 0;
            pState.recoilingX = false;
        }

        void StopRecoilY()
        {
            stepsYRecoiled = 0;
            pState.recoilingY = false;
        }

        public void TakeDamage(float _damage)
        {
            Debug.Log($"TakeDamage called - Current Health before: {Health}");
            Health = Health - Mathf.RoundToInt(_damage); // Use the property instead of the field
            Debug.Log($"Health after damage: {Health}");
            StartCoroutine(StopTakingDamage());
        }

        IEnumerator StopTakingDamage()
        {
            pState.invincible = true;
            GameObject _bloodSpurtParticles = Instantiate(bloodSpurt, transform.position, Quaternion.identity);
            Destroy(_bloodSpurtParticles, 1.5f);
            anim.SetTrigger("takeDamage");
            yield return new WaitForSeconds(1f);
            pState.invincible = false;
        }

        void FlashWhileInvincible()
        {
        if (pState.cutscene) return;
        sr.material.color = pState.invincible ? Color.Lerp(Color.white, Color.black, Mathf.PingPong(Time.time * hitFlashSpeed, 1.0f)) : Color.white;
        }


        void RestoreTimeScale()
        {
            if (restoreTime)
            {
                if (Time.timeScale < 1)
                {
                    Time.timeScale += Time.unscaledDeltaTime * restoreTimeSpeed;
                }
                else
                {
                    Time.timeScale = 1;
                    restoreTime = false;
                }
            }
        }

        public void HitStopTime(float _newTimeScale, int _restoreSpeed, float _delay)
        {
            restoreTimeSpeed = _restoreSpeed;
            if (_delay > 0)
            {
                StopCoroutine(StartTimeAgain(_delay));
                StartCoroutine(StartTimeAgain(_delay));
            }
            else
            {
                restoreTime = true;
            }
            Time.timeScale = _newTimeScale;
        }

        IEnumerator StartTimeAgain(float _delay)
        {
            yield return new WaitForSecondsRealtime(_delay);
            restoreTime = true;
        }

    private int health;
    public int Health
    {
        get { return health; }
        set
        {
            Debug.Log($"Health changing from {health} to {value}");
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);
                Debug.Log($"Health clamped to: {health}");
                onHealthChangedCallback?.Invoke();
            }
        }
    }

    void Jump()
    {
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !pState.jumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, JumpForce);
            pState.jumping = true;
        }
        if (!Grounded() && airJumpCounter < maxAirJumps && Input.GetButtonDown("Jump"))
        {
            pState.jumping = true;
            airJumpCounter++;
            rb.velocity = new Vector2(rb.velocity.x, JumpForce);
        }
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 3)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            pState.jumping = false;
        }
        anim.SetBool("Jumping", !Grounded());
    }

    void UpdateJumpVariables()
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

    private void StopDash()
    {
        StopCoroutine("Dash");
        rb.gravityScale = gravity;
        pState.dashing = false;
        anim.SetBool("IsDashing", false);
        StartCoroutine(DashCooldown());
    }

    public IEnumerator WalkIntoNewScene(Vector2 _exitDir, float _delay)
    {
        pState.invincible = true;
        if (_exitDir.y > 0)
        {
            rb.velocity = JumpForce * _exitDir;
        }
        if (_exitDir.x != 0)
        {
            xAxis = _exitDir.x > 0 ? 1 : -1;
            Move();
        }

        Flip();
        yield return new WaitForSeconds(_delay);
        pState.invincible = false;
        pState.cutscene = false;
    }

}