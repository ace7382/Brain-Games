using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UniversalInspectorVariables : MonoBehaviour
{
    public static UniversalInspectorVariables instance = null;

    public TMP_FontAsset    KGHappySolid;
    public TMP_FontAsset    KGHappy;
    public int              gameScreenOrderInLayer;
    public int              popupScreenOrderInLayer;
    public List<Color>      statColors;

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        if (statColors.Count != (int)Helpful.StatTypes.COUNT)
            Debug.LogWarning("The number of Stat Colors on the universal Inspector Variabels does not equal the number of stats");
    }
}
