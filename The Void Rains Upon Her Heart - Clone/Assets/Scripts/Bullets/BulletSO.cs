using UnityEngine;

[CreateAssetMenu(fileName = "Bullet", menuName = "Bullet/DefaultBullet")]
public class BulletSO : ScriptableObject
{
    public float BulletSpeed = 0f;
    public float BulletDamage = 0f;
    public float BulletLifetime = 1f;
    public bool BouncyBullet = false;
    public bool BossBullet = false;
}
