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
}