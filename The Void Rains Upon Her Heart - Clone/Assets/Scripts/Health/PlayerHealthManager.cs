using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PlayerHealthManager : MonoBehaviour
{
    [SerializeField] private float _MaxHealth = 16f;

    private float _CurrentHealth;

    public UnityEvent OnPlayerDeath;

    
    [SerializeField] private Slider _HealthSlider;

    public SpriteRenderer SpriteRenderer;
   private float _invincibilityAfterHitTime = 1f;
   private float _invincibilityCooldown = 1f;
    // sets current health at start to the players max health
    private void Awake()
    {
        _CurrentHealth = _MaxHealth;
        // _HealthSlider.maxValue = _MaxHealth;
        // _HealthSlider.value = _CurrentHealth;
    }
    void Update()
    {
        //TEMP
        _invincibilityCooldown -= Time.deltaTime;
        if(_invincibilityCooldown <= 0)
        {
            SpriteRenderer.color = Color.red;
        }
        //TEMP
    }
    // used by bullet script to damage the player
    public void TakeDamage(float damage)
    {
        if(_invincibilityCooldown >0) return;
        _CurrentHealth -= damage;
        _CurrentHealth = Mathf.Clamp(_CurrentHealth, 0, _MaxHealth); //makes sure current health doesnt fall below 0
        //_HealthSlider.value = _CurrentHealth; // update UI

        Debug.Log($"Player Health: {_CurrentHealth}");

        if (_CurrentHealth <= 0)
        {
            PlayerDeath();
        }

        _invincibilityCooldown = _invincibilityAfterHitTime;
        SpriteRenderer.color = Color.white;
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