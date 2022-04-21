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

    public Vector3          trayPosition;

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

        trayPosition = Vector3.zero;
    }

    //Used by the WordScramble Tile Prefab's CLick callbaack
    public void Click()
    {
        AudioManager.instance.Play("Chalkboard Writing");

        Signal.Send("WordScramble", "TileClicked", this);
    }
}
