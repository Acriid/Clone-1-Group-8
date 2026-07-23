using UnityEngine;

[CreateAssetMenu(fileName = "Laser", menuName = "Laser/DefaultBullet")]
public class LaserSO : ScriptableObject
{
    public float LaserShootDelay = 1f;
    public float LaserDamage = 0f;
    public float LaserLifetime = 1f;
    public float LaserWidth = 5f;
}
