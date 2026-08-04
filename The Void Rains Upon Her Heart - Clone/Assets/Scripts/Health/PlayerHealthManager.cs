using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

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

   public PlayerController player;
    public Rigidbody2D _rb;
    private void PlayerDeath()
    {
        Debug.Log("Player Died");

        OnPlayerDeath?.Invoke();

        

        player.enabled = false;

        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.simulated = false;    // Stops all physics

        StartCoroutine(DeathAnimation());
    }

    [SerializeField] private Image _whiteFlash;
    [SerializeField] private SpriteRenderer _playerSprite;

    [SerializeField] private Transform _blackHeart;
    [SerializeField] private Transform _crackedHeart;

    [SerializeField] private Transform _topHeartHalf;
    [SerializeField] private Transform _bottomHeartHalf;


    IEnumerator DeathAnimation()
    {
        // Remember where the player died
        Vector3 deathPosition = transform.position;

        // Move all death sprites to the death position
        _blackHeart.position = deathPosition;
        _crackedHeart.position = deathPosition;
        _topHeartHalf.position = deathPosition + new Vector3(0f, 0.275f, 0f);
        _bottomHeartHalf.position = deathPosition + new Vector3(0f, -0.275f, 0f);

        // Reset transforms
        _blackHeart.localScale = Vector3.one;
        _crackedHeart.localScale = Vector3.one;
        _topHeartHalf.localScale = Vector3.one;
        _bottomHeartHalf.localScale = Vector3.one;

        _blackHeart.rotation = Quaternion.identity;
        _crackedHeart.rotation = Quaternion.identity;
        _topHeartHalf.rotation = Quaternion.identity;
        _bottomHeartHalf.rotation = Quaternion.identity;

        // Hide everything except the player
        _blackHeart.gameObject.SetActive(false);
        _crackedHeart.gameObject.SetActive(false);
        _topHeartHalf.gameObject.SetActive(false);
        _bottomHeartHalf.gameObject.SetActive(false);

        // Hide the player
        _playerSprite.enabled = false;
        _whiteFlash.gameObject.SetActive(true);

        // ------------------------
        // Black heart appears
        // ------------------------

        _blackHeart.gameObject.SetActive(true);

        Sequence shake = DOTween.Sequence();

        shake.Join(_blackHeart.DOShakePosition(
            1f,
            strength: 0.05f,
            vibrato: 35));

        shake.Join(_blackHeart.DOShakeRotation(
            1f,
            strength: 12));

        yield return shake.WaitForCompletion();

        // ------------------------
        // Show cracked heart
        // ------------------------


        _crackedHeart.gameObject.SetActive(true);

        Vector3 startPos = deathPosition + Vector3.left * 3f;
        _crackedHeart.position = startPos;

        yield return _crackedHeart
            .DOMove(deathPosition, 1f)
            .SetEase(Ease.OutCubic)
            .WaitForCompletion();

        _blackHeart.gameObject.SetActive(false);
        _crackedHeart.gameObject.SetActive(false);

        // ------------------------
        // Split heart
        // ------------------------

        _crackedHeart.gameObject.SetActive(false);
        _blackHeart.gameObject.SetActive(false);

        _topHeartHalf.gameObject.SetActive(true);
        _bottomHeartHalf.gameObject.SetActive(true);

        Sequence split = DOTween.Sequence();

        split.Join(
            _topHeartHalf.DOMove(
                deathPosition + new Vector3(0f, 5f, 0f),
                0.5f));

        split.Join(
            _bottomHeartHalf.DOMove(
                deathPosition + new Vector3(0f, -5f, 0f),
                0.5f));

       

        yield return split.WaitForCompletion();

        yield return new WaitForSeconds(1.5f);

        // Hide the pieces
        _topHeartHalf.gameObject.SetActive(false);
        _bottomHeartHalf.gameObject.SetActive(false);
        

        //load main menu Scene
        
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