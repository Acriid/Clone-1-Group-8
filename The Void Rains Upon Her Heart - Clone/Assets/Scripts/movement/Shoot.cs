 using UnityEngine;
using UnityEngine.InputSystem;

public class Shoot : MonoBehaviour
{
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 0.15f;
    
    private float lastShotTime;

    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && Time.time > lastShotTime + fireRate)
        {
            lastShotTime = Time.time;
            Instantiate(bulletPrefab, shootPoint.position, transform.rotation);
        }
    }
}
