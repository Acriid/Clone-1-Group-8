  using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Get input from WASD using the new Input System
        float moveX = 0f;
        float moveY = 0f;

        // WASD
        if (Keyboard.current.wKey.isPressed) moveY = 1f;
        if (Keyboard.current.sKey.isPressed) moveY = -1f;
        if (Keyboard.current.aKey.isPressed) moveX = -1f;
        if (Keyboard.current.dKey.isPressed) moveX = 1f;

        // Arrow Keys (override WASD if both are pressed)
        if (Keyboard.current.upArrowKey.isPressed) moveY = 1f;
        if (Keyboard.current.downArrowKey.isPressed) moveY = -1f;
        if (Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
        if (Keyboard.current.rightArrowKey.isPressed) moveX = 1f;

        moveInput = new Vector2(moveX, moveY).normalized;

        Flip();
    }

    void Flip()
    {
        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            transform.Rotate(0, 180, 0);
        }
    }

    void FixedUpdate()
    {
        // Move the player using physics
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}