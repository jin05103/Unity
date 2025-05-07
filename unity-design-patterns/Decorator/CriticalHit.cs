public class CriticalHit : DamageDecorator
{
    public CriticalHit(IDamage baseDamage) : base(baseDamage) { }

    public override int GetDamage() => baseDamage.GetDamage() + 5;
    public override string GetDescription() => baseDamage.GetDescription() + " + 치명타";
}
