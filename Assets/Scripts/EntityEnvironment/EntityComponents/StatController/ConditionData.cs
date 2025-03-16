using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCondition", menuName = "Entity Toolbox/Simple Condition")]
public class ConditionData : ScriptableObject
{
    [SerializeField, Min(1)] readonly int m_stackMax = 1;
    public int stackMax { get { return m_stackMax; } }

    [SerializeField, Min(1)] int baseDuration = 1;
    public int duration { get => baseDuration; }
    [SerializeField] bool baseIsPermanent = false;
    public bool isPermanent { get => baseIsPermanent; }

    [Space()]
    [SerializeField] int strengthSummand = 0;
    [SerializeField] int dexteritySummand = 0;
    [SerializeField] int intelligenceSummand = 0;
    [SerializeField] int wisdomSummand = 0;

    [SerializeField, Min(0)] int strengthMultiplier = 1;
    [SerializeField, Min(0)] int dexterityMultiplier = 1;
    [SerializeField, Min(0)] int intelligenceMultiplier = 1;
    [SerializeField, Min(0)] int wisdomMultiplier = 1;

    [SerializeField] int damageOverTime = 0;
    [SerializeField] int dotSummandPerTurn = 0;

    // returns a command that removes *this* from *stats*
    public Action Apply(StatController stats)
    {
        List<Action> removeCommands = new List<Action>();

        removeCommands.Add(stats.Strength.AddModifier((x) => x + strengthSummand));
        removeCommands.Add(stats.Dexterity.AddModifier((x) => x + dexteritySummand));
        removeCommands.Add(stats.Intelligence.AddModifier((x) => x + intelligenceSummand));
        removeCommands.Add(stats.Wisdom.AddModifier((x) => x + wisdomSummand));

        removeCommands.Add(stats.Strength.AddModifier((x) => x * strengthMultiplier, true));
        removeCommands.Add(stats.Dexterity.AddModifier((x) => x * dexterityMultiplier, true));
        removeCommands.Add(stats.Intelligence.AddModifier((x) => x * intelligenceMultiplier, true));
        removeCommands.Add(stats.Wisdom.AddModifier((x) => x * wisdomMultiplier, true));

        return () =>
        {
            foreach (Action command in removeCommands)
                command.Invoke();
        };
    }

    public void Step(StatController stats, int lifeTime)
    {
        stats.HP.Current -= damageOverTime + dotSummandPerTurn * lifeTime;
    }
}
