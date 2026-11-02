using System;

[Serializable]
public class NpcIntRange
{
    public int min = 1;
    public int max = 1;

    public int Roll(Random random)
    {
        if (max < min)
            (min, max) = (max, min);

        if (min == max)
            return min;

        return random.Next(min, max + 1);
    }

    public static NpcIntRange Fixed(int value) => new NpcIntRange { min = value, max = value };
}
