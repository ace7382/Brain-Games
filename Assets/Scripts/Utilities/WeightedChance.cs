using System.Linq;

/*
 * Use example:
 
    WeightedChance rando = new WeightedChance(
        new WeightedChanceParam(() => SetRandomArrowRotation(0), 25),   This choice has a 25/145 chance of running
        new WeightedChanceParam(() => SetRandomArrowRotation(1), 10),   This choice has a 10/145 chance of running
        new WeightedChanceParam(() => SetRandomArrowRotation(2), 20),   This choice has a 20/145 chance of running
        new WeightedChanceParam(() => SetRandomArrowRotation(3), 100)   This choice has a 100/145 chance of running
        );

    rando.Execute(); <-- call this to run the weighted chance once it's set up
*/

public class WeightedChance
{
    public  WeightedChanceParam[]   Parameters { get; }
    private System.Random           r;

    public double RatioSum
    {
        get { return Parameters.Sum(p => p.Ratio); }
    }

    public WeightedChance(params WeightedChanceParam[] parameters)
    {
        Parameters = parameters;
        r = new System.Random();
    }

    public void Execute()
    {
        double numericValue = r.NextDouble() * RatioSum;

        foreach (var parameter in Parameters)
        {
            numericValue -= parameter.Ratio;

            if (!(numericValue <= 0))
                continue;

            parameter.Func();
            return;
        }
    }
}
