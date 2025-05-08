using UnityEngine;

public class Player : MonoBehaviour
{
    void Start()
    {
        // 단일 효과
        IAttackEffect bleed = new BleedEffect();
        IAttackEffect stun = new StunEffect();

        // 복합 효과 생성
        CompositeEffect combo = new CompositeEffect();
        combo.AddEffect(bleed);
        combo.AddEffect(stun);

        // 복합 효과 적용
        combo.Apply();
    }
}
