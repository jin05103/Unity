using UnityEngine;
using UnityEngine.UI;

public class HandEquipmentSlotUI : MonoBehaviour
{
    public Image icon;
    WeaponItem weapon;
    public Sprite sprite;

    public bool rightHandSlot01;
    public bool rightHandSlot02;
    public bool leftHandSlot01;
    public bool leftHandSlot02;

    public void AddItem(WeaponItem newWeapon)
    {
        weapon = newWeapon;
        icon.sprite = weapon.itemIcon;
        icon.enabled = true;
        gameObject.SetActive(true);
    }

    public void ClearItem()
    {
        weapon = null;
        icon.sprite = null;
        icon.enabled = false;
        gameObject.SetActive(false);
    }

    public void SetIcon(WeaponItem newWeapon)
    {
        weapon = newWeapon;
        if (newWeapon.weaponType == WeaponType.none)
        {
            icon.sprite = sprite;
        }
        else
        {
            icon.sprite = newWeapon.itemIcon;
        }
        // icon.sprite = newWeapon.itemIcon;
    }
}
