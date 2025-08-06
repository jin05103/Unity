using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Weapon Database")]
public class InventorySaveLoad : ScriptableObject
{
    public List<WeaponItem> weaponItems;

    public WeaponItem currentRightWeapon;
    public WeaponItem currentLeftWeapon;
}
