using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;
using TMPro;
using UnityEngine.UI;
using Doozy.Runtime.UIManager.Containers;

public class WordScrambleController : MonoBehaviour
{
    public WordScrambleLevel    currentWordScrambleLevel;

    public GameObject           letterTray;
    public GameObject           selectedLettersTray;
    public GameObject           foundWordList;
    public GameObject           letterTilePrefab;
    public GameObject           foundWordPrefab;
    public TextMeshProUGUI      wordCountFoundText;
    public TextMeshProUGUI      wordCountGoalText;
    public GameObject           finishButton;

    public TMP_FontAsset        selectedFont;
    public TMP_FontAsset        unselectedFont;

    public string               selectedWord;

    private List<WordScrambleTileController> tiles;

    private SignalReceiver      wordscramble_wordscramblesetup_receiver;
    private SignalStream        wordscramble_wordscramblesetup_stream;
    private SignalReceiver      wordscramble_tileclicked_receiver;
    private SignalStream        wordscramble_tileclicked_stream;
    private SignalReceiver      quitconfirmation_exitlevel_receiver;
    private SignalStream        quitconfirmation_exitlevel_stream;

    private const float         FOUND_WORD_COLUMN_WIDTH_PER_LETTER = 35f;
    private const float         FOUND_WORD_COLUMN_HEIGHT = 50f;

    private void Awake()
    {
        wordscramble_wordscramblesetup_stream   = SignalStream.Get("WordScramble", "WordScrambleSetup");
        wordscramble_tileclicked_stream         = SignalStream.Get("WordScramble", "TileClicked");
        quitconfirmation_exitlevel_stream       = SignalStream.Get("QuitConfirmation", "ExitLevel");

        wordscramble_wordscramblesetup_receiver = new SignalReceiver().SetOnSignalCallback(SetUp);
        wordscramble_tileclicked_receiver       = new SignalReceiver().SetOnSignalCallback(TileClicked);
        quitconfirmation_exitlevel_receiver     = new SignalReceiver().SetOnSignalCallback(ExitGameFromQuitConfirmationScreen);
    }

    private void OnEnable()
    {
        wordscramble_wordscramblesetup_stream.ConnectReceiver(wordscramble_wordscramblesetup_receiver);
        wordscramble_tileclicked_stream.ConnectReceiver(wordscramble_tileclicked_receiver);
        quitconfirmation_exitlevel_stream.ConnectReceiver(quitconfirmation_exitlevel_receiver);
    }

    private void OnDisable()
    {
        wordscramble_wordscramblesetup_stream.DisconnectReceiver(wordscramble_wordscramblesetup_receiver);
        wordscramble_tileclicked_stream.DisconnectReceiver(wordscramble_tileclicked_receiver);
        quitconfirmation_exitlevel_stream.DisconnectReceiver(quitconfirmation_exitlevel_receiver);
    }

    //Called by the WordScramblePlay screen's OnHide callback
    public void OnHide()
    {
        quitconfirmation_exitlevel_stream.DisconnectReceiver(quitconfirmation_exitlevel_receiver);
    }

    //Called by the WordScramblePlay screen's OnShow callback
    public void OnShow()
    {
        quitconfirmation_exitlevel_stream.ConnectReceiver(quitconfirmation_exitlevel_receiver);
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
        for (int i = 0; i < tiles.Count; i++)
            ReturnTile(tiles[i]);

        selectedWord = "";

        if (!currentWordScrambleLevel.objective1 && currentWordScrambleLevel.foundWords.Count >= currentWordScrambleLevel.goalWordCount)
            currentWordScrambleLevel.objective1 = true;

        if (!currentWordScrambleLevel.objective2 && currentWordScrambleLevel.foundWords.Count >= currentWordScrambleLevel.secondGoalWordCount)
            currentWordScrambleLevel.objective2 = true;

        if (!currentWordScrambleLevel.objective3 && currentWordScrambleLevel.foundWords.Contains(currentWordScrambleLevel.specialWord))
            currentWordScrambleLevel.objective3 = true;

        if (currentWordScrambleLevel.nextLevel != null && !currentWordScrambleLevel.nextLevel.unlocked && currentWordScrambleLevel.objective1)
            currentWordScrambleLevel.nextLevel.unlocked = true;

        //object[] data = new object[1];
        //data[0] = currentWordScrambleLevel;

        //Signal.Send("WordScramble", "EndGame", data);

        object[] data   = new object[3];
        data[0]         = currentWordScrambleLevel;
        data[1]         = true; //Anytime you leave this mode it's a "Success" since progress is maintained
        data[2]         = string.Format("{0} Words Found", currentWordScrambleLevel.foundWords.Count.ToString());

        Signal.Send("GameManagement", "LevelEnded", data);
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
            currentWordScrambleLevel = (WordScrambleLevel)currentWordScrambleLevel.nextLevel;
        }
        //data[0] == 1 => Replay doesn't need to update the WOrdScrambleLevel

