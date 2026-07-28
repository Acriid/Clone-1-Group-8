using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossSection : MonoBehaviour
{

    [SerializeField] private BulletManager _positiveBulletManager;
    [SerializeField] private BulletManager _negativeBulletManager;
    [SerializeField] private float _sectionSpeed = 1f;
    

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
    private Coroutine _bulletRoutine = null;
    private Coroutine _rotateRoutine = null;

    private bool _isPositive = true;
    #region Damage

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
    #endregion
    //DELETE LATER
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






    #region UnityFunctions
    private void Awake()
    {
        if(_laser != null)
        _laser.OnLaserFinished += FinishedLaserShot;
    }
    private void OnDisable()
    {
        if(_laser != null)
        _laser.OnLaserFinished -= FinishedLaserShot;
    }
    #endregion
    #region Movement
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
    #endregion
    #region Rotation
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
    #endregion
    #region SinWave
    public void MoveSinWave(float shotSpeed,float bulletSpeed)
    {
        if(_sinRoutine != null)
        {
            ShootBullet(shotSpeed,Vector2.left,bulletSpeed);
            StopCoroutine(_sinRoutine);
            _sinRoutine = null;
        }
        else
        {
            _startPosition = transform.position;
            _sinRoutine = StartCoroutine(MoveSinWaveEnumerator());
            ShootBullet(shotSpeed,Vector2.left,bulletSpeed);
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
    #endregion
    #region Shooting Bullets
    public void ShootBullet(float shotSpeed, Vector2 shootDirection,float bulletSpeed)
    {
        if(_bulletRoutine != null)
        {
            StopCoroutine(_bulletRoutine);
            _bulletRoutine = null;
        }
        else
        {
            _bulletRoutine = StartCoroutine(ShootSinBullets(shotSpeed,shootDirection,bulletSpeed));
        }
    }
    public void Phase2ShootFourDirections(float shotSpeed, int bulletAmount, BulletDirection direction, float offset)
    {
        StartCoroutine(ShootBulletsFromDirection(shotSpeed,direction,bulletAmount,offset));
    }
    public IEnumerator ShootBulletsFromDirection(float shotSpeed, BulletDirection direction, int bulletAmount,float offset)
    {
        
        WaitForSeconds waitTime = new(shotSpeed);
        int bulletsShot = 0;

        while (bulletsShot < bulletAmount)
        {
            Vector2 dir = GetDirectionVector(direction);
            dir = Quaternion.Euler(0f,0f,offset) * dir;
            ShootBullet(dir);
            bulletsShot++;
            yield return waitTime;
        }
    }
    private IEnumerator ShootSinBullets(float shootSpeed, Vector2 shootDirection, float bulletSpeed)
    {
        WaitForSeconds waitTime = new(shootSpeed);
        while(true)
        {
            ShootBullet(shootDirection,bulletSpeed);
            yield return waitTime;
        }
    }
    public void ShootBullet(Vector2 bulletDirection)
    {
        if(_isPositive)
        {
            _positiveBulletManager.ShootBullet(transform.position,bulletDirection);
        }
        else
        {
            _negativeBulletManager.ShootBullet(transform.position,bulletDirection);
        }
    }
    public void ShootBullet(Vector2 bulletDirection, float bulletSpeed)
    {
        if(_isPositive)
        {
            _positiveBulletManager.ShootBullet(transform.position,bulletDirection,bulletSpeed);
        }
        else
        {
            _negativeBulletManager.ShootBullet(transform.position,bulletDirection,bulletSpeed);
        }
    }
    #endregion
    #region Laser
    public void ShootLaser()
    {
        _laser.ShootLaser(transform);
    }
    public void SetLaserSO(LaserSO newLaserSO)
    {
        _laser.SetLaserSO(newLaserSO);
    }
    public void StopLaser()
    {
        _laser.StopLaser();
    }
    private void FinishedLaserShot()
    {
        OnLaserFinished?.Invoke(this);
    }
    #endregion
    private Vector2 GetDirectionVector(BulletDirection direction)
    {
        return direction switch
        {
            BulletDirection.Up => (Vector2)transform.up,
            BulletDirection.Down => (Vector2)(-transform.up),
            BulletDirection.Left => (Vector2)(-transform.right),
            BulletDirection.Right => (Vector2)transform.right,
            _ => (Vector2)transform.up,
        };
    }
    public void StopAllRoutines()
    {
        StopAllCoroutines();
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

public enum BulletDirection { Up, Down, Left, Right }
