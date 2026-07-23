using System;
using Unity.VisualScripting;
using UnityEngine;

public class BossSection : MonoBehaviour
{
    private float _sectionHealth = 400;
    private bool _sectionDestroyed = false;

    [SerializeField] private float _sectionSpeed = 10f;
    [SerializeField] private Rigidbody2D _sectionRigidBody;

    public event Action<float> OnBossDamage;
    public void Damage(float damage)
    {
        if(_sectionDestroyed)
        {
            damage *= 0.1f;
        }
        else
        {
            _sectionHealth -= damage;
            if(_sectionHealth <= 0)
            {
                _sectionDestroyed = true;
            }
        }

        OnBossDamage?.Invoke(damage);
        
    }

    private void DestroySection()
    {
        //TODO - implement the sprite change
    }


}
