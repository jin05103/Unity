using UnityEngine;

public class Player : MonoBehaviour
{
    void Start()
    {
        IDamage attack = new BasicAttack();
        attack = new CriticalHit(attack);  // 치명타 추가
        attack = new FireEnchant(attack);  // 화염 속성 추가

        Debug.Log($"총 데미지: {attack.GetDamage()}"); // 10 + 5 + 3 = 18
        Debug.Log($"설명: {attack.GetDescription()}"); // "기본 공격 + 치명타 + 화염"
    }
}
