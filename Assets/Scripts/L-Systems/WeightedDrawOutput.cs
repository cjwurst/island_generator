using System;
using UnityEngine;

namespace LSystemEngine
{
    [Serializable]
    public struct WeightedDrawOutput
    {
        [SerializeField, Min(1f)]
        float baseWeight;
        public float weight { get { return baseWeight <= 0 ? 1 : baseWeight; } }
        public DrawInstruction.Instruction output;
        public float parameter;
    }
}
