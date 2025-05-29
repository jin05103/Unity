using UnityEngine;

public class EnemyWeaponSlotManager : MonoBehaviour
{
    public EnemyManager enemyManager;
    public WeaponItem rightWeapon;
    public WeaponItem leftWeapon;

    WeaponHolderSlot leftHandSlot;
    WeaponHolderSlot rightHandSlot;

    DamageCollider leftHandDamageCollider;
    DamageCollider rightHandDamageCollider;
    public GameObject handPosition;

    public bool shootSuccess = false;

    private void Awake()
    {
        enemyManager = GetComponent<EnemyManager>();
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
                rightHandDamageCollider = rightHandSlot.GetComponentInChildren<DamageCollider>();
            }
        }
    }

    public void LoadWeaponOnSlot(WeaponItem weaponItem, bool isLeft)
    {
        if (isLeft)
        {
            leftHandSlot.LoadWeaponModel(weaponItem);
            LoadWeaponsDamageCollider(true);
        }
        else
        {
            rightHandSlot.LoadWeaponModel(weaponItem);
            LoadWeaponsDamageCollider(false);
        }
    }

    public void LoadWeaponsDamageCollider(bool isLeft)
    {
        if (isLeft)
        {
            leftHandDamageCollider = leftHandSlot.currentWeaponModel.GetComponentInChildren<DamageCollider>();
        }
        else
        {
            rightHandDamageCollider = rightHandSlot.currentWeaponModel.GetComponentInChildren<DamageCollider>();
        }
    }

    public void OpenDamageCollider()
    {
        rightHandDamageCollider.EnableDamageCollider();
    }

    public void CloseDamageCollider()
    {
        rightHandDamageCollider.DisableDamageCollider();
    }

    public void BowAttackStart()
    {
        shootSuccess = false;
        leftHandSlot.currentWeaponModel.GetComponentInChildren<Bow>().AttackStart(handPosition, transform.forward);
    }

    public void BowAttackShoot()
    {
        shootSuccess = true;
        leftHandSlot.currentWeaponModel.GetComponentInChildren<Bow>().Shoot(enemyManager.currentTarget.transform.position.y);
    }

    public void BowAttackStop()
    {
        if (!shootSuccess)
        {
            leftHandSlot.currentWeaponModel.GetComponentInChildren<Bow>().AttackStop();
        }
    }
}
