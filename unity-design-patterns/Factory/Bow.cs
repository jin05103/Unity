// Bow.cs
using UnityEngine;

public class Bow : MonoBehaviour, IWeapon
{
    public void Fire()
    {
        Debug.Log("🏹 화살 발사!");
    }
}
