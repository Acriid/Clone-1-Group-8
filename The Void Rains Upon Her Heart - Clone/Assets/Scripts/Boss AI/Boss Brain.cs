using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BossBrain : MonoBehaviour
{
    [SerializeField] private List<BossSection> _sectionList = new(4);
    [Header("General Attacks")]
    [SerializeField] private float _attackDelay = 2f;
    [Header("SinWave Attack")]
    [SerializeField] private float _sinWaveX = 10f;
    [SerializeField] private float _sinWaveYOffset = 4f;
    [SerializeField] private float _sinWaveAttackTime = 6.5f;

    private List<bool> _sectionsFinishedMoving = new(4)
    {
        false,
        false,
        false,
        false,
    };

    private BossSection _bossSection1;
    private BossSection _bossSection2;
    private BossSection _bossSection3;
    private BossSection _bossSection4;

    void Start()
    {
        _bossSection1 = _sectionList[0];
        _bossSection2 = _sectionList[1];
        _bossSection3 = _sectionList[2];
        _bossSection4 = _sectionList[3];

    }




    public void BasicLaserAttack()
    {
        List<BossSection> section1 = new(2);
        HashSet<BossSection> hashSections = new(_sectionList);

        
    }
    public void StartSinAttack()
    {
        float yOffset = _sinWaveYOffset;
        Vector2 movePosition = new(_sinWaveX, 2f* yOffset);


        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.MoveSection(movePosition,1f);
            bossSection.OnFinishedMove += CheckIfCanSinAttack;
            movePosition.y -= yOffset;
        }
    }
    private void CheckIfCanSinAttack(BossSection bossSection)
    {
        bossSection.OnFinishedMove -= CheckIfCanSinAttack;
        _sectionsFinishedMoving[_sectionList.IndexOf(bossSection)] = true;

        //Check if all finished moving
        foreach(bool booleans in _sectionsFinishedMoving)
        {
            if(!booleans) return;
        }

        //Reset Movement
        for(int i = 0; i < _sectionsFinishedMoving.Count ; i++)
        {
            _sectionsFinishedMoving[i] = false;
        }

        StartCoroutine(SinWaveAttack(_attackDelay));
    }
    private IEnumerator SinWaveAttack(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.MoveSinWave();
        }
        yield return new WaitForSeconds(_sinWaveAttackTime);
        foreach(BossSection bossSection in _sectionList)
        {
            bossSection.MoveSinWave();
        }
    }


}
