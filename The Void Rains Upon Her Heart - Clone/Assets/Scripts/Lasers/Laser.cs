using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField] private LaserSO _laserSO;
    [SerializeField] private LineRenderer _laserLine;
    public void ShootLaser(Vector2 startPosition, Vector2 endPosition)
    {
        _laserLine.enabled = true;

        _laserLine.SetPosition(0,startPosition);
        _laserLine.SetPosition(1,endPosition);

        _laserLine.startWidth = 0.25f;
        _laserLine.endWidth = 0.25f;

        StartCoroutine(ShootLaserEnumerator());
    }

    private IEnumerator ShootLaserEnumerator()
    {

        yield return new WaitForSeconds(_laserSO.LaserShootDelay);

        _laserLine.startWidth = _laserSO.LaserWidth;
        _laserLine.endWidth = _laserSO.LaserWidth;

        StartCoroutine(ContinuosLaserShot());
    }

    private IEnumerator ContinuosLaserShot()
    {
        float timeTracker = 0f;
        while(timeTracker < _laserSO.LaserLifetime)
        {
            timeTracker += Time.deltaTime;
            yield return null;
        }
        _laserLine.enabled = false;
    }
}
