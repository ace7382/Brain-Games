using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniversalInspectorVariables : MonoBehaviour
{
    public static UniversalInspectorVariables instance = null;

    public Font KGHappySolid;
    public Font KGHappy;
    public int  gameScreenOrderInLayer;
    public int  popupScreenOrderInLayer;

    public void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
}