        //TODO: Pool these too
        foreach (Transform child in foundWordList.transform)
            Destroy(child.gameObject);

        foundWordList.GetComponent<GridLayoutGroup>().cellSize
            = new Vector2(FOUND_WORD_COLUMN_WIDTH_PER_LETTER * currentWordScrambleLevel.letters.Length, FOUND_WORD_COLUMN_HEIGHT);

        selectedWord = "";

        if (tiles == null)
        {
            tiles = new List<WordScrambleTileController>();
        }

        letterTray.GetComponent<HorizontalLayoutGroup>().enabled = true;

        for (int i = 0; i < currentWordScrambleLevel.letters.Length; i++)
        {
            if (tiles.Count > i)
            {
                tiles[i].Clear();

                if (i < currentWordScrambleLevel.letters.Length)
                {
                    tiles[i].SetUp(currentWordScrambleLevel.letters[i]);
                    tiles[i].gameObject.SetActive(true);
                }
                else
                {
                    tiles[i].gameObject.SetActive(false);
                }
            }
            else //if there is no tile yet
            {
                char letter = currentWordScrambleLevel.letters[i];

                GameObject go = Instantiate(letterTilePrefab);
                WordScrambleTileController control = go.GetComponent<WordScrambleTileController>();

                go.transform.SetParent(letterTray.transform);

                go.transform.localScale = Vector3.one;
                go.transform.localPosition = Vector3.one;

                control.SetUp(letter);
                tiles.Add(control);
            }
        }

        for (int i = currentWordScrambleLevel.letters.Length; i < tiles.Count; i++)
        {
            tiles[i].Clear();
            tiles[i].gameObject.SetActive(false);
        }

        Canvas.ForceUpdateCanvases();
        letterTray.GetComponent<HorizontalLayoutGroup>().enabled = false;

        for (int i = 0; i < tiles.Count; i++) //needs to be after the canvas update above
        {
            tiles[i].trayPosition = tiles[i].GetComponent<RectTransform>().anchoredPosition;
        }

        for (int i = 0; i < currentWordScrambleLevel.foundWords.Count; i++)
        {
            CreateFoundWordListing(currentWordScrambleLevel.foundWords[i]);
        }

        UpdateWordFoundText();
        ResizeFoundWordScroll();
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

            tiles[index].selectedPlace = selectedWord.Length;

            tiles[index].letText.font = selectedFont;
            tiles[index].letText.color = Color.white;
            tiles[index].tileBackground.enabled = false;
        }
        else
        {
            int removeIndex = tiles[index].selectedPlace;
            selectedWord = removeIndex > 0 ? selectedWord.Substring(0, removeIndex - 1) : "";

            for (int i = 0; i < tiles.Count; i++)
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
        RectTransform r = t.GetComponent<RectTransform>();

        r.SetParent(letterTray.transform);
        r.localRotation = Quaternion.identity;
        r.anchoredPosition = t.trayPosition;

        Canvas.ForceUpdateCanvases();

        t.selected = false;
        t.selectedPlace = -1;

        t.letText.font = unselectedFont;
        t.tileBackground.enabled = true;
        t.letText.color = Color.black;
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

        ResizeFoundWordScroll();
    }

    private void ResizeFoundWordScroll()
    {
        int colCount = currentWordScrambleLevel.foundWords.Count / 4 +
                        (currentWordScrambleLevel.foundWords.Count % 4 > 0 ? 1 : 0);

        LayoutElement le = foundWordList.GetComponent<LayoutElement>();
        GridLayoutGroup g = foundWordList.GetComponent<GridLayoutGroup>();

        le.preferredWidth =
            g.padding.left + g.padding.right
            + (g.spacing.x * (colCount - 1))
            + (colCount * FOUND_WORD_COLUMN_WIDTH_PER_LETTER * currentWordScrambleLevel.letters.Length);

        Canvas.ForceUpdateCanvases();

        if (le.preferredWidth > foundWordList.transform.parent.GetComponent<RectTransform>().rect.width)
            foundWordList.GetComponentInParent<ScrollRect>().enabled = true;
        else
            foundWordList.GetComponentInParent<ScrollRect>().enabled = false;
    }

    private void UpdateWordFoundText()
    {
        wordCountFoundText.text = currentWordScrambleLevel.foundWords.Count.ToString();
        wordCountGoalText.text = currentWordScrambleLevel.foundWords.Count <= currentWordScrambleLevel.goalWordCount ?
                                      currentWordScrambleLevel.goalWordCount.ToString() :
                                      currentWordScrambleLevel.secondGoalWordCount.ToString();

        finishButton.SetActive(currentWordScrambleLevel.foundWords.Count >= currentWordScrambleLevel.goalWordCount);
    }

    private void ExitGameFromQuitConfirmationScreen(Signal signal)
    {
        EndGame();
    }
}
