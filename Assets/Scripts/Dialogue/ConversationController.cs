using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConversationController : MonoBehaviour
{
    public Conversation                         conversation;
    public float                                lettersPerSecond;

    #region Inspector Variables

    [SerializeField] private GameObject         display;
    [SerializeField] private TextMeshProUGUI    displayText;

    #endregion

    #region Private Variables

    private string                              currentLine;
    private IEnumerator                         typingCoroutine;

    #endregion

    #region Unity Functions

    private void Start()
    {

    }

    #endregion

    #region Public Functions

    public void ShowNextLine()
    {
        if (conversation == null || conversation.conversationSteps.Count == 0)
            return;

        if (display.activeInHierarchy == false)
        {
            display.SetActive(true);
            conversation.StartConversation();
        }

        if (currentLine == null)
        {
            currentLine     = conversation.GetNextStep();

            if (currentLine == null) //Conversation is over
            {
                display.SetActive(false);
                conversation = null;
                return;
            }

            typingCoroutine = TypeLine();

            StartCoroutine(typingCoroutine);
        }
        else
        {
            StopCoroutine(typingCoroutine);

            typingCoroutine = null;

            ShowFullLine();
        }
    }

    #endregion

    #region Private Functions

    private IEnumerator TypeLine()
    {
        WaitForSeconds w = new WaitForSeconds(1f / lettersPerSecond);

        displayText.text = "";

        for (int i = 0; i < currentLine.Length; i++)
        {
            displayText.text += currentLine[i];

            yield return w;
        }

        currentLine = null;
    }

    private void ShowFullLine()
    {
        displayText.text    = currentLine;

        currentLine     = null;
    }
    
    #endregion
}
