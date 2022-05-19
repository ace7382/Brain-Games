using System;

public class WeightedChanceParam
{
    public Action Func { get; }
    public double Ratio { get; }

    public WeightedChanceParam(Action func, double ratio)
    {
        Func = func;
        Ratio = ratio;
    }
}
