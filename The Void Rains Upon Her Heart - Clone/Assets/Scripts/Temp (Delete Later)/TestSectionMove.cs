using UnityEngine;
using System;

public class TestSectionMove : MonoBehaviour
{
    public BossBrain BossBrain;

    void Awake()
    {
        BossBrain.StartSinAttack();
        
    }
}
