using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] AnimationController animationController;
    [SerializeField] UIManager uiManager;
    public WeaponSlotManager weaponSlotManager;

    public WeaponItem rightWeapon;
    public WeaponItem leftWeapon;

    public WeaponItem unarmedWeapon;

    public WeaponItem[] weaponInRightHandSlots = new WeaponItem[1];
    public WeaponItem[] weaponInLeftHandSlots = new WeaponItem[1];

    public int currentRightWeaponIndex = 0;
    public int currentLeftWeaponIndex = 0;

    public List<WeaponItem> weaponsInventory;

    public Save save;

    public bool isTwoHanded = false;

    private void Awake()
    {
        weaponSlotManager = GetComponentInChildren<WeaponSlotManager>();
    }

    private void Start()
    {
        // rightWeapon = weaponInRightHandSlots[currentRightWeaponIndex];
        // leftWeapon = weaponInLeftHandSlots[currentLeftWeaponIndex];

        // weaponSlotManager.LoadWeaponOnSlot(rightWeapon, false);
        // weaponSlotManager.LoadWeaponOnSlot(leftWeapon, true);
        // rightWeapon = unarmedWeapon;
        // leftWeapon = unarmedWeapon;

        InitializeInventory();
    }

    public void InitializeInventory()
    {
        if (save != null)
        {
            rightWeapon = save.rightWeapon;
            leftWeapon = save.leftWeapon;

            weaponInRightHandSlots = save.rightHandWeapons.ToArray();
            weaponInLeftHandSlots = save.leftHandWeapons.ToArray();

            currentRightWeaponIndex = save.currentRightWeaponIndex;
            currentLeftWeaponIndex = save.currentLeftWeaponIndex;

            isTwoHanded = save.isTwoHanded;

            if (currentRightWeaponIndex == -1)
            {
                weaponSlotManager.LoadWeaponOnSlot(unarmedWeapon, false);
            }
            else
            {
                weaponSlotManager.LoadWeaponOnSlot(weaponInRightHandSlots[currentRightWeaponIndex], false);
            }

            if (currentLeftWeaponIndex == -1)
            {
                weaponSlotManager.LoadWeaponOnSlot(unarmedWeapon, true);
            }
            else
            {
                weaponSlotManager.LoadWeaponOnSlot(weaponInLeftHandSlots[currentLeftWeaponIndex], true);
            }

            if (!isTwoHanded)
            {
                if (leftWeapon.weaponType == WeaponType.shield)
                {
                    animationController.ChangeWeaponLayer(rightWeapon.weaponType, false, true);
                }
                else
                {
                    animationController.ChangeWeaponLayer(rightWeapon.weaponType, false, false);
                }
            }
            else
            {
                animationController.ChangeWeaponLayer(rightWeapon.weaponType, true, false);
            }

            weaponsInventory = save.weaponsInventory;
        }
        else
        {
            rightWeapon = unarmedWeapon;
            leftWeapon = unarmedWeapon;

            weaponInRightHandSlots[0] = unarmedWeapon;
            weaponInLeftHandSlots[0] = unarmedWeapon;

            currentRightWeaponIndex = -1;
            currentLeftWeaponIndex = -1;

            weaponSlotManager.LoadWeaponOnSlot(unarmedWeapon, false);
            weaponSlotManager.LoadWeaponOnSlot(unarmedWeapon, true);
        }

        uiManager.UpdatePanel();
    }

    public void ChangeRightWeapon()
    {
        currentRightWeaponIndex = currentRightWeaponIndex + 1;

        if (currentRightWeaponIndex == 0 && weaponInRightHandSlots[0] != null)
        {
            rightWeapon = weaponInRightHandSlots[currentRightWeaponIndex];
            weaponSlotManager.LoadWeaponOnSlot(weaponInRightHandSlots[currentRightWeaponIndex], false);
        }
        else if (currentRightWeaponIndex == 0 && weaponInRightHandSlots[0] == null)
        {
            currentRightWeaponIndex = currentRightWeaponIndex + 1;
        }
        else if (currentRightWeaponIndex == 1 && weaponInRightHandSlots[1] != null)
        {
            rightWeapon = weaponInRightHandSlots[currentRightWeaponIndex];
            weaponSlotManager.LoadWeaponOnSlot(weaponInRightHandSlots[currentRightWeaponIndex], false);
        }
        else
        {
            currentRightWeaponIndex = currentRightWeaponIndex + 1;
        }

        if (currentRightWeaponIndex > weaponInRightHandSlots.Length - 1)
        {
            currentRightWeaponIndex = -1;
            rightWeapon = unarmedWeapon;
            weaponSlotManager.LoadWeaponOnSlot(unarmedWeapon, false);
        }
    }

    public void ChangeLeftWeapon()
    {
        currentLeftWeaponIndex = currentLeftWeaponIndex + 1;

        if (currentLeftWeaponIndex == 0 && weaponInLeftHandSlots[0] != null)
        {
            leftWeapon = weaponInLeftHandSlots[currentLeftWeaponIndex];
            weaponSlotManager.LoadWeaponOnSlot(weaponInLeftHandSlots[currentLeftWeaponIndex], true);
        }
        else if (currentLeftWeaponIndex == 0 && weaponInLeftHandSlots[0] == null)
        {
            currentLeftWeaponIndex = currentLeftWeaponIndex + 1;
        }
        else if (currentLeftWeaponIndex == 1 && weaponInLeftHandSlots[1] != null)
        {
            leftWeapon = weaponInLeftHandSlots[currentLeftWeaponIndex];
            weaponSlotManager.LoadWeaponOnSlot(weaponInLeftHandSlots[currentLeftWeaponIndex], true);
        }
        else
        {
            currentLeftWeaponIndex = currentLeftWeaponIndex + 1;
        }

        if (currentLeftWeaponIndex > weaponInLeftHandSlots.Length - 1)
        {
            currentLeftWeaponIndex = -1;
            leftWeapon = unarmedWeapon;
            weaponSlotManager.LoadWeaponOnSlot(unarmedWeapon, true);
        }
    }

    public WeaponType GetWeaponType(bool isLeft)
    {
        if (isLeft)
        {
            return leftWeapon.weaponType;
        }
        else
        {
            return rightWeapon.weaponType;
        }
    }
}
