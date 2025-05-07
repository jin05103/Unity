public abstract class DamageDecorator : IDamage
{
    protected IDamage baseDamage;

    public DamageDecorator(IDamage baseDamage)
    {
        this.baseDamage = baseDamage;
    }

    public virtual int GetDamage() => baseDamage.GetDamage();
    public virtual string GetDescription() => baseDamage.GetDescription();
}
