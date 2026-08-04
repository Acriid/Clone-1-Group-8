using UnityEngine;

public class PanicHitbox : MonoBehaviour
{
    [SerializeField] private float _damage = 270f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        BossSection section = other.GetComponent<BossSection>();

        if (section != null)
        {
            section.Damage(_damage);
        }

      
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.gameObject.layer.Equals(LayerMask.NameToLayer("Bullet")))
            return;

        Bullet bullet = other.GetComponent<Bullet>();

        if (bullet != null && bullet.IsBossBullet)
        {
            bullet.Despawn();
        }
    }
}