using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class Laser : MonoBehaviour
{
    [SerializeField] private LaserSO _laserSO;
    [SerializeField] private LineRenderer _laserLine;

    public event Action OnLaserFinished;
    const float LASERLENGTH = 50f;
    //Laser Settings
    private Vector2 _laserSize;
    private Vector2 _laserStartPosition;
    private Vector2 _laserMiddle;
    private Vector2 _laserEndPosition;
    private Transform _shootingTransform;
    private float _laserAngle;
    public void ShootLaser(Transform shootingTransform)
    {
        _laserSO.PlayerMask = LayerMask.GetMask("Player");
        _shootingTransform = shootingTransform;
        _laserSize = new(LASERLENGTH,_laserSO.LaserWidth);

        UpdateLaserPosition();

        StartCoroutine(InitialLaserShot());
    }
    public void StopLaser()
    {
        StopAllCoroutines();
        _laserLine.enabled = false;
    }
    private IEnumerator InitialLaserShot()
    {

        float initialWidth = _laserSO.LaserWidth / 10f;

        _laserLine.enabled = true;


        SetLaserPosition();


        _laserLine.startWidth = initialWidth;
        _laserLine.endWidth = initialWidth;

        float timeTracker = 0f;
        while(timeTracker < _laserSO.LaserShootDelay)
        {
            UpdateLaserPosition();
            timeTracker += Time.deltaTime;
            yield return null;
        }

        _laserLine.startWidth = _laserSO.LaserWidth;
        _laserLine.endWidth = _laserSO.LaserWidth;

        StartCoroutine(ContinuosLaserShot());

    }

    private void UpdateLaserPosition()
    {
        _laserStartPosition = _shootingTransform.position;
        _laserEndPosition =  _shootingTransform.position + _shootingTransform.up.normalized * LASERLENGTH;
        _laserMiddle = (_laserStartPosition + _laserEndPosition) * 0.5f;

        _laserAngle = Vector2.SignedAngle(Vector2.right,_shootingTransform.up);

        SetLaserPosition();
    }
    private IEnumerator ContinuosLaserShot()
    {
        float timeTracker = 0f;

        while(timeTracker < _laserSO.LaserLifetime)
        {
            Collider2D hit = Physics2D.OverlapBox(_laserMiddle,_laserSize,_laserAngle,_laserSO.PlayerMask);
            if(hit)
            {
                if(hit.TryGetComponent<PlayerHealthManager>(out PlayerHealthManager component))
                {
                    component.TakeDamage(_laserSO.LaserDamage);
                }
            }

            UpdateLaserPosition();

            timeTracker += Time.deltaTime;

            yield return null;
        }

        OnLaserFinished?.Invoke();
        _laserLine.enabled = false;
    }
    private void SetLaserPosition()
    {
        _laserLine.SetPosition(0,_laserStartPosition);
        _laserLine.SetPosition(1,_laserEndPosition);  
    }
    public void ChangeLaserColour(Color newColour)
    {
        _laserLine.startColor = newColour;
        _laserLine.endColor = newColour;
    }
    public void SetLaserSO(LaserSO newLaserSO)
    {
        _laserSO = newLaserSO;
    }
}
