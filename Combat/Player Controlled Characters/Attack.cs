// Inherits from Action, for player controlled character attacks.

[System.Serializable]
public class Attack : Action
{
    public enum Contact { direct, indirect }
    public Contact _contact;

    // how many times the attack hits the enemy
    public int _parts;

    // do this to a character's ATK to get an attack's final damage
    public enum Modifier { additive, multiplicative }
    public Modifier _modifier;

    // modifier value
    public int _mod;

    public Attack(string name, Side side, Target target, Contact contact, Trait trait = Trait.none, int cost = 0, int parts = 1, 
        Modifier modifier = Modifier.multiplicative, int mod = 1, bool has_status = false, Status status = null) : 
        base(name, side, target, trait, cost, has_status, status)
    {
        _contact = contact;
        _mod = mod;
        _parts = parts;
        _modifier = modifier;
    }

    public Attack(Attack attack) : base(attack)
    {
        _contact = attack._contact;
        _mod = attack._mod;
        _parts = attack._parts;
        _modifier = attack._modifier;
    }
}
