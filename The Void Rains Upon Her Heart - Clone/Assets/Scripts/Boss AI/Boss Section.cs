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
    [SerializeField] private Laser _laser;

    //Health for lvl 2 per section is 349
    //Health for lvl 2 for phase 2 is 97 per section
    private float _sectionHealth = 349;
    private bool _sectionDestroyed = false;

    public event Action<float> OnBossDamage;
    public event Action<BossSection> OnSectionDestroyed;
    public event Action<BossSection> OnFinishedMove;
    public event Action<BossSection> OnLaserFinished;
    

    private Vector2 _startPosition = new(8f,0f);

    private Coroutine _moveRoutine = null;
    private Coroutine _sinRoutine = null;
    private Coroutine _sinBulletRoutine = null;
    private Coroutine _rotateRoutine = null;
    public void Damage(float damage)
    {
        if(_sectionDestroyed)
        {
            damage *= 0f;
        }
        else
        {
            _sectionHealth -= damage;
            if(_sectionHealth <= 0)
            {
                _sectionDestroyed = true;
                OnSectionDestroyed?.Invoke(this);
            }
        }

        OnBossDamage?.Invoke(damage);
    }

    public void TESTDESTROYSECTION()
    {
        _sectionHealth = 0f;
        _sectionDestroyed = true;
        OnSectionDestroyed?.Invoke(this);
    }






    private void DestroySection()
    {
        //TODO - implement the sprite change
    }
    private void Awake()
    {
        _laser.OnLaserFinished += FinishedLaserShot;
    }
    private void OnDisable()
    {
        _laser.OnLaserFinished -= FinishedLaserShot;
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
    public void MoveSection(Vector2 movePosition, float timeToMove)
    {
        if(_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }

        _moveRoutine = StartCoroutine(MoveSectionEnumerator(movePosition,timeToMove));

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
        OnFinishedMove?.Invoke(this);
    }

    private IEnumerator MoveSectionEnumerator(Vector2 movePosition, float moveTime)
    {
        Vector2 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / moveTime);

            transform.position = Vector2.Lerp(
                startPosition,
                movePosition,
                t);

            yield return null;
        }

        transform.position = movePosition;
        OnFinishedMove?.Invoke(this);
    }

    public void RotateAndShoot(float shootInterval,float rotateSpeed)
    {
        _rotateRoutine = StartCoroutine(RotateOverTime(shootInterval,rotateSpeed));

    }

    public void RotateAndShoot()
    {
        if(_rotateRoutine != null)
        {
            StopCoroutine(_rotateRoutine);
            _rotateRoutine = null;
        }
    }

    private IEnumerator RotateOverTime(float shootInterval,float rotateSpeed)
    {
        float bulletShot = -1f;
        while(true)
        {
            transform.Rotate(0,0,rotateSpeed * Time.deltaTime);
            if(bulletShot < 0)
            {
                ShootBullet(transform.up);
                bulletShot = shootInterval;
            }
            bulletShot -= Time.deltaTime;
            yield return null;
        }
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

    private IEnumerator MoveSinWaveEnumerator()
    {

        float timeTracker = 0f;
        while (true)
        {
            float movementOffset = -Mathf.Sin(timeTracker * _sectionSpeed) * _movementDistance;

            transform.position = _startPosition + Vector2.up * movementOffset;


            timeTracker += Time.deltaTime;
            yield return null;
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
        _bulletManager.ShootBullet(transform.position,bulletDirection);
    }

    public void ShootLaser()
    {
        _laser.ShootLaser(transform);
    }
    public void SetLaserSO(LaserSO newLaserSO)
    {
        _laser.SetLaserSO(newLaserSO);
    }
    private void FinishedLaserShot()
    {
        OnLaserFinished?.Invoke(this);
    }


    private void DisplayTime(DateTime timeToDisplay)
    {
        int hour = timeToDisplay.Hour;
        int minute = timeToDisplay.Minute;
        int second = timeToDisplay.Second;
        int milliseconds = timeToDisplay.Millisecond;

        Debug.Log($"Current Time: {hour}:{minute}:{second}:{milliseconds}");         
    }

}
