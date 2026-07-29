using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class Bullet : MonoBehaviour
{
    //The BulletSO and RigidBody needs to be set in the unity engine.
    [SerializeField] private BulletSO _bulletSO = null;
    [SerializeField] private Rigidbody2D _bulletRigidBody = null;

    private Coroutine _despawnRoutine = null;
    //Event that needs to get called to despawn the bullet
    public event Action<Bullet> OnBulletRemoved;
    const int BULLETSPEEDOFFSET = 100;
    private WaitForSeconds _bulletDespawnWaitTime;
    void Awake()
    {
        _bulletDespawnWaitTime = new WaitForSeconds(_bulletSO.BulletLifetime);
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        //If your code needs the bullet to despawn after it hits something 
        // call OnBulletRemoved?.Invoke(this);
        // and StopDespawnRoutine();
        if(_bulletSO.BouncyBullet && collision.collider.CompareTag("MapEdge"))
        {
            //TODO - Code the bullet bounce
        }
        else if (collision.collider.CompareTag("MapEdge"))
        {
            StopDespawnRoutine();
            OnBulletRemoved?.Invoke(this);
        }
        else if(!_bulletSO.BossBullet && collision.collider.CompareTag("Boss"))
        {
            if(collision.collider.TryGetComponent<BossSection>(out BossSection component))
            {
                component.Damage(_bulletSO.BulletDamage);
                OnBulletRemoved?.Invoke(this);
                StopDespawnRoutine();
            }
        }
        else if(collision.collider.CompareTag("Player"))
        {
            if(collision.collider.TryGetComponent<PlayerHealthManager>(out PlayerHealthManager component))
            {
                component.TakeDamage(_bulletSO.BulletDamage);
                OnBulletRemoved?.Invoke(this);
                StopDespawnRoutine();
            }
        }

    }
    public void Shoot(Vector2 spawnPoint, Vector2 bulletDirection)
    {
        if(_bulletSO == null)
        {
            Debug.LogError("Bullet ScriptableObject is not set");
            return;
        }
        if(_bulletRigidBody == null)
        {
            Debug.LogError("Bullet RigidBody is not set");
            return;           
        }
        
        if(_despawnRoutine != null)
        {
            Debug.LogWarning("Attempting to shoot a bullet that was already shot");
            return;
        }

        _despawnRoutine = StartCoroutine(DespawnBullet());

        //Set position and rotation
        gameObject.transform.position = spawnPoint;
        gameObject.transform.right = bulletDirection;

        _bulletRigidBody.AddForce(gameObject.transform.right * _bulletSO.BulletSpeed * BULLETSPEEDOFFSET);

    }
    public void Shoot(Vector2 spawnPoint, Vector2 bulletDirection,float speedToUse)
    {
        if(_bulletSO == null)
        {
            Debug.LogError("Bullet ScriptableObject is not set");
            return;
        }
        if(_bulletRigidBody == null)
        {
            Debug.LogError("Bullet RigidBody is not set");
            return;           
        }
        
        if(_despawnRoutine != null)
        {
            Debug.LogWarning("Attempting to shoot a bullet that was already shot");
            return;
        }

        _despawnRoutine = StartCoroutine(DespawnBullet());

        //Set position and rotation
        gameObject.transform.position = spawnPoint;
        gameObject.transform.right = bulletDirection;

        _bulletRigidBody.AddForce(gameObject.transform.right * speedToUse * BULLETSPEEDOFFSET);

    }
    private IEnumerator DespawnBullet()
    {
        yield return _bulletDespawnWaitTime;
        _despawnRoutine = null;
        OnBulletRemoved?.Invoke(this);
    }

    private void StopDespawnRoutine()
    {
        if(_despawnRoutine == null)
        {
            Debug.LogWarning("StopDespawnRoutine called when _despawnRoutine is null");
            return;
        }
        StopCoroutine(_despawnRoutine);
        _despawnRoutine = null;
    }
}
