using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleGameControllerBase : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] protected CanvasGroup gameElementsCanvasGroup;

    #endregion

    #region Public Properties

    public CanvasGroup GameElementsCanvasGroup { get { return gameElementsCanvasGroup; } }

    #endregion

    #region Unity Functions

    protected virtual void Awake()
    {

    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void OnDisable()
    {

    }

    #endregion

    #region Public Functions

    public virtual void StartGame()
    {
        Debug.Log("BattleGameBase 'Start Game' Function called");
    }

    public virtual void EndGame()
    {

    }

    public virtual void Pause()
    {

    }

    public virtual void Unpause()
    {

    }

    public abstract void BoardReset();
    public abstract string GetBattleGameName();

    #endregion

    #region Protected Functions

    protected virtual void Setup()
    {

    }

    #endregion
}
