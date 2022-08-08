using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Doozy.Runtime.Signals;

public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager   instance = null;

    #endregion

    #region Private Variables

    private NewLevelBase        currentLevel;

    #endregion

    #region Public Properties

    public NewLevelBase         CurrentLevel { get { return currentLevel; } }

    #endregion

    #region Signal Variables

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    #endregion

    #region Public Functions

    public void SetLevel(NewLevelBase level)
    {
        currentLevel = level;
    }

    //TODO: Probably move the list of things to animate to here vs the wordl map controller
    //      currently world map hides when moving to a game, might have it unload completely
    public void SetWorldMapUnlockLevels(NewLevelBase level)
    {
        //TODO: Re-add this; currently turned off bc world map is unloaded before battle
        FindObjectOfType<WorldMapController>().AddLevelsToUnlock(level);
    }

    #endregion
}
