// Gun.cs
using UnityEngine;

public class Gun : MonoBehaviour, IWeapon
{
    public void Fire()
    {
        Debug.Log("🔫 총 발사!");
    }
}
