using UnityEngine;
using UnityEngine.Events;

public class PlayerHealthManager : MonoBehaviour
{
    [SerializeField] private float _MaxHealth = 100f;

    private float _CurrentHealth;

    public UnityEvent OnPlayerDeath;


    // sets current health at start to the players max health
    private void Awake()
    {
        _CurrentHealth = _MaxHealth;
    }

    // used by bullet script to damage the player
    public void TakeDamage(float damage)
    {
        _CurrentHealth -= damage;
        _CurrentHealth = Mathf.Clamp(_CurrentHealth, 0, _MaxHealth); //makes sure current health doesnt fall below 0

        Debug.Log($"Player Health: {_CurrentHealth}");

        if (_CurrentHealth <= 0)
        {
            PlayerDeath();
        }
    }

    private void PlayerDeath()
    {
        Debug.Log("Player Died");

        OnPlayerDeath?.Invoke();

        // Disable player controls
        // Play animation
        // Game Over
        // Will fill this in later
    }

    public float GetCurrentHealth()
    {
        return _CurrentHealth;
    }

    public float GetMaxHealth()
    {
        return _MaxHealth;
    }
}