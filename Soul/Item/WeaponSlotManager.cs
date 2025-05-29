using UnityEngine;

public class WeaponSlotManager : MonoBehaviour
{
    public WeaponHolderSlot leftHandSlot;
    public WeaponHolderSlot rightHandSlot;

    DamageCollider leftHandDamageCollider;
    DamageCollider rightHandDamageCollider;

    public WeaponItem attackingWeapon;

    [SerializeField] QuickSlotsUI quickSlotsUI;

    PlayerStats playerStats;
    PlayerController playerController;

    public GameObject handPosition;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        playerController = GetComponent<PlayerController>();

        WeaponHolderSlot[] weaponHolderSlots = GetComponentsInChildren<WeaponHolderSlot>();
        foreach (WeaponHolderSlot weaponSlot in weaponHolderSlots)
        {
            if (weaponSlot.isLeftHandSlot)
            {
                leftHandSlot = weaponSlot;
            }
            else if (weaponSlot.isRightHandSlot)
            {
                rightHandSlot = weaponSlot;
            }
        }
    }

    public void LoadWeaponOnSlot(WeaponItem weaponItem, bool isLeft)
    {
        if (isLeft)
        {
            leftHandSlot.LoadWeaponModel(weaponItem);
            LoadLeftWeaponDamageCollider();
            quickSlotsUI.UpdateWeaponQuickSlotUI(true, weaponItem);
        }
        else
        {
            rightHandSlot.LoadWeaponModel(weaponItem);
            LoadRightWeaponDamageCollider();
            quickSlotsUI.UpdateWeaponQuickSlotUI(false, weaponItem);
            if (weaponItem.weaponType != WeaponType.none && weaponItem.weaponType != WeaponType.bow)
            {
                rightHandSlot.currentWeaponModel.GetComponentInChildren<DamageCollider>().root = gameObject;
            }
            else if (weaponItem.weaponType == WeaponType.bow)
            {
                rightHandSlot.currentWeaponModel.GetComponentInChildren<Bow>().root = gameObject;
            }
        }
    }

    private void LoadLeftWeaponDamageCollider()
    {
        leftHandDamageCollider = leftHandSlot.currentWeaponModel.GetComponentInChildren<DamageCollider>();
    }

    private void LoadRightWeaponDamageCollider()
    {
        rightHandDamageCollider = rightHandSlot.currentWeaponModel.GetComponentInChildren<DamageCollider>();
    }

    public void OpenRightDamageCollider()
    {
        rightHandDamageCollider.EnableDamageCollider();
    }

    public void OpenLeftDamageCollider()
    {
        leftHandDamageCollider.EnableDamageCollider();
    }

    public void CloseRightDamageCollider()
    {
        rightHandDamageCollider.DisableDamageCollider();
    }

    public void CloseLeftDamageCollider()
    {
        leftHandDamageCollider.DisableDamageCollider();
    }

    public void BowAttackStart()
    {
        rightHandSlot.currentWeaponModel.GetComponentInChildren<Bow>().AttackStart(handPosition, transform.forward);
    }
    
    public void BowAttackShoot()
    {
        rightHandSlot.currentWeaponModel.GetComponentInChildren<Bow>().Shoot(playerController._cinemachineTargetPitch);
    }

    public void DrainStaminaLightAttack()
    {
        playerStats.TakeStaminaDamage(Mathf.RoundToInt(attackingWeapon.baseStamina * attackingWeapon.lightAttackMultiplier));
    }

    public void DrainStaminaHeavyAttack()
    {
        playerStats.TakeStaminaDamage(Mathf.RoundToInt(attackingWeapon.baseStamina * attackingWeapon.heavyAttackMultiplier));
    }
}
