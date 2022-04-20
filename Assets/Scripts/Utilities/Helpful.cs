using BizzyBeeGames;
using UnityEngine;
using UnityEngine.UI;

public static class Helpful
{
    public static void TextPopup(string word, Transform par, Vector2 center, Color col, Font font)
    {
        GameObject floatingTextObject = new GameObject("found_word_floating_text", typeof(Shadow));
        RectTransform floatingTextRectT = floatingTextObject.AddComponent<RectTransform>();
        Text floatingText = floatingTextObject.AddComponent<Text>();
        floatingText.font = font;
        floatingText.fontSize = 80;

        floatingText.text = word;
        floatingText.color = col;

        floatingText.transform.SetParent(par, false);
        floatingTextRectT.anchoredPosition = center;
        floatingTextRectT.localScale = Vector3.one;
        floatingTextRectT.anchorMin = new Vector2(.5f, .5f);
        floatingTextRectT.anchorMax = new Vector2(.5f, .5f);
        floatingTextRectT.pivot = new Vector2(.5f, .5f);

        ContentSizeFitter csf = floatingTextObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        UIAnimation anim;
        anim = UIAnimation.PositionY(floatingText.rectTransform, center.y, center.y + 200f, .75f);
        anim.Play();
        anim = UIAnimation.Color(floatingText, new Color(floatingText.color.r, floatingText.color.g, floatingText.color.b, 0f), 1f);
        anim.OnAnimationFinished = (GameObject obj) => { GameObject.Destroy(obj); };
        anim.Play();
    }

    public static string GetLevelObjectiveTitles(LevelBase level, int objectiveNumber)
    {
        if (level.GetType() == typeof(TimedTriviaLevel))
        {
            if (objectiveNumber == 1)
            {
                return "Completed";
            }
            else if (objectiveNumber == 2)
            {
                System.TimeSpan ts = System.TimeSpan.FromSeconds(((TimedTriviaLevel)level).parTimeRemainingInSeconds);

                return string.Format("{0}:{1} Remaining", ts.Minutes.ToString(), ts.Seconds.ToString("00"));
            }
            else if (objectiveNumber == 3)
            {
                return "All Correct";
            }
        }
        else if (level.GetType() == typeof(WordScrambleLevel))
        {
            if (objectiveNumber == 1)
            {
                return string.Format("{0} Words Found", ((WordScrambleLevel)level).goalWordCount.ToString());
            }
            else if (objectiveNumber == 2)
            {
                return string.Format("{0} Words Found", ((WordScrambleLevel)level).secondGoalWordCount.ToString());
            }
            else if (objectiveNumber == 3)
            {
                return "Special Word Found";
            }
        }
        else if (level.GetType() == typeof(PathPuzzleLevel))
        {
            if (objectiveNumber == 1)
            {
                return "Completed";
            }
            else if (objectiveNumber == 2)
            {
                System.TimeSpan ts = System.TimeSpan.FromSeconds(((PathPuzzleLevel)level).parTimeInSeconds);

                return string.Format("{0}:{1} Remaining", ts.Minutes.ToString(), ts.Seconds.ToString("00"));
            }
            else if (objectiveNumber == 3)
            {
                return string.Format("{0} Pieces Connected", ((PathPuzzleLevel)level).piecesConnectedGoal.ToString());
            }
        }

        Debug.Log("Something went wrong when grabbing a level's objective names");

        return "";
    }
}
