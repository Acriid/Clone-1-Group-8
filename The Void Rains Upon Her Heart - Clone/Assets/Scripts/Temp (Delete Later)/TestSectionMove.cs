using UnityEngine;
using System;

public class TestSectionMove : MonoBehaviour
{
    public BossSection BossSection;

    void Awake()
    {
        
        BossSection.MoveSinWave();
    }
}
