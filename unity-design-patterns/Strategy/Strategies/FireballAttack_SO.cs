using UnityEngine;

[CreateAssetMenu(menuName = "Strategy/FireballAttack")]
public class FireballAttack_SO : ScriptableObject, IAttackStrategy
{
    public void Attack(Transform origin)
    {
        Debug.Log("Fireball Attack! (SO 기반)");
        // 예: 파이어볼 이펙트 생성
    }
}
