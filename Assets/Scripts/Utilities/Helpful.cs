using BizzyBeeGames;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class Helpful
{
    public enum BattleGameTypes
    {
        NO_GAME         = -999,
        ArrowSwipe      = 0,
        ColorDissonance = 10
    }

    public static string GetBattleGameboardLoadingPath(BattleGameTypes gameType)
    {
        switch (gameType)
        {
            case BattleGameTypes.ArrowSwipe:
                return "Prefabs/Battle Game Boards/Arrow Swipe Board";
            case BattleGameTypes.ColorDissonance:
                return "Prefabs/Battle Game Boards/Color Dissonance Board";
            default:
                Debug.Log("Game Type invalid");
                return "YEET";
        }
    }

    //TODO: Make this use TMPro
    public static void TextPopup(string word, Transform par, Vector2 center, Color col, Font font, int fontSize)
    {
        GameObject floatingTextObject = new GameObject("found_word_floating_text", typeof(Shadow));
        RectTransform floatingTextRectT = floatingTextObject.AddComponent<RectTransform>();
        Text floatingText = floatingTextObject.AddComponent<Text>();
        floatingText.font = font;
        floatingText.fontSize = fontSize;

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
                return string.Format("{0} Found", ((WordScrambleLevel)level).objective3 ? 
                    "\"" + ((WordScrambleLevel)level).specialWord.ToUpperInvariant() + "\""
                    : "Special Word");
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
        else if (level.GetType() == typeof(SpeedMathLevel))
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
        else if (level.GetType() == typeof(CodeBreakerLevel))
        {
            if (objectiveNumber == 1)
            {
                return "Code Broken";
            }
            else if (objectiveNumber == 2)
            {
                return string.Format("{0} or fewer attempts", ((CodeBreakerLevel)level).highNumberOfGuessesGoal.ToString());
            }
            else if (objectiveNumber == 3)
            {
                return string.Format("{0} or fewer attempts", ((CodeBreakerLevel)level).lowNumberOfGuessesGoal.ToString());
            }
        }

        Debug.Log("Something went wrong when grabbing a level's objective names");

        return "";
    }

    public static int GetGameID(System.Type t)
    {
        if (t == typeof(WordScrambleLevel))
            return 0;
        else if (t == typeof(TimedTriviaLevel))
            return 1;
        else if (t == typeof(PathPuzzleLevel))
            return 2;
        else if (t == typeof(SpeedMathLevel))
            return 3;
        else if (t == typeof(CodeBreakerLevel))
            return 4;

        return -1;
    }

    public static IEnumerator Pulse(Transform transform, float currentRatio, float growthBound, float shrinkBound, float approachSpeed)
    {
        // Run this indefinitely
        while (true)
        {
            // Get bigger for a few seconds
            while (currentRatio != growthBound)
            {
                // Determine the new ratio to use
                currentRatio = Mathf.MoveTowards(currentRatio, growthBound, approachSpeed);

                // Update scale
                transform.localScale = Vector3.one * currentRatio;

                yield return new WaitForEndOfFrame();
            }

            // Shrink for a few seconds
            while (currentRatio != shrinkBound)
            {
                // Determine the new ratio to use
                currentRatio = Mathf.MoveTowards(currentRatio, shrinkBound, approachSpeed);

                // Update scale
                transform.localScale = Vector3.one * currentRatio;

                yield return new WaitForEndOfFrame();
            }
        }
    }

    public static IEnumerator FadeCanvasIn(CanvasGroup c, float speed)
    {
        while (c.alpha < 1)
        {
            c.alpha = Mathf.MoveTowards(c.alpha, 1, speed * Time.deltaTime);

            yield return null;
        }
    }

    public static IEnumerator FadePanelOut(CanvasGroup c, float speed)
    {
        while (c.alpha > 0)
        {
            c.alpha = Mathf.MoveTowards(c.alpha, 0f, speed * Time.deltaTime);

            yield return null;
        }
    }

    public static Vector3 ScreenToWorld(Camera camera, Vector3 position)
    {
        position.z = camera.nearClipPlane;

        return camera.ScreenToWorldPoint(position);
    }

    public static object GetInstance(string strFullyQualifiedName)
    {
        Type t = Type.GetType(strFullyQualifiedName);
        
        return Activator.CreateInstance(t);
    }
}
