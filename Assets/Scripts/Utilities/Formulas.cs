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

    public static int MultiplyIntByPercentAndTruncate(int i, float percent)
    {
        percent *= 100.0f;

        //Debug.Log(string.Format("int: {0}, float: {1}, int * float: {2}, truncated: {3}",
        //    i.ToString()
        //    , percent.ToString()
        //    , (i * percent).ToString()
        //    , ((int)(i * percent)).ToString()));

        return (int)((i * percent)/100f);
    }

    public static int GetDifficulty(Unit player, Unit enemy, Helpful.BattleGameTypes gameType, float gamePerformanceModifierValue)
    {
        int ret         = enemy.BaseDifficulty;
        int min, max    = 0;

        int levelDifference = player.GetStat(Helpful.StatTypes.Level) - enemy.GetStat(Helpful.StatTypes.Level);

        if (Mathf.Abs(levelDifference) >= 350)
        {
            ret += (int)(Mathf.Sign(levelDifference) * 250f);
        }
        else if (Mathf.Abs(levelDifference) >= 200)
        {
            ret += (int)(Mathf.Sign(levelDifference) * 200f);
        }
        else if (Mathf.Abs(levelDifference) >= 125)
        {
            ret += (int)(Mathf.Sign(levelDifference) * 150f);
        }
        else if (Mathf.Abs(levelDifference) >= 75)
        {
            ret += (int)(Mathf.Sign(levelDifference) * 100f);
        }
        else if (Mathf.Abs(levelDifference) >= 25)
        {
            ret += (int)(Mathf.Sign(levelDifference) * 50f);
        }

        ret = Mathf.Clamp(ret, 0, 1000);

        if (ret > 900) //Impossible
        {
            //Whole range of difficulty available for impossible
            //TODO: Maybe make it weighted toward the hard end?
            max = 1000;
            min = 0;
        }
        else if (ret > 650) //Very Hard
        {
            max = Mathf.Clamp(ret + 150, 0, 1000);
            min = Mathf.Clamp(ret - 200, 0, 1000);
        }
        else if (ret > 500) //Hard
        {
            max = Mathf.Clamp(ret + 100, 0, 1000);
            min = Mathf.Clamp(ret - 150, 0, 1000);
        }
        else if (ret > 300) //Moderate
        {
            max = Mathf.Clamp(ret + 75, 0, 1000);
            min = Mathf.Clamp(ret - 100, 0, 1000);
        }
        else if (ret > 150) //Easy
        {
            max = Mathf.Clamp(ret + 35, 0, 1000);
            min = 0;
        }
        else //Very Easy
        {
            max = Mathf.Clamp(ret + 20, 0, 1000);
            min = 0;
        }

        List<Helpful.StatTypes> affectingStats = Helpful.GetStatTypesThatAffectBattleGame(gameType);

        if (affectingStats.Count > 3)
            Debug.Log("Game has more than 3 affecting stats and it shouldn't");

        int statsMod = 0;

        for (int i = 0; i < affectingStats.Count; i++)
        {
            statsMod += (player.GetStatWithMods(affectingStats[i]) - enemy.GetStatWithMods(affectingStats[i])) * (3 - i);
        }

        //TODO: review this? 4 might not be enough, or might want to give a larger/smaller bonus from stat differences
        //      depending on level or difficulty or stat totals?
        statsMod = statsMod / 4;

        min = Mathf.Clamp(Mathf.Clamp(min - statsMod, 0, 1000) + (int)gamePerformanceModifierValue, 0, 1000);
        max = Mathf.Clamp(Mathf.Clamp(max - statsMod, 0, 1000) + (int)gamePerformanceModifierValue, 0, 1000);

        ret = Random.Range(min, max + 1);

        return ret;
    }
}
