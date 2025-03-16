using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damage
{
    public int Amount { get; set; }
    public readonly DamageType type;

    public Damage(int amount, DamageType _type)
    {
        Amount = amount;
        type = _type;
    }
}

public enum DamageType
{
    untyped,
    bludgeoning,
    piercing,
    slashing,
    seraphic,
    necrotic,
    fire,
    cold,
    lightning,
    acid,
    poison,
    psychic
}

public struct DamageProtection
{
    int m_armor;
    public int Armor
    {
        get { return m_armor; }
        set { m_armor = Mathf.Min(value, 0); }
    }

    int m_resistance;
    public int Resistance
    {
        get { return m_resistance; }
        set { m_resistance = value.Bound(-2, 4); }
    }

    public DamageProtection(int armor, int resistance)
    {
        m_armor = Mathf.Min(armor, 0);
        m_resistance = resistance.Bound(-2, 4);
    }

    public int Apply(int amount)
    {
        amount -= Mathf.CeilToInt(amount * Resistance / 2f);
        var armor = Mathf.Min(Armor, amount);
        return amount - armor;
    }
}
