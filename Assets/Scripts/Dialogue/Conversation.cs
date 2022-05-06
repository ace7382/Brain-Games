using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Conversation
{
    [TextArea(3, 10)]
    public List<string> conversationSteps;

    private int         currentIndex;

    public void StartConversation()
    {
        currentIndex = -1;
    }

    public string GetNextStep()
    {
        currentIndex++;

        if (currentIndex >= conversationSteps.Count)
        {
            return null;
        }
        else
        {
            return conversationSteps[currentIndex];
        }
    }
}
