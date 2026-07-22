using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    //Put the BulletManager on the GameObject that shoots the bullets
    //Create a BulletSO (Right Click -> Create -> Bullet -> DefaultBullet)
    //For fire rate code that in the script that calls BulletManager
    [SerializeField] private GameObject _bullet;
    [SerializeField] private int _bulletPoolSize = 0;

    private GenericPool<Bullet> _bulletPool;
    private List<Bullet> _activeBullets;
    void Awake()
    {
        _bulletPool = PoolManager.Instance.GetPool<Bullet>(_bullet, _bulletPoolSize);
        if(_bulletPool == null)
        {
            Debug.LogError("Failed to load bullet pool.");
        }
        _activeBullets = new(_bulletPoolSize);
    }


    /// <summary>
    /// Used to shoot the bullet.
    /// </summary>
    /// <param name="spawnPoint">The place the bullet originally appears</param>
    /// <param name="bulletDirection">The direction the bullet is getting shot in</param>
    public void ShootBullet(Vector2 spawnPoint, Vector2 bulletDirection)
    {
        Bullet instance = _bulletPool.Get();
        _activeBullets.Add(instance);

        //Subscribe event
        instance.OnBulletRemoved += ReturnBullet;
        instance.Shoot(spawnPoint,bulletDirection);

        Debug.Log("Shot Bullet");
    }

    //Removes bullet after it has hit something or reached the end of its lifetime
    private void ReturnBullet(Bullet bulletToReturn)
    {
        //UnSubscribe event
        bulletToReturn.OnBulletRemoved -= ReturnBullet;
        
        _activeBullets.Remove(bulletToReturn);
        _bulletPool.Return(bulletToReturn);
    }
}
