using System;

namespace LSystemEngine
{
    [Serializable]
    public struct LRule
    {
        public char input;
        public WeightedRuleOutput[] outputs;
        public string output
        {
            get
            {
                float sum = 0;
                foreach (WeightedRuleOutput o in outputs) sum += o.weight;
                float roll = sum * UnityEngine.Random.value;
                float partialSum = 0;
                foreach (WeightedRuleOutput o in outputs)
                { 
                    partialSum += o.weight;
                    if (partialSum >= roll) return o.output;
                }
                return " Something has gone terribly wrong. ";
            }
        }
    }
}


