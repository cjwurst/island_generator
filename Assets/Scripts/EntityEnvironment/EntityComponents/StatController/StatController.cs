using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class StatController : EntityComponent
{
    [SerializeField] StatBlock statBlock;

    [SerializeField] Exhaustible m_hp;
    public Exhaustible HP { get => m_hp; private set => m_hp = value; }
    [SerializeField] Exhaustible m_ap;
    public Exhaustible AP { get => m_ap; private set => m_ap = value; }

    public Stat Strength { get; private set; }
    public Stat Dexterity { get; private set; }
    public Stat Intelligence { get; private set; }
    public Stat Wisdom { get; private set; }

    Dictionary<StatType, Stat> stats = new Dictionary<StatType, Stat>();

    readonly List<ConditionData> conditionImmunities = new List<ConditionData>();
    readonly Dictionary<ConditionData, List<(int lifeTime, Action removeCommand)>> conditions = 
        new Dictionary<ConditionData, List<(int lifeTime, Action removeCommand)>>();

    Dictionary<DamageType, DamageProtection> protections = new Dictionary<DamageType, DamageProtection>();

    void Start()
    {
        HP = statBlock.HP;
        AP = statBlock.AP;

        Strength = statBlock.Strength;
        Dexterity = statBlock.Dexterity;
        Intelligence = statBlock.Intelligence;
        Wisdom = statBlock.Wisdom;

        stats.Add(StatType.HP, HP);
        stats.Add(StatType.AP, AP);
        stats.Add(StatType.Strength, Strength);
        stats.Add(StatType.Dexterity, Dexterity);
        stats.Add(StatType.Intelligence, Intelligence);
        stats.Add(StatType.Wisdom, Wisdom);

        conditionImmunities.AddRange(statBlock.ConditionImmunitiesArray);
    }
    protected override EventPriorityData InitPriorityData()
    {
        return new EventPriorityData
        (
            ("OnEntityAttacked", float.PositiveInfinity)
        );
    }

    protected override void OnEntityActed(EntityActedArgs args, List<IInvertible> commands)
    {
        if (args.entity != gameObject) return;
        commands.Add(new ExhaustCommand(args.APCost, AP));
    }

    protected override void OnEntityAttacked(EntityAttackedArgs args, List<IInvertible> commands)
    {
        if (!args.TargetCells.Contains(Position)) return;

        var amount = args.Damage.Amount;
        if (args.Damage.type != DamageType.untyped)
        {
            protections.TryGetValue(args.Damage.type, out var protection);
            amount = protection.Apply(amount);
        }
        args.damageDealt.Add(gameObject, amount);                                        // note that the amount is added before it is bounded by current hp
        amount = amount.Bound(HP.Current - HP.Total, HP.Current);

        if (amount == 0 || args.isTest) return;
        commands.Add(new ExhaustCommand(amount, HP));
    }

    protected override void OnStatsRequested(StatsRequestedArgs args, List<IInvertible> commands)
    {
        if (!args.stats.Keys.ToArray().Contains(gameObject)) return;

        var stat = stats[args.requestedStatType];
        if (stat is Exhaustible) args.stats[gameObject] = ((Exhaustible)stat).Current;
        else args.stats[gameObject] = stat.Total;
    }

    protected override void OnRoundPassed(List<IInvertible> commands)
    {
        var apRegen = AP.Total;
        apRegen = apRegen.Bound(0, AP.Total - AP.Current);
        commands.Add(new ExhaustCommand(-apRegen, AP));
    }

    bool TryApplyCondition(ConditionData condition)
    {
        if (conditionImmunities.Contains(condition)) return false;

        if (!conditions.TryGetValue(condition, out var lifeTimes))
            lifeTimes = new List<(int lifeTime, Action removeCommand)>();
        conditions.Add(condition, lifeTimes);
        if (lifeTimes.Count >= condition.stackMax) return false;

        lifeTimes.Add((0, condition.Apply(this)));
        return true;
    }

    class ExhaustCommand : IInvertible
    {
        int amount;
        Exhaustible stat;

        public ExhaustCommand(int _amount, Exhaustible _stat)
        {
            amount = _amount;
            stat = _stat;
        }

        public void Do()
        {
            stat.Current -= amount;
        }

        public void Undo()
        {
            stat.Current += amount;
        }
    }
}

public enum StatType
{
    HP,
    AP,
    Strength,
    Dexterity, 
    Intelligence,
    Wisdom
}
