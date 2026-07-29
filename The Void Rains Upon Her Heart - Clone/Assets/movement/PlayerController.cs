 using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.15f;
    
    [Header("Combat")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.15f;
    [SerializeField] private float bulletSpeed = 20f;
    
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
        Cursor.lockState = CursorLockMode.Confined;
    }
    
    void Update()
    {
        // MOVEMENT - Works with both old and new input systems
        float horizontal = 0f;
        float vertical = 0f;
        
        // Try old input first
        #if ENABLE_LEGACY_INPUT_MANAGER
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        #endif
        
        // Also check new input if available
        #if ENABLE_INPUT_SYSTEM
        // If you're using the new system, uncomment and set up your input actions
        // var keyboard = Keyboard.current;
        // if (keyboard != null)
        // {
        //     if (keyboard.wKey.isPressed) vertical = 1f;
        //     if (keyboard.sKey.isPressed) vertical = -1f;
        //     if (keyboard.aKey.isPressed) horizontal = -1f;
        //     if (keyboard.dKey.isPressed) horizontal = 1f;
        // }
        #endif
        
        moveInput = new Vector2(horizontal, vertical);
        if (moveInput.magnitude > 1f)
            moveInput.Normalize();
        
        // MOUSE POSITION
        mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        
        // DASH
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && moveInput != Vector2.zero)
        {
            StartDash();
        }
        
        // SHOOTING
        isFiring = Input.GetMouseButton(0);
        
        // AIMING
        Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
        aimDirection = direction;
        
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        
        // SHOOTING COOLDOWN
        if (fireCooldown > 0)
            fireCooldown -= Time.deltaTime;
        
        if (isFiring && fireCooldown <= 0)
        {
            ShootBullet();
            fireCooldown = fireRate;
        }
        
        if (Input.GetMouseButtonDown(0) && fireCooldown <= 0)
        {
            ShootBullet();
            fireCooldown = fireRate;
        }
        
        // DASH TIMER
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0)
            {
                isDashing = false;
                rb.gravityScale = 1f;
            }
        }
    }
    
    void FixedUpdate()
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
    
    void ShootBullet()
    {
        if (bulletPrefab == null || firePoint == null)
            return;
        
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        
        if (bulletRb != null)
        {
            bulletRb.linearVelocity = aimDirection * bulletSpeed;
        }
        
        Destroy(bullet, 3f);
    }
    
    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashDirection = moveInput.normalized;
        rb.gravityScale = 0f;
    }
    
    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(firePoint.position, 0.2f);
        }
    }
}
