using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private WeaponFactory factory;

    private IWeapon weapon;

    void Start()
    {
        weapon = factory.CreateWeapon(WeaponType.Gun); // 초기 무기 선택
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            weapon?.Fire();
        }
    }
}
