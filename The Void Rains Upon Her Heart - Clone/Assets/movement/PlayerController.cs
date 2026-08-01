 using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    
    [Header("Combat Settings")]
    [SerializeField] private BulletManager bulletManager;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.15f;
    
    private Rigidbody2D rb;
    private Camera mainCamera;
    
    private Vector2 moveInput;
    private Vector2 mousePos;
    private Vector2 aimDirection;
    
    private bool isDashing;
    private float dashTimer;
    private Vector2 dashDirection;
    
    private float fireCooldown;
    private bool isFiring;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        //Cursor.lockState = CursorLockMode.Confined;
        
        // Quick sanity checks so I know if something's missing
        if (bulletManager == null)
            Debug.LogWarning("BulletManager not assigned on " + gameObject.name);
        if (firePoint == null)
            Debug.LogWarning("FirePoint not assigned on " + gameObject.name);
    }
    
    void Update()
    {
        GetMovementInput();
        GetAimDirection();
        HandleShooting();
        HandleDashTiming();
    }
    
    void FixedUpdate()
    {
        MovePlayer();
    }
    
    void GetMovementInput()
    {
        // WASD or arrow keys, your choice
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        moveInput = new Vector2(horizontal, vertical);
        
        // Stops you from moving faster diagonally
        if (moveInput.magnitude > 1f)
            moveInput.Normalize();
        
        // Dash only works if you're actually moving
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && moveInput != Vector2.zero)
        {
            StartDash();
        }
    }
    
    void GetAimDirection()
    {
        // Where's the mouse in the game world?
        mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        
        // Direction from player to mouse
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
        aimDirection = direction;
        
        // Spin the player to face the mouse
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
    
    void HandleShooting()
    {
        // Cooldown counting
        if (fireCooldown > 0)
            fireCooldown -= Time.deltaTime;
        
        // Hold left click for auto-fire
        isFiring = Input.GetMouseButton(0);
        
        if (isFiring && fireCooldown <= 0)
        {
            FireBullet();
            fireCooldown = fireRate;
        }
        
        // Single click also works (catches the first frame of clicking)
        if (Input.GetMouseButtonDown(0) && fireCooldown <= 0)
        {
            FireBullet();
            fireCooldown = fireRate;
        }
    }
    
    void FireBullet()
    {
        // Don't crash if someone forgot to set things up
        if (bulletManager == null || firePoint == null)
            return;
        
        bulletManager.ShootBullet(firePoint.position, aimDirection);
    }
    
    void HandleDashTiming()
    {
        if (!isDashing)
            return;
        
        dashTimer -= Time.deltaTime;
        
        if (dashTimer <= 0f)
        {
            isDashing = false;
            rb.gravityScale = 1f; // Turn gravity back on if you had it off
        }
    }
    
    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashDirection = moveInput.normalized;
        rb.gravityScale = 0f; // Turn off gravity so you dash smoothly
    }
    
    void MovePlayer()
    {
        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
        }
        else
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }
    
    // Shows where bullets come from in the editor - helpful for setup
    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.15f);
        }
    }
}
