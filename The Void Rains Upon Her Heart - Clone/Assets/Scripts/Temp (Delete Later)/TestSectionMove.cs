using UnityEngine;
using System;

public class TestSectionMove : MonoBehaviour
{
    public BossBrain BossBrain;
    public Laser TestLaser;

    void Awake()
    {
        BossBrain.StartSinAttack();
        TestLaser.ShootLaser(Vector2.zero,new(10f,10f));
    }
}
