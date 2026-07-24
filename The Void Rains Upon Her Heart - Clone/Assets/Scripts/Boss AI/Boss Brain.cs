using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBrain : MonoBehaviour
{
    [SerializeField] private List<BossSection> _sectionList = new(4);

    private BossSection _bossSection1;
    private BossSection _bossSection2;
    private BossSection _bossSection3;
    private BossSection _bossSection4;

    void Awake()
    {
        _bossSection1 = _sectionList[0];
        _bossSection2 = _sectionList[1];
        _bossSection3 = _sectionList[2];
        _bossSection4 = _sectionList[3];
    }

    private IEnumerator SinWaveAttack()
    {
        yield return new WaitForSeconds(100f);
    }
}
