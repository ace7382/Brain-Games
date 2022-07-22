using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Formulas
{
    public static int GetNextLevelEXP(Helpful.StatGrowthRates rate, int currentLevel)
    {
        //TODO: Request modifiers
        //TODO: Make these better lol

        if (currentLevel == 0)
            currentLevel = 1;

        switch(rate)
        {
            case Helpful.StatGrowthRates.NonExistent:
                return currentLevel * 500;
            case Helpful.StatGrowthRates.Very_Slow:
                return currentLevel * 50;
            case Helpful.StatGrowthRates.Slow:
                return currentLevel * 40;
            case Helpful.StatGrowthRates.Standard:
                return currentLevel * 30;
            case Helpful.StatGrowthRates.Fast:
                return currentLevel * 20;
            case Helpful.StatGrowthRates.Very_Fast:
                return currentLevel * 10;
            case Helpful.StatGrowthRates.YEET:
                return currentLevel * 5;
            default:
                Debug.Log("Trying to get next level with an unknown Growth Rate");
                return -9999;
        }
    }
}
