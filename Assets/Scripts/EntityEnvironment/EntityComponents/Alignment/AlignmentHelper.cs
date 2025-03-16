using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AlignmentHelper
{
    static int[][] alignmentChart;

    static AlignmentHelper()
    {
        alignmentChart = new int[8][];
        //                                  Neutral     Homies      Monsters    Reserved1   Reserved2   Reserved3   Reserved4   Reserved5
        alignmentChart[0] = new int[1] {    0,                                                                                              };    // Neutral
        alignmentChart[0] = new int[2] {    0,          1,                                                                                  };    // Homies
        alignmentChart[0] = new int[3] {    0,          -1,         1,                                                                      };    // Monsters
        alignmentChart[0] = new int[4] {    0,          0,          0,          0,                                                          };    // Reserved1
        alignmentChart[0] = new int[5] {    0,          0,          0,          0,          0,                                              };    // Reserved2
        alignmentChart[0] = new int[6] {    0,          0,          0,          0,          0,          0,                                  };    // Reserved3
        alignmentChart[0] = new int[7] {    0,          0,          0,          0,          0,          0,          0,                      };    // Reserved4
        alignmentChart[0] = new int[8] {    0,          0,          0,          0,          0,          0,          0,          0           };    // Reserved5
    }

    public static Alignments GetAlignment(AlignmentFlags alignments1, AlignmentFlags alignments2)
    {
        var sum = 0;
        for(var i = 0; i < 8; i++)
        {
            var x = (AlignmentFlags)Math.Pow(2, i) & alignments1;
            for(var j = 0; j <= i; j++)
            {
                var y = (AlignmentFlags)Math.Pow(2, j) & alignments2; 
                if ((x & y) > 0)
                    sum += alignmentChart[i][j];
            }
        }

        if (sum > 0) return Alignments.Ally;
        else if (sum == 0) return Alignments.Neutral;
        else return Alignments.Enemy;
    }
}

public enum Alignments
{
    Ally,
    Enemy,
    Neutral
}

[Flags]
public enum AlignmentFlags
{
    None        = 0b_0000_0000,
    Neutral     = 0b_0000_0001,
    Homies      = 0b_0000_0010,
    Monsters    = 0b_0000_0100,
    Reserved1   = 0b_0000_1000,
    Reserved2   = 0b_0001_0000,
    Reserved3   = 0b_0010_0000,
    Reserved4   = 0b_0100_0000,
    Reserved5   = 0b_1000_0000
}
