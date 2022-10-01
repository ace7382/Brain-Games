using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;
using UnityEngine.UI;

public class UnitSelectCardController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private GameObject disabledIndicator;
    [SerializeField] private GameObject requiredIndicator;
    [SerializeField] private GameObject KOedIndicator;
    [SerializeField] private Color      disabledColor;

    #endregion

    #region Private Variables

    private Unit    unit;       //TODO: Might just need a string reference with the unit's name?
    private Image   unitImage;

    #endregion

    #region Public Properties

    public Unit Unit { get { return unit; } }
    public bool Required { 
        get { return requiredIndicator.activeInHierarchy; } 
        set
        {
            requiredIndicator.SetActive(value);
        }
    }

    public bool Disabled { 
        get { return disabledIndicator.activeInHierarchy; } 
        set
        {
            disabledIndicator.SetActive(value);
            unitImage.color = value ? disabledColor : (KOed ? disabledColor : Color.white);
        }
    }

    public bool KOed {
        get { return KOedIndicator.activeInHierarchy; }
        set
        {
            KOedIndicator.SetActive(value);
            unitImage.color = value ? disabledColor : (Disabled ? disabledColor : Color.white);
        }
    }

    #endregion

    #region Public Functions

    //When clicked, move between all units and not all units
    //Hold down to get info?
    //Can be disabled if not allowed in level or KOed

    public void Setup(Unit u, bool disabled, bool reqed = false, bool k = false)
    {
        Debug.Log(string.Format("{0} - Disabled: {1} required: {2}",
            u.Name, disabled, reqed));

        unit                            = u;
        unitImage                       = GetComponent<Image>();
        unitImage.sprite                = u.InBattleSprite;
        Disabled                        = disabled;
        Required                        = reqed;
        KOed                            = k;
    }

    public void OnClick()
    {
        if (Disabled || KOed || Required)
            return;

        object[] info   = new object[2];
        info[0]         = this;
        info[1]         = GetComponent<RectTransform>();

        Signal.Send("Battle", "UnitSelectCardClicked", info);
    }

    #endregion
}
