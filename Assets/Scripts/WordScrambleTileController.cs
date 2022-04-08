using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Doozy.Runtime.Signals;

public class WordScrambleTileController : MonoBehaviour
{
    public char             letter;
    public bool             selected;
    public int              selectedPlace;

    public TextMeshProUGUI  letText;
    public Image            tileBackground;

    public void SetUp(char c)
    {
        letter = c;
        letText.text = c.ToString();
        selected = false;
        selectedPlace = -1;
    }

    public void Clear()
    {
        letter = ' ';
        letText.text = "";
        selected = false;
        selectedPlace = -1;
    }

    //Used by the WordScramble Tile Prefab's CLick callbaack
    public void Click()
    {
        Signal.Send("WordScramble", "TileClicked", this);
    }
}
