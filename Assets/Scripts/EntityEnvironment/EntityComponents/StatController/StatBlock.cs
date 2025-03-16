using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStatBlock", menuName = "Entity Toolbox/Stat Block")]
public class StatBlock : ScriptableObject
{
    static System.Random rng = new System.Random();

    [SerializeField, Min(0)] int baseStr = 5;
    [SerializeField, Min(0)] int strDeviation = 0;
    public Stat Strength { get { return MakeStat(baseStr, strDeviation); } }

    [SerializeField, Min(0)] int baseDex = 5;
    [SerializeField, Min(0)] int dexDeviation = 0;
    public Stat Dexterity { get { return MakeStat(baseDex, dexDeviation); } }

    [SerializeField, Min(0)] int baseInt = 5;
    [SerializeField, Min(0)] int intDeviation = 0;
    public Stat Intelligence { get { return MakeStat(baseInt, intDeviation); } }

    [SerializeField, Min(0)] int baseWis = 5;
    [SerializeField, Min(0)] int wisDeviation = 0;
    public Stat Wisdom { get { return MakeStat(baseWis, wisDeviation); } }

    [Space()]
    [SerializeField, Min(0)] int baseHP = 16;
    [SerializeField, Min(0)] int hpDeviation = 0;
    public Exhaustible HP { get { return (Exhaustible)MakeStat(baseHP, hpDeviation, true); } }

    [SerializeField, Min(0)] int baseAP = 4;
    [SerializeField, Min(0)] int apDeviation = 0;
    public Exhaustible AP { get { return (Exhaustible)MakeStat(baseAP, apDeviation, true); } }

    [SerializeField] List<ConditionData> conditionImmunities = new List<ConditionData>();
    public ConditionData[] ConditionImmunitiesArray { get { return conditionImmunities.ToArray(); } }

    Stat MakeStat(int baseValue, int maxDeviation, bool isExhaustible = false)
    {
        int deviation = rng.Next(-maxDeviation, maxDeviation + 1);
        baseValue += deviation;
        if (isExhaustible) return new Exhaustible(baseValue, baseValue);
        return new Stat(baseValue + deviation);
    }
}
