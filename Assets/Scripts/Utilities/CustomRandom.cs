using System;

public class CustomRandom : Random
{

    public CustomRandom(int seed) : base(seed)
    {

    }
    public float NextFloat(float a, float b)
    {
        var sample = Sample();
        return a + (b - a) * (float)sample;
    }
    public float NextFloat()
    {

        return (float)NextDouble();
    }
}
