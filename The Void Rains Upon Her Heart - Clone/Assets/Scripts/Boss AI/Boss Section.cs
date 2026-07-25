using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BossSection : MonoBehaviour
{

    [SerializeField] private BulletManager _bulletManager;
    [SerializeField] private float _sectionSpeed = 1f;
    [SerializeField] private float _shotSpeed = 0.2f;

    [SerializeField] private float _movementDistance = 2f;
    [SerializeField] private float _phaseOffset = 0f;

    private float _sectionHealth = 400;
    private bool _sectionDestroyed = false;

    public event Action<float> OnBossDamage;
    public event Action OnFinishedMove;
    


    private Vector2 _startPosition = new(8f,0f);

    private Coroutine _moveRoutine = null;
    private Coroutine _sinRoutine = null;
    private Coroutine _sinBulletRoutine = null;
    public void Damage(float damage)
    {
        if(_sectionDestroyed)
        {
            damage *= 0.1f;
        }
        else
        {
            _sectionHealth -= damage;
            if(_sectionHealth <= 0)
            {
                _sectionDestroyed = true;
            }
        }

        OnBossDamage?.Invoke(damage);
        
    }

    private void DestroySection()
    {
        //TODO - implement the sprite change
    }
    private void Awake()
    {
        
    }

    private void Update()
    {

    }

    public void MoveSection(Vector2 movePosition)
    {
        if(_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }

        _moveRoutine = StartCoroutine(MoveSectionEnumerator(movePosition));

    }
    public void MoveSinWave()
    {
        if(_sinRoutine != null)
        {
            StopCoroutine(_sinBulletRoutine);
            StopCoroutine(_sinRoutine);
            _sinRoutine = null;
            _sinBulletRoutine = null;
        }
        else
        {
            _startPosition = transform.position;
            _sinRoutine = StartCoroutine(MoveSinWaveEnumerator());
            _sinBulletRoutine = StartCoroutine(ShootBullets(_shotSpeed,Vector2.left));
        }      
    }
    private IEnumerator ShootBullets(float shootSpeed, Vector2 shootDirection)
    {
        WaitForSeconds waitTime = new(shootSpeed);
        while(true)
        {
            ShootBullet(shootDirection);
            yield return waitTime;
        }
    }
    private void ShootBullet(Vector2 bulletDirection)
    {
        _bulletManager.ShootBullet(transform.position,Vector2.left);
    }
    private IEnumerator MoveSinWaveEnumerator()
    {
        while (true)
        {
            float movementOffset = Mathf.Sin(Time.time * _sectionSpeed + _phaseOffset) * _movementDistance;

            transform.position = _startPosition + Vector2.up * movementOffset;



            yield return null;
        }
    }
    private IEnumerator MoveSectionEnumerator(Vector2 movePosition)
    {
        float stoppingDistance = 0.001f;

        while (Vector2.Distance(transform.position, movePosition) > stoppingDistance)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                movePosition,
                _sectionSpeed * Time.deltaTime);

            yield return null;
        }

        transform.position = movePosition;
        OnFinishedMove?.Invoke();
    }
    private void DisplayTime(DateTime timeToDisplay)
    {
        int hour = timeToDisplay.Hour;
        int minute = timeToDisplay.Minute;
        int second = timeToDisplay.Second;
        int milliseconds = timeToDisplay.Millisecond;

        Debug.Log($"Current Time: {hour}:{minute}:{second}:{milliseconds}");         
    }

    public void ShootBullet(Vector2 spawnPoint, Vector2 bulletDirection)
    {
        _bulletManager.ShootBullet(spawnPoint,bulletDirection);
    }

}
