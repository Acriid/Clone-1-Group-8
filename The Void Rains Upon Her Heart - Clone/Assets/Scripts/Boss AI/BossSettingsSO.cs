using UnityEngine;

[CreateAssetMenu(fileName = "BossSettings", menuName = "Boss/BossSettings")]
public class BossSettingsSO : ScriptableObject
{
    [Header("General")]
    //Level 1 is lvl 2 
    //Level 2 is lvl 9
    [Range(1,2)]
    public int BossLevel = 1;
    public float ArenaPaddingPercentageX = 96f/100f;
    public float ArenaPaddingPercentageY = 92f/100f;
    public float BossHealth = 1929;
    public float SectionHealthPhase1 = 349;
    public float SectionHealthPhase2 = 97;
    [Header("Rotation Speed")]
    public float SectionsParentRotationSpeed = 40f;
    public float BulletSectionRotationSpeed = 60f;
    [Header("General Attacks")]
    public float AttackDelay = 2f;
    public float TimeToMoveToAttackPosition = 2f;
    [Header("SinWave Attack")]
    public float SinWaveAttackTime = 6.5f;
    public float SinShotSpeed = 0.2f;
    public float SinWaveBulletSpeed = 10f;
    public float SinSectionSpeed = 1f;
    public float SinMovementDistance = 2f;
    [Header("SpinningBullet Attack")]
    public float SpinningBulletTime = 3f;
    public float SpinningBulletShootTime = 0.5f;
    public float SpinningBulletRotateSpeed = 40f;
    [Header("Phase 2")]
    public float Phase2Offset = 2f;
    public float TimeToMoveToPhase2 = 3f;
    [Header("FourBullet Attack")]
    public float FourBulletAttackWait = 2f;
    public float FourBulletAttackTime = 0.1f;
    public float FourBulletOffset = 0f;
    public int FourBulletBulletAmount = 6;
    public int FourBulletBulletRingAmount = 4;
    [Header("BulletSpreadAttack")]
    public float BulletSpreadAttackTime = 1.5f;
    public float BulletSpreadRotationDegrees = 360f/7f;
    public int BulletSpreadBulletAmount = 7;
    public int BulletSpreadBulletCircleAmount = 4;
}
