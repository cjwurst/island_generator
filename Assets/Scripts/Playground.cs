using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playground : MonoBehaviour
{
    [SerializeField] float[] numbers;

    [Space]
    [SerializeField] float testNumber;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Test();
    }

    void Start()
    {
        Test();
    }

    void Test()
    {
        numbers = new float[6];
        var numberString = "";
        for (var i = 0; i < numbers.Length; i++)
        {
            numbers[i] = i + i * Mathf.Sqrt(Mathf.Abs(i)) * (2 * (i % 2) - 1);
            numberString += numbers[i].ToString() + " | ";
        }
        print(numberString);

        var tree = new KDTree<float>(new List<float>(numbers), (n) => new float[] { n }, 1);
        print($"tree: { tree }");
        print($"element nearest to { testNumber } (KDTree): { tree.GetNearestNeighbor(testNumber, out _) }");

        var leastDistance = float.PositiveInfinity;
        var nearestNeighbor = numbers[0];
        foreach (var neighbor in numbers)
        {
            var distance = Mathf.Abs(neighbor - testNumber);
            if (distance < leastDistance)
            {
                leastDistance = distance;
                nearestNeighbor = neighbor;
            }
        }
        print($"element nearest to { testNumber } (Linear): { nearestNeighbor }");
    }
}