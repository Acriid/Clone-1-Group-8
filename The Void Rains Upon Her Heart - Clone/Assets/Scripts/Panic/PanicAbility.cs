using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class PanicAbility : MonoBehaviour
{
    [Header("Panic Settings")]
    [SerializeField] private float _panicDuration = 2f;
    [SerializeField] private float _cooldown = 8f;
    [SerializeField] private int _panicAmount = 2;

    [Header("References")]
    [SerializeField] private PlayerHealthManager _playerHealth;
    [SerializeField] private Transform _panicEffect;
    [SerializeField] private ParticleSystem _heartParticles;
    [SerializeField] private TMP_Text _panicText;

    private CircleCollider2D _panicCollider;
    private Vector3 _originalScale;
    private bool _canUse = true;

    private void Awake()
    {
        if (_panicEffect == null)
        {
            Debug.LogError("Panic Effect has not been assigned!");
            enabled = false;
            return;
        }

        _panicCollider = _panicEffect.GetComponent<CircleCollider2D>();

        if (_panicCollider == null)
        {
            Debug.LogError("No CircleCollider2D found on Panic Effect!");
            enabled = false;
            return;
        }

        _originalScale = _panicEffect.localScale;

        // Hide the panic effect when the game starts
        _panicEffect.localScale = Vector3.zero;
        _panicCollider.enabled = false;

        if (_heartParticles != null)
        {
            _heartParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        //set max panic
        _panicAmount = 2;
        _panicText.text = _panicAmount.ToString();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _canUse && _panicAmount>0)
        {
            StartCoroutine(PanicRoutine());
        }
    }

    private IEnumerator PanicRoutine()
    {
        _canUse = false;

        // Minus one use

        _panicAmount -= 1;
        _panicText.text = _panicAmount.ToString();

        // Player cannot be damaged
        _playerHealth.IsInvulnerable = true;

        // Enable the panic hitbox
        _panicCollider.enabled = true;

        // Play heart particles
        if (_heartParticles != null)
        {
            _heartParticles.Play();
        }

        // Stop any existing tweens on the panic effect
        _panicEffect.DOKill();

        // Grow the panic circle to the size set in the Inspector
        _panicEffect.localScale = Vector3.zero;

        _panicEffect
            .DOScale(_originalScale, 0.25f)
            .SetEase(Ease.OutBack);

        // Ability active
        yield return new WaitForSeconds(_panicDuration);

        if (_heartParticles != null)
        {
            _heartParticles.Stop();
        }

        // Shrink the panic circle
        _panicEffect
            .DOScale(Vector3.zero, 0.25f)
            .SetEase(Ease.InBack);

        yield return new WaitForSeconds(0.25f);

        // Disable hitbox
        _panicCollider.enabled = false;

        // Player can take damage again
        _playerHealth.IsInvulnerable = false;

        // Stop particles
        if (_heartParticles != null)
        {
            _heartParticles.Stop();
        }

        // Cooldown
        yield return new WaitForSeconds(_cooldown);

        _canUse = true;
    }
}