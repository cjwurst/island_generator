using System;

namespace LSystemEngine
{
    [Serializable]
    public struct DrawInstruction
    {
        public enum Instruction
        {
            Forward,
            TurnLeft,
            TurnRight,
            HalfAngle,
            Save,
            Load,
            ChangeColor
        }
        public char input;
        public WeightedDrawOutput[] outputs;

        public Instruction output
        {
            get
            {
                float sum = 0;
                foreach (WeightedDrawOutput o in outputs) sum += o.weight;
                float roll = sum * UnityEngine.Random.value;
                float partialSum = 0;
                foreach (WeightedDrawOutput o in outputs)
                {
                    partialSum += o.weight;
                    if (partialSum >= roll) return o.output;
                }
                return Instruction.Forward;
            }
        }
    }
}

