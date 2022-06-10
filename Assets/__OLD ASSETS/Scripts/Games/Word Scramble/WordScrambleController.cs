using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;
using TMPro;
using UnityEngine.UI;
using Doozy.Runtime.UIManager.Containers;

public class WordScrambleController : MonoBehaviour
{
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
    private bool                                initialLoad;

    private SignalReceiver                      gamemanagement_gamesetup_receiver;
    private SignalStream                        gamemanagement_gamesetup_stream;
    private SignalReceiver                      wordscramble_tileclicked_receiver;
    private SignalStream                        wordscramble_tileclicked_stream;
    private SignalReceiver                      quitconfirmation_exitlevel_receiver;
    private SignalStream                        quitconfirmation_exitlevel_stream;

    private const float                         FOUND_WORD_COLUMN_WIDTH_PER_LETTER = 35f;
    private const float                         FOUND_WORD_COLUMN_HEIGHT = 50f;

    private void Awake()
    {
        Canvas c        = GetComponentInParent<Canvas>();
        c.worldCamera   = Camera.main;
        c.sortingOrder  = UniversalInspectorVariables.instance.gameScreenOrderInLayer;

        gamemanagement_gamesetup_stream         = SignalStream.Get("GameManagement", "GameSetup");
        wordscramble_tileclicked_stream         = SignalStream.Get("WordScramble", "TileClicked");
        quitconfirmation_exitlevel_stream       = SignalStream.Get("QuitConfirmation", "ExitLevel");

        gamemanagement_gamesetup_receiver       = new SignalReceiver().SetOnSignalCallback(Setup);
        wordscramble_tileclicked_receiver       = new SignalReceiver().SetOnSignalCallback(TileClicked);
        quitconfirmation_exitlevel_receiver     = new SignalReceiver().SetOnSignalCallback(ExitGameFromQuitConfirmationScreen);
    }

    private void OnEnable()
    {
        gamemanagement_gamesetup_stream.ConnectReceiver(gamemanagement_gamesetup_receiver);
        wordscramble_tileclicked_stream.ConnectReceiver(wordscramble_tileclicked_receiver);
        quitconfirmation_exitlevel_stream.ConnectReceiver(quitconfirmation_exitlevel_receiver);
    }

    private void OnDisable()
    {
        gamemanagement_gamesetup_stream.DisconnectReceiver(gamemanagement_gamesetup_receiver);
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

    //Called by the View - WordScramblePlay's Show Animation Finished callback
    public void StartGame()
    {
        //TODO: Evaluate the necesity of this
        //      No timer or anything currently. Still linked on the callback incase something needs to be added here
    }

    //Used by the Submit Button's Click callback
    public void SubmitButtonClicked()
    {
        AudioManager.instance.Play("Button Click");

        SubmitWord();
    }

    //Called by the Finish Button's OnClick Behavior
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

        if (currentWordScrambleLevel.objective1 && currentWordScrambleLevel.levelsUnlockedByThisLevel != null)
        {
            for (int i = 0; i < currentWordScrambleLevel.levelsUnlockedByThisLevel.Count; i++)
            {
                if (!currentWordScrambleLevel.levelsUnlockedByThisLevel[i].unlocked)
                {
                    currentWordScrambleLevel.levelsUnlockedByThisLevel[i].unlocked = true;
                    //GameManager.instance.SetWorldMapUnlockLevels(currentWordScrambleLevel.levelsUnlockedByThisLevel[i]);
                }
            }
        }

        Signal.Send("GameManagement", "DisableExitLevelButton", false);

        LevelResultsData results    = new LevelResultsData();
        results.successIndicator    = true;
        results.subtitleText        = string.Format("{0} Words Found", currentWordScrambleLevel.foundWords.Count.ToString());

        GameManager.instance.SetLevelResults(results);

        Signal.Send("GameManagement", "LevelEnded", 0);
    }

    private void Setup(Signal signal)
    {
        initialLoad = true;

        currentWordScrambleLevel = (WordScrambleLevel)GameManager.instance.currentLevelOLD;

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

        Signal.Send("GameManagement", "DisableExitLevelButton", true);

        UpdateWordFoundText();
        ResizeFoundWordScroll();

        foundWordList.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        initialLoad = false;
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

            AudioManager.instance.Play("Chalkboard Writing");
        }
        else
        {
            int removeIndex = tiles[index].selectedPlace;
            selectedWord = removeIndex > 0 ? selectedWord.Substring(0, removeIndex - 1) : "";

            AudioManager.instance.Play("Chalkboard Erasing");

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

        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        t.text = word;

        if (word == currentWordScrambleLevel.specialWord)
        {
            t.color = Color.yellow;

            if (!initialLoad)
                AudioManager.instance.Play("Go");
        }

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

    private void SubmitWord()
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
