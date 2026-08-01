  using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float focusSpeed = 2.5f; // Slower speed when focusing
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    
    [Header("Combat Settings")]
    [SerializeField] private BulletManager bulletManager;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.15f;
    
    [Header("Focus Settings")]
    [SerializeField] private float focusFOV = 50f; // Optional: zoom in when focusing
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] private float fovTransitionSpeed = 10f;
    
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
    
    private bool isFocusing;
    private float targetFOV;
    private Camera camComponent;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        camComponent = mainCamera.GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Confined;
        
        // Set initial FOV
        targetFOV = normalFOV;
        camComponent.fieldOfView = normalFOV;
        
        // Quick sanity checks
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
        HandleFocus();
        HandleFOVTransition();
    }
    
    void FixedUpdate()
    {
        MovePlayer();
    }
    
    void GetMovementInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        moveInput = new Vector2(horizontal, vertical);
        
        if (moveInput.magnitude > 1f)
            moveInput.Normalize();
        
        // Check for focus mode (hold Shift)
        isFocusing = Input.GetKey(KeyCode.LeftShift) && !isDashing;
        
        // Dash only works if you're moving and press Shift
        // Using GetKeyDown for dash activation (tap Shift)
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && moveInput != Vector2.zero)
        {
            StartDash();
        }
    }
    
    void GetAimDirection()
    {
        mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
        aimDirection = direction;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }
    
    void HandleShooting()
    {
        if (fireCooldown > 0)
            fireCooldown -= Time.deltaTime;
        
        isFiring = Input.GetMouseButton(0);
        
        if (isFiring && fireCooldown <= 0)
        {
            FireBullet();
            fireCooldown = fireRate;
        }
        
        if (Input.GetMouseButtonDown(0) && fireCooldown <= 0)
        {
            FireBullet();
            fireCooldown = fireRate;
        }
    }
    
    void FireBullet()
    {
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
            rb.gravityScale = 1f;
        }
    }
    
    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashDirection = moveInput.normalized;
        rb.gravityScale = 0f;
    }
    
    void HandleFocus()
    {
        // You can add focus-specific effects here
        // For example: slower fire rate, more accuracy, damage boost, etc.
        if (isFocusing)
        {
            // Optional: Reduce fire rate when focusing (uncomment if desired)
            // fireRate = 0.1f; // Faster shooting while focused
        }
    }
    
    void HandleFOVTransition()
    {
        // Smoothly transition FOV when focusing/unfocusing
        if (isFocusing && !isDashing)
        {
            targetFOV = focusFOV;
        }
        else
        {
            targetFOV = normalFOV;
        }
        
        camComponent.fieldOfView = Mathf.Lerp(
            camComponent.fieldOfView, 
            targetFOV, 
            fovTransitionSpeed * Time.deltaTime
        );
    }
    
    void MovePlayer()
    {
        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
        }
        else if (isFocusing && moveInput != Vector2.zero)
        {
            // Focus mode: slower movement
            rb.linearVelocity = moveInput * focusSpeed;
        }
        else
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.15f);
        }
    }
}