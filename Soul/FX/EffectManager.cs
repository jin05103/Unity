using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public WeaponFX rightWeaponFX;
    public WeaponFX leftWeaponFX;

    public void PlayWeaponFx(bool isLeft)
    {
        if (isLeft)
        {
            leftWeaponFX.PlayWeaponFX();
        }
        else
        {
            rightWeaponFX.PlayWeaponFX();
        }
    }
}
