using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealthManager : MonoBehaviour
{
    [SerializeField] private float _MaxHealth = 16f;

    private float _CurrentHealth;

    public UnityEvent OnPlayerDeath;

    
    [SerializeField] private Slider _HealthSlider;
    [SerializeField] private Slider _DamageSlider;


    public SpriteRenderer SpriteRenderer;
   private float _invincibilityAfterHitTime = 1f;
   private float _invincibilityCooldown = 1f;
    public bool IsInvulnerable;

    // sets current health at start to the players max health
    private void Awake()
    {
        _CurrentHealth = _MaxHealth;
        _HealthSlider.maxValue = _MaxHealth;
        _HealthSlider.value = _CurrentHealth;
        _DamageSlider.maxValue = _MaxHealth;
        _DamageSlider.value = _CurrentHealth;
        IsInvulnerable = false;
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
        if (IsInvulnerable) return;
        _CurrentHealth -= damage;
        _CurrentHealth = Mathf.Clamp(_CurrentHealth, 0, _MaxHealth); //makes sure current health doesnt fall below 0
        CameraShake.Instance.MinorShake();
        // _HealthSlider.value = _CurrentHealth; // update UI
        //StartCoroutine(HealthBarAnimation());

        Debug.Log($"Player Health: {_CurrentHealth}");

        if (_CurrentHealth <= 0)
        {
            PlayerDeath();
        }

        StartCoroutine(HealthBarAnimation());
        PlayerDamageAnimations();

        _invincibilityCooldown = _invincibilityAfterHitTime;
        SpriteRenderer.color = Color.white;
    }

    private void PlayerDamageAnimations()
    {
        
    }

    IEnumerator HealthBarAnimation()
    {
        _HealthSlider.value = _CurrentHealth;
        yield return new WaitForSeconds(2f);
        //add shakes when health low
        while (_HealthSlider.value !<= _DamageSlider.value) 
        {
            _DamageSlider.value -= 0.3f;
            yield return new WaitForSeconds(0.05f);

        }

    }

    private void PlayerDeath()
    {
        Debug.Log("Player Died");

        OnPlayerDeath?.Invoke();
        CameraShake.Instance.MinorShake();

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