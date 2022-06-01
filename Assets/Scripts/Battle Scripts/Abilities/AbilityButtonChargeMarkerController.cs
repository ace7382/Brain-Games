using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityButtonChargeMarkerController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Image fill;

    #endregion

    #region Public Functions

    public void SetFillColor(Color col)
    {
        fill.color = col;
    }

    #endregion
}
