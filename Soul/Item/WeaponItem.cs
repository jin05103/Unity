using UnityEngine;

public enum WeaponType
{
    sword, heavySword, none, shield, bow
}

[CreateAssetMenu(menuName = "Item/Weapon Item")]
public class WeaponItem : Item
{
    public GameObject modelPrefab;
    public bool isUnarmed;
    public WeaponType weaponType;
    public float guardRate;
    public int strengthModifier;
    public int agilityModifier;

    [Header("One Handed Attack Animations")]
    public string OH_Light_Attack1;
    public string OH_Light_Attack2;
    public string OH_Heavy_Attack1;

    [Header("Stamina Costs")]
    public int baseStamina;
    public float lightAttackMultiplier;
    public float heavyAttackMultiplier;

    [Header("Guard Stamina Cost")]
    public int baseStamina2;
    public float lightAttackMultiplier2;
    public float heavyAttackMultiplier2;
}
