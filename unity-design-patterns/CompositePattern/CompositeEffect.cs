using System.Collections.Generic;
using UnityEngine;

public class CompositeEffect : IAttackEffect
{
    private List<IAttackEffect> children = new();

    public void AddEffect(IAttackEffect effect)
    {
        children.Add(effect);
    }

    public void RemoveEffect(IAttackEffect effect)
    {
        children.Remove(effect);
    }

    public void Apply()
    {
        Debug.Log("복합 효과 실행 시작");
        foreach (var effect in children)
        {
            effect.Apply();
        }
    }
}
