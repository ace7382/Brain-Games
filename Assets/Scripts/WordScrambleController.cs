using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;
using TMPro;
using UnityEngine.UI;

public class WordScrambleController : MonoBehaviour
{
    //TODO: When finding words that will overflow the found words box, figure out how to
    //      increase the content box's size so that it's fully scrollable. I think I figured
    //      it out in Achievement Hunter's shop code? maybe on the word search somewhere too??

    public TextAsset                            FullWordList;
    public WordScrambleLevel                    currentWordScrambleLevel;

    public GameObject                           letterTray;
    public GameObject                           selectedLettersTray;
    public GameObject                           foundWordList;
    public GameObject                           letterTilePrefab;
    public GameObject                           foundWordPrefab;
    public TextMeshProUGUI                      wordCountFoundText;
    public TextMeshProUGUI                      wordCountGoalText;
    public GameObject                           finishButton;

    public TMP_FontAsset                        selectedFont;
    public TMP_FontAsset                        unselectedFont;

    public string                               selectedWord;

    private List<WordScrambleTileController>    tiles;

    private SignalReceiver                      wordscramble_wordscramblesetup_receiver;
    private SignalStream                        wordscramble_wordscramblesetup_stream;
    private SignalReceiver                      wordscramble_tileclicked_receiver;
    private SignalStream                        wordscramble_tileclicked_stream;

    private void Awake()
    {
        wordscramble_wordscramblesetup_stream = SignalStream.Get("WordScramble", "WordScrambleSetup");
        wordscramble_tileclicked_stream = SignalStream.Get("WordScramble", "TileClicked");

        wordscramble_wordscramblesetup_receiver = new SignalReceiver().SetOnSignalCallback(SetUp);
        wordscramble_tileclicked_receiver = new SignalReceiver().SetOnSignalCallback(TileClicked);
    }

    private void OnEnable()
    {
        wordscramble_wordscramblesetup_stream.ConnectReceiver(wordscramble_wordscramblesetup_receiver);
        wordscramble_tileclicked_stream.ConnectReceiver(wordscramble_tileclicked_receiver);
    }

    private void OnDisable()
    {
        wordscramble_wordscramblesetup_stream.DisconnectReceiver(wordscramble_wordscramblesetup_receiver);
        wordscramble_tileclicked_stream.DisconnectReceiver(wordscramble_tileclicked_receiver);
    }

    //Used by the Submit Button's Click callback
    public void SubmitWord()
    {
        if (selectedWord == "")
            return;

        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].selected)
                ReturnTile(tiles[i]);
        }

        if (!currentWordScrambleLevel.foundWords.Contains(selectedWord) 
            && currentWordScrambleLevel.hiddenWords.Contains(selectedWord))
            FoundWord(selectedWord);

        selectedWord = "";
    }

    public void EndGame()
    {
        object[] data = new object[1];
        data[0] = currentWordScrambleLevel;

        Signal.Send("WordScramble", "EndGame", data);
    }

    private void SetUp(Signal signal)
    {
        //Signal Data should be object[2]
        //  index 0 =>  int                 - 0 == new level, 1 == replay current level, 2 == play next level
        //  index 1 =>  WordScrambleLevel   - the info for the current level (only needed if 0 above)

        object[] data = signal.GetValueUnsafe<object[]>();

        if ((int)data[0] == 0) //Level was defined by call
        {
            currentWordScrambleLevel = (WordScrambleLevel)data[1];
        }
        else if ((int)data[0] == 2) //Play Next Level
        {
            currentWordScrambleLevel = currentWordScrambleLevel.nextWordScrambleLevel;
        }
        //data[0] == 1 => Replay doesn't need to update the WOrdScrambleLevel

        tiles       = new List<WordScrambleTileController>();

        //TODO: Pool these too
        foreach (Transform child in foundWordList.transform)
            Destroy(child.gameObject);

        foundWordList.GetComponent<GridLayoutGroup>().cellSize = new Vector2(40 * currentWordScrambleLevel.letters.Length, 50);

        //TODO: Pool these, or make them reset/hide
        for (int i = 0; i < currentWordScrambleLevel.letters.Length; i++)
        {
            char letter                         = currentWordScrambleLevel.letters[i];

            GameObject go                       = Instantiate(letterTilePrefab);
            WordScrambleTileController control  = go.GetComponent<WordScrambleTileController>();

            go.transform.SetParent(letterTray.transform);

            go.transform.localScale             = Vector3.one;
            go.transform.localPosition          = Vector3.one;

            control.SetUp(letter);
            tiles.Add(control);
        }

        for (int i = 0; i < currentWordScrambleLevel.foundWords.Count; i++)
        {
            CreateFoundWordListing(currentWordScrambleLevel.foundWords[i]);
        }

        UpdateWordFoundText();
    }


    private void TileClicked(Signal signal)
    {
        int index = tiles.FindIndex(x => x == signal.GetValueUnsafe<WordScrambleTileController>());

        if (index < 0)
        {
            Debug.Log("tile controller was not sent correctly or tile controller was not added to game list correctly");
            return;
        }

        if (!tiles[index].selected)
        {
            tiles[index].transform.SetParent(selectedLettersTray.transform);
            tiles[index].transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-25f, 25f));
            tiles[index].selected = true;

            selectedWord += tiles[index].letter.ToString();

            tiles[index].selectedPlace              = selectedWord.Length;

            tiles[index].letText.font               = selectedFont;
            tiles[index].letText.color              = Color.white;
            tiles[index].tileBackground.enabled     = false;
        }
        else
        {
            int removeIndex     = tiles[index].selectedPlace;
            selectedWord        = removeIndex > 0 ? selectedWord.Substring(0, removeIndex - 1) : "";

            for (int i = 0; i < tiles.Count; i++ )
            {
                if (tiles[i].selected && tiles[i].selectedPlace >= removeIndex)
                {
                    ReturnTile(tiles[i]);
                }
            }
        }
    }

    private void ReturnTile(WordScrambleTileController t)
    {
        t.transform.SetParent(letterTray.transform);
        t.transform.localRotation = Quaternion.identity;

        t.selected                  = false;
        t.selectedPlace             = -1;

        t.letText.font              = unselectedFont;
        t.tileBackground.enabled    = true;
        t.letText.color             = Color.black;
    }

    private void FoundWord(string word)
    {
        currentWordScrambleLevel.foundWords.Add(word);
        currentWordScrambleLevel.hiddenWords.Remove(word);

        CreateFoundWordListing(word);

        UpdateWordFoundText();
    }

    private void CreateFoundWordListing(string word)
    {
        GameObject go = Instantiate(foundWordPrefab);
        go.transform.SetParent(foundWordList.transform);
        go.transform.localScale = Vector3.one;

        go.GetComponent<TextMeshProUGUI>().text = word;
    }

    private void UpdateWordFoundText()
    {
        wordCountFoundText.text     = currentWordScrambleLevel.foundWords.Count.ToString();
        wordCountGoalText.text      = currentWordScrambleLevel.foundWords.Count <= currentWordScrambleLevel.goalWordCount ?
                                      currentWordScrambleLevel.goalWordCount.ToString() :
                                      currentWordScrambleLevel.secondGoalWordCount.ToString();

        finishButton.SetActive(currentWordScrambleLevel.foundWords.Count >= currentWordScrambleLevel.goalWordCount);
    }
}
