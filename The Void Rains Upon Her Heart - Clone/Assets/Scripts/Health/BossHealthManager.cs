using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthManager : MonoBehaviour
{
   [SerializeField] private float _maxHealth = 0f;
    [SerializeField] private float _currentHealth = 0f;
    [SerializeField] private BossBrain _bossBrain;
   private List<BossSection> _bossSections;
    [SerializeField] private Slider _HealthSlider;
    [SerializeField] private Slider _DamageSlider;


    void Awake()
    {
        _maxHealth = _bossBrain.GetMaxHealth();
        _bossSections = _bossBrain.GetBossSections();

        _currentHealth = _maxHealth;
        _HealthSlider.maxValue = _maxHealth;
        _HealthSlider.value = _currentHealth;
        _DamageSlider.maxValue = _maxHealth;
        _DamageSlider.value = _currentHealth;

        foreach (BossSection bossSection in _bossSections)
        {
            bossSection.OnBossDamage += TakeDamage;
        }
    }

    void OnDisable()
    {
        foreach(BossSection bossSection in _bossSections)
        {
            bossSection.OnBossDamage -= TakeDamage;
        }       
    }
    private void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth); //makes sure current health doesnt fall below 0

    

        if (_currentHealth <= 0)
        {
           BossDeath();
        }

        StartCoroutine(HealthBarAnimation());

    }

    IEnumerator HealthBarAnimation()
    {
       //flash White
        //add shakes when health low
        while (_HealthSlider.value >= _currentHealth)
        {
            _HealthSlider.value -= 1f;
            yield return new WaitForSeconds(0.05f);

        }

    }

    IEnumerator HealthBarAnimationEnd()
    {
        CameraShake.Instance.MinorShake();

        while (_DamageSlider.value > _HealthSlider.value)
        {
            _DamageSlider.value = Mathf.Max(
                _HealthSlider.value,
                _DamageSlider.value - 10f);

            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.05f);
        StartCoroutine(WinAnimation());
    }

    private void BossDeath()
    {
        StartCoroutine(HealthBarAnimationEnd());
       // StartCoroutine(WinAnimation());
    }

    [SerializeField] private SpriteRenderer _playerSprite;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private ParticleSystem _winParticles;
    [SerializeField] private ParticleSystem _trailParticles;
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private Rigidbody2D _rb;

    [SerializeField] private float _flyDistance = 20f;
    [SerializeField] private float _flyTime = 4f;

    private IEnumerator WinAnimation()
    {
        // Disable player controls
        //_playerController.enabled = false;

        // Stop movement
        _rb.linearVelocity = Vector2.zero;
        _rb.angularVelocity = 0f;
        _rb.simulated = false;

        // Play victory burst
        if (_winParticles != null)
            _winParticles.Play();

        yield return new WaitForSeconds(1f);

        // Start trail
        if (_trailParticles != null)
            _trailParticles.Play();

        Vector3 targetPosition =
    _playerTransform.position + Vector3.right * _flyDistance;

     

        yield return _playerTransform
            .DOMove(targetPosition, _flyTime)
            .SetEase(Ease.InQuad)
            .WaitForCompletion();

        // Stop particles
        if (_trailParticles != null)
            _trailParticles.Stop();

        // Hide player
        _playerSprite.enabled = false;

        // main menu scene
    }

}
