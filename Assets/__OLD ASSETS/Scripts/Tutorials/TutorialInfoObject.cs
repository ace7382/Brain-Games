using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tutorial", menuName = "New Tutorial", order = 55)]
public class TutorialInfoObject : ScriptableObject
{
    [System.Serializable]
    public class TutorialPage
    {
        public GameObject instructionPage;
        public GameObject displayPage;
    }

    public List<TutorialPage>   pages;
    public bool                 pagesOnRight;
}
