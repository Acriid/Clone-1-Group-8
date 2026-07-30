using UnityEngine;

[CreateAssetMenu(fileName = "BossSettingsLVL9", menuName = "Boss/BossSettingsLVL9")]
public class BossSettingsLVL9SO : ScriptableObject
{
    //3.25 bullets per second
    [Header("SpinningLaserAttack")]
    public float SpinningLaserRotationSpeed = 4f;
    public float SpinningLaserBackAndForthSpeed = 10f;
}
