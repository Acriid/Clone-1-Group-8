using Unity.VisualScripting;
using UnityEngine;

public class BossSectionDamage : MonoBehaviour
{
    [SerializeField] private float _collisionDamage = 4f;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.CompareTag("Player"))
        {
            if(collision.collider.TryGetComponent<PlayerHealthManager>(out PlayerHealthManager component))
            {
                component.TakeDamage(_collisionDamage);
            }
        }
    }
}
