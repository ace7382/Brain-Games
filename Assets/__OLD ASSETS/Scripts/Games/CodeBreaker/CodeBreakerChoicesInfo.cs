using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class CodeBreakerChoicesInfo
{
    public enum CodebreakerChoices
    {
        NULL            = -100,
        TestIcon        = 1,
        TestIcon_Blue   = 2,
        TestIcon_Green  = 3,
        TestIcon_Grey   = 4,
        TestIcon_Cyan   = 5,
        TestIcon_Yellow = 6,
        TestIcon_Purple = 7,
        TestIcon_Red    = 8,
    }

    public static void GetChoiceImage(CodebreakerChoices choice, ref Image imageToSet)
    {
        switch (choice)
        {
            case CodebreakerChoices.TestIcon:
                imageToSet.sprite   = Resources.Load<Sprite>("CodeBreakerIcons/Test Icon");
                imageToSet.color    = Color.white;
                break;
            case CodebreakerChoices.TestIcon_Blue:
                imageToSet.sprite   = Resources.Load<Sprite>("CodeBreakerIcons/Test Icon");
                imageToSet.color    = Color.blue;
                break;
            case CodebreakerChoices.TestIcon_Green:
                imageToSet.sprite   = Resources.Load<Sprite>("CodeBreakerIcons/Test Icon");
                imageToSet.color    = Color.green;
                break;
            case CodebreakerChoices.TestIcon_Grey:
                imageToSet.sprite   = Resources.Load<Sprite>("CodeBreakerIcons/Test Icon");
                imageToSet.color    = Color.grey;
                break;
            case CodebreakerChoices.TestIcon_Cyan:
                imageToSet.sprite   = Resources.Load<Sprite>("CodeBreakerIcons/Test Icon");
                imageToSet.color    = Color.cyan;
                break;
            case CodebreakerChoices.TestIcon_Yellow:
                imageToSet.sprite   = Resources.Load<Sprite>("CodeBreakerIcons/Test Icon");
                imageToSet.color    = Color.yellow;
                break;
            case CodebreakerChoices.TestIcon_Purple:
                imageToSet.sprite   = Resources.Load<Sprite>("CodeBreakerIcons/Test Icon");
                imageToSet.color    = Color.magenta;
                break;
            case CodebreakerChoices.TestIcon_Red:
                imageToSet.sprite   = Resources.Load<Sprite>("CodeBreakerIcons/Test Icon");
                imageToSet.color    = Color.red;
                break;
        }
    }
}
