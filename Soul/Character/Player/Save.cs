using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Save", menuName = "ScriptableObjects/Save", order = 1)]
public class Save : ScriptableObject
{
    public WeaponItem rightWeapon;
    public WeaponItem leftWeapon;

    public List<WeaponItem> rightHandWeapons;
    public List<WeaponItem> leftHandWeapons;

    public List<WeaponItem> weaponsInventory;

    public int currentRightWeaponIndex = 0;
    public int currentLeftWeaponIndex = 0;

    public bool isTwoHanded = false;
}
