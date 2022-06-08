using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleGameControllerBase : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] protected CanvasGroup gameElementsCanvasGroup;

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
        Debug.Log("Game Started");
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

    #endregion

    #region Protected Functions

    protected virtual void Setup()
    {

    }

    #endregion
}
