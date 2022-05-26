using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldMapController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private GameObject     blockMapTouchInputGO;

    #endregion

    #region Private Variables

    [SerializeField]private List<LevelBase> levelsToAnimateUnlocking;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        Canvas c        = GetComponentInParent<Canvas>();
        c.worldCamera   = Camera.main;
        c.sortingOrder  = UniversalInspectorVariables.instance.gameScreenOrderInLayer;

        Setup();
    }

    #endregion

    #region Public Functions

    //Called by the World Map View's OnVisible Callback
    public void OnShow()
    {
        blockMapTouchInputGO.SetActive(true);

        StartCoroutine(UnlockLevels());
    }

    public void AddLevelsToUnlock(List<LevelBase> levels)
    {
        if (levelsToAnimateUnlocking == null)
            levelsToAnimateUnlocking = new List<LevelBase>();

        for (int i = 0; i < levels.Count; i++)
            if (!levelsToAnimateUnlocking.Contains(levels[i]))
                levelsToAnimateUnlocking.Add(levels[i]);
    }

    public void AddLevelsToUnlock(LevelBase level)
    {
        if (levelsToAnimateUnlocking == null)
            levelsToAnimateUnlocking = new List<LevelBase>();

        if (!levelsToAnimateUnlocking.Contains(level))
            levelsToAnimateUnlocking.Add(level);
    }

    #endregion

    #region Private Functions

    private void Setup()
    {
        NewLevelSelectButtonController[] levelButtons = FindObjectsOfType<NewLevelSelectButtonController>();

        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].ShowLockedStatus();
        }
    }

    //TODO: Add an invisible panel that blocks touch/mouse input
    private IEnumerator UnlockLevels()
    {
        WaitForSeconds pauseBetweenUnlocks = new WaitForSeconds(.5f);

        while (levelsToAnimateUnlocking != null && levelsToAnimateUnlocking.Count > 0)
        {
            NewLevelSelectButtonController buttonToUnlock =
                FindObjectsOfType<NewLevelSelectButtonController>().ToList().Find(x => x.Level == levelsToAnimateUnlocking[0]);

            if (buttonToUnlock != null)
            {
                yield return pauseBetweenUnlocks;
                
                yield return buttonToUnlock.AnimatedUnlocking();
            }

            levelsToAnimateUnlocking.RemoveAt(0);
        }

        blockMapTouchInputGO.SetActive(false);
    }

    #endregion
}
