using UnityEngine;

public class PlayerAttacker : MonoBehaviour
{
    AnimationController animationController;
    WeaponSlotManager weaponSlotManager;
    public string lastAttack;

    private void Awake()
    {
        animationController = GetComponent<AnimationController>();
        weaponSlotManager = GetComponent<WeaponSlotManager>();
    }

    public void HandleWeaponCombo(WeaponItem weapon)
    {
        if (lastAttack == weapon.OH_Light_Attack1)
        {
            animationController.PlayTargetAnimation(weapon.OH_Light_Attack2, true);
            // animationController.SetAnim();
            // animationController.SetBool("DoComboAttack", true);
            lastAttack = weapon.OH_Light_Attack2;
        }
    }

    public int StaminaCost(WeaponItem weapon, string attackType)
    {
        if (attackType == "Light")
        {
            return Mathf.RoundToInt(weapon.baseStamina * weapon.lightAttackMultiplier);
        }
        else if (attackType == "Heavy")
        {
            return Mathf.RoundToInt(weapon.baseStamina * weapon.heavyAttackMultiplier);
        }
        return 0;
    }

    public void HandleLightAttack(WeaponItem weapon)
    {
        weaponSlotManager.attackingWeapon = weapon;
        animationController.PlayTargetAnimation(weapon.OH_Light_Attack1, true);
        lastAttack = weapon.OH_Light_Attack1;
    }

    public void HandleHeavyAttack(WeaponItem weapon)
    {
        weaponSlotManager.attackingWeapon = weapon;
        animationController.PlayTargetAnimation(weapon.OH_Heavy_Attack1, true);
        lastAttack = weapon.OH_Heavy_Attack1;
    }

    public void CanDoComboOn()
    {
        animationController.SetBool("CanDoCombo", true);
    }

    public void CanDoComboOff()
    {
        animationController.SetBool("CanDoCombo", false);
    }

    public void ResetInteracting()
    {
        animationController.SetBool("IsInteracting",false);
    }
}
