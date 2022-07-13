using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stats
{
    #region Private Variables

    [SerializeField] private int[]              _data = new int[(int)Helpful.StatTypes.COUNT];

    #endregion

    #region Public Properties

    public int this[Helpful.StatTypes s]
    {
        get { return _data[(int)s]; }
        set
        {
            if (_data[(int)s] == value)
                return;

            _data[(int)s] = value;
        }
    }

    #endregion
}
