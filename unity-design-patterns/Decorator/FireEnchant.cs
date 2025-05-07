public class FireEnchant : DamageDecorator
{
    public FireEnchant(IDamage baseDamage) : base(baseDamage) { }

    public override int GetDamage() => baseDamage.GetDamage() + 3;
    public override string GetDescription() => baseDamage.GetDescription() + " + 화염";
}
