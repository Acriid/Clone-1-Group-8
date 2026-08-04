using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BossSection : MonoBehaviour
{
    private static WaitForSeconds _waitForSeconds0_1 = new WaitForSeconds(0.1f);
    [SerializeField] private Collider2D _collider;
    [SerializeField] private BulletManager _positiveBulletManager;
    [SerializeField] private BulletManager _negativeBulletManager;
    [SerializeField] private Laser _laser;
    [SerializeField] private Sprite _brokenSpritePositive;
    [SerializeField] private Sprite _brokenSpriteNegative;
    [SerializeField] private Sprite _brokenSpritePositivePhase2;
    [SerializeField] private Sprite _brokenSpriteNegativePhase2;

    [SerializeField] private Sprite _spritePositive;
    [SerializeField] private Sprite _spriteNegative;
    //Temp Delete Later
    public SpriteRenderer SpriteRenderer;

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

    private bool _isPositive = false;

    private int _currentPhase = 1;
    #region Damage

    public void Damage(float damage)
    {
        if(_sectionDestroyed)
        {
            damage *= 0f;
            if(SpriteRenderer.sprite != _brokenSpritePositive || SpriteRenderer.sprite != _brokenSpriteNegative)
            {
                ChangePositiveOrNegative(_isPositive);
            }

        }
        else
        {
            StartCoroutine(IndicateHit());
            _sectionHealth -= damage;
            if(_sectionHealth <= 0)
            {
                
                _sectionDestroyed = true;
                OnSectionDestroyed?.Invoke(this);
                
                ChangePositiveOrNegative(_isPositive);
            }
        }

        OnBossDamage?.Invoke(damage);
    }
    public void ChangePositiveOrNegative(bool isPositive)
    {
        _isPositive = isPositive;
        if(_sectionDestroyed && _currentPhase == 2)
        {
            _collider.enabled = false;
            if(_isPositive)
            {
                SpriteRenderer.sprite = _brokenSpritePositivePhase2;
            }
            else
            {
                SpriteRenderer.sprite = _brokenSpriteNegativePhase2;
            }
            return;
        }
        if(_isPositive)
        {
            if(_laser != null)
            _laser.ChangeLaserColour(Color.red);

            SpriteRenderer.sprite = _spritePositive;
            if(_sectionDestroyed)
            {
                SpriteRenderer.sprite = _brokenSpritePositive;
            }
        }
        else
        {
            if(_laser != null)
            _laser.ChangeLaserColour(Color.blue);

            SpriteRenderer.sprite = _spriteNegative;
            if(_sectionDestroyed)
            {
                SpriteRenderer.sprite = _brokenSpriteNegative;
            }         
        }
        
    }
    private IEnumerator IndicateHit()
    {
        SpriteRenderer.color = Color.gray;
        yield return _waitForSeconds0_1;
        SpriteRenderer.color = Color.white;
    }
    #endregion
    public void StartPhase2(float newHealth)
    {
        _sectionHealth = newHealth;
        _currentPhase = 2;
        if(SpriteRenderer != null)
        SpriteRenderer.color = Color.white;
        _sectionDestroyed = false;

        _isPositive = true;
        ChangePositiveOrNegative(_isPositive);
    }
    //DELETE LATER
    public void TESTDESTROYSECTION()
    {
        _sectionHealth = 0f;
        _sectionDestroyed = true;
        OnSectionDestroyed?.Invoke(this);
    }
    public void IndicateDestroyed()
    {
        ChangePositiveOrNegative(_isPositive);
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
    public void MoveSection(Vector2 movePosition, float timeToMove)
    {
        if(_moveRoutine != null)
        {
            StopCoroutine(_moveRoutine);
            _moveRoutine = null;
        }

        _moveRoutine = StartCoroutine(MoveSectionEnumerator(movePosition,timeToMove));

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
    public void RotateAndShoot(float shootInterval,float rotationTime, float rotateAmount,bool clockwise = false,bool shoot = true)
    {
        _rotateRoutine = StartCoroutine(RotateOverTime(shootInterval,rotationTime,rotateAmount,clockwise,shoot));
    }
    public void RotateBetweenAnglesAndShoot(float shootInterval,float rotationTime,float minRotation,float maxRotation,bool startTowardsMax,
        bool inRotation,bool shoot = true)
    {
        _rotateRoutine = StartCoroutine(RotateBetweenAngles(shootInterval,rotationTime,minRotation,maxRotation,startTowardsMax,inRotation,shoot));
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
    private IEnumerator RotateOverTime(float shootInterval,float rotationTime,float rotationAmount,bool clockwise = false, bool shoot = true)
    {
        float bulletShot = -1f;

        float startAngle = transform.eulerAngles.z;
        float targetAngle = clockwise
            ? startAngle - rotationAmount
            : startAngle + rotationAmount;

        float elapsedTime = 0f;

        while (elapsedTime < rotationTime)
        {
            elapsedTime += Time.deltaTime;

            float t = Mathf.Clamp01(elapsedTime / rotationTime);

            float currentAngle = Mathf.Lerp(
                startAngle,
                targetAngle,
                t);

            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

            if (shoot && bulletShot < 0f)
            {
                ShootBullet(transform.up);
                bulletShot = shootInterval;
            }

            bulletShot -= Time.deltaTime;

            yield return null;
        }

        transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
    }
    private IEnumerator RotateBetweenAngles(float shootInterval,float rotationTime,float minRotation,float maxRotation,bool startTowardsMax,
        bool inRotation,bool shoot = true)
    {
        float bulletShot = -1f;

        float midRotation = (minRotation + maxRotation) * 0.5f;
        float amplitude = (maxRotation - minRotation) * 0.5f;
        float angularSpeed = 2f * Mathf.PI / rotationTime;

        float phase;

        if (inRotation)
        {
            //Resuming mid-swing: solve for the phase that matches where we currently are.
            float currentRotation = transform.eulerAngles.z;
            float normalized = Mathf.Clamp((currentRotation - midRotation) / amplitude, -1f, 1f);
            float asinValue = Mathf.Asin(normalized);

            //startTowardsMax tells us which half of the wave we're travelling through,
            //since Asin alone can't distinguish the two.
            phase = startTowardsMax ? asinValue : Mathf.PI - asinValue;
        }
        else
        {
            //minRotation sits at sin = -1 (phase = -pi/2), maxRotation sits at sin = 1 (phase = pi/2).
            phase = startTowardsMax ? -Mathf.PI * 0.5f : Mathf.PI * 0.5f;
        }

        while (true)
        {
            phase += angularSpeed * Time.deltaTime;

            //Keep phase bounded so it doesn't lose float precision over a long-running loop.
            if (phase > Mathf.PI * 2f)
            {
                phase -= Mathf.PI * 2f;
            }

            float currentAngle = midRotation + amplitude * Mathf.Sin(phase);
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);

            if (shoot && bulletShot < 0f)
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
    public void MoveSinWave(float shotSpeed,float bulletSpeed,float sectionSpeed,float movementDistance,bool negative = true)
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
            _sinRoutine = StartCoroutine(MoveSinWaveEnumerator(sectionSpeed,movementDistance,negative));
            ShootBullet(shotSpeed,Vector2.left,bulletSpeed);
        }      
    }
    private IEnumerator MoveSinWaveEnumerator(float sectionSpeed,float movementDistance,bool negative = true)
    {
        int negativeInt = 1;
        if(negative) negativeInt = -1;
        float timeTracker = 0f;
        while (true)
        {
            float movementOffset = Mathf.Sin(timeTracker * sectionSpeed) * movementDistance * negativeInt;

            transform.position = _startPosition + Vector2.up * movementOffset;


            timeTracker += Time.deltaTime;
            yield return null;
        }
    }
    public void MoveSinWave(float shotSpeed,float bulletSpeed,float sectionSpeed,Vector2 pointA,Vector2 pointB,Vector2 bulletDirection,bool negative = true)
    {
        if(_sinRoutine != null)
        {
            ShootBullet(shotSpeed,bulletDirection,bulletSpeed);
            StopCoroutine(_sinRoutine);
            _sinRoutine = null;
        }
        else
        {
            Vector2 middlePoint = (pointA + pointB) * 0.5f;
            Vector2 currentPoint = transform.position;

            _sinRoutine = StartCoroutine(MoveSinWaveEnumerator(sectionSpeed,pointA,pointB,middlePoint,currentPoint,negative));
            ShootBullet(shotSpeed,bulletDirection,bulletSpeed);
        }      
    }
    private IEnumerator MoveSinWaveEnumerator(float sectionSpeed,Vector2 pointA,Vector2 pointB,Vector2 middlePoint,Vector2 currentPoint,bool negative = true)
    {
        int negativeInt = 1;
        if(negative) negativeInt = -1;

        Vector2 direction = (pointB - middlePoint).normalized;
        float amplitude = Vector2.Distance(middlePoint,pointB);

        //Work out how far along the wave currentPoint already sits, so movement continues
        //smoothly from here instead of snapping back as if it started at middlePoint.
        float displacement = Vector2.Dot(currentPoint - middlePoint,direction);
        float normalizedDisplacement = amplitude > 0f
            ? Mathf.Clamp((displacement / amplitude) * negativeInt,-1f,1f)
            : 0f;

        float timeTracker = (amplitude > 0f && sectionSpeed != 0f)
            ? Mathf.Asin(normalizedDisplacement) / sectionSpeed
            : 0f;

        while (true)
        {
            float movementOffset = Mathf.Sin(timeTracker * sectionSpeed) * amplitude * negativeInt;

            transform.position = middlePoint + direction * movementOffset;

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
    public void Phase2ShootFourDirections(float shotSpeed, int bulletAmount, BulletDirection direction, float offset = 0f)
    {
        StartCoroutine(ShootBulletsFromDirection(shotSpeed,direction,bulletAmount,offset));
    }
    public IEnumerator ShootBulletsFromDirection(float shotSpeed, BulletDirection direction, int bulletAmount,float offset = 0f)
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
    public LaserSO GetLaserSO()
    {
        return _laser.GetLaserSO();
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

    public void SetHealth(float newHealth)
    {
        _sectionHealth = newHealth;
    }
}

public enum BulletDirection { Up, Down, Left, Right }
