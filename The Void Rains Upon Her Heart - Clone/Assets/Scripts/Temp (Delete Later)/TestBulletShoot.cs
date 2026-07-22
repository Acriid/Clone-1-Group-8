using UnityEngine;

public class TestBulletShoot : MonoBehaviour
{
    public BulletManager BulletManager;
    void Start()
    {
        BulletManager.ShootBullet(new Vector2(-8.36f,0f),Vector2.right);
    }
}
