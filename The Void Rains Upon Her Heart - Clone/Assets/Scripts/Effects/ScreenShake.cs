using UnityEngine;
using DG.Tweening;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    [Header("Minor Shake")]
    [SerializeField] private float _minorDuration = 0.15f;
    [SerializeField] private float _minorStrength = 0.2f;
    [SerializeField] private int _minorVibrato = 15;

    [Header("Major Shake")]
    [SerializeField] private float _majorDuration = 0.4f;
    [SerializeField] private float _majorStrength = 0.5f;
    [SerializeField] private int _majorVibrato = 25;

    private Vector3 _originalPosition;
    private Tween _currentShake;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _originalPosition = transform.localPosition;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MinorShake()
    {
        Shake(_minorDuration, _minorStrength, _minorVibrato);
    }

    public void MajorShake()
    {
        Shake(_majorDuration, _majorStrength, _majorVibrato);
    }

    public void Shake(float duration, float strength, int vibrato)
    {
        if (_currentShake != null && _currentShake.IsActive())
            _currentShake.Kill();

        transform.localPosition = _originalPosition;

        _currentShake = transform.DOShakePosition(
            duration,
            strength,
            vibrato,
            randomness: 90f,
            snapping: false,
            fadeOut: true
        )
        .OnComplete(() =>
        {
            transform.localPosition = _originalPosition;
        });
    }
}