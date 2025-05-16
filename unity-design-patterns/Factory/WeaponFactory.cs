using UnityEngine;

public enum WeaponType { Gun, Bow }

public class WeaponFactory : MonoBehaviour
{
    [SerializeField] private GameObject gunPrefab;
    [SerializeField] private GameObject bowPrefab;

    public IWeapon CreateWeapon(WeaponType type)
    {
        switch (type)
        {
            case WeaponType.Gun:
                return Instantiate(gunPrefab).GetComponent<IWeapon>();
            case WeaponType.Bow:
                return Instantiate(bowPrefab).GetComponent<IWeapon>();
            default:
                Debug.LogWarning("Unknown weapon type");
                return null;
        }
    }
}
