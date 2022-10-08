using BizzyBeeGames;
using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlePopupController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private List<TextMeshProUGUI>  pooledPopups;

    #endregion

    #region Private Variables

    private Queue<Signal>                           statUpPopupQueue;
    private bool                                    showingStatUp;
    private Queue<Signal>                           expPopupQueue;
    private bool                                    canSendNextEXPPopup;

    #endregion

    #region Signal Variables

    private SignalReceiver                          partymanagement_expadded_receiver;
    private SignalStream                            partymanagement_expadded_stream;
    private SignalReceiver                          partymanagement_statleveledup_receiver;
    private SignalStream                            partymanagement_statleveledup_stream;
    private SignalReceiver                          partymanagement_unitstatchanged_receiver;
    private SignalStream                            partymanagement_unitstatchanged_stream;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        partymanagement_expadded_stream             = SignalStream.Get("PartyManagement", "EXPAdded");
        partymanagement_expadded_receiver           = new SignalReceiver().SetOnSignalCallback(EnqueueEXPPopup);
        partymanagement_statleveledup_stream        = SignalStream.Get("PartyManagement", "StatLeveledUp");
        partymanagement_statleveledup_receiver      = new SignalReceiver().SetOnSignalCallback(EnqueueLevelUpPopup);
        partymanagement_unitstatchanged_stream      = SignalStream.Get("PartyManagement", "UnitStatChanged");
        partymanagement_unitstatchanged_receiver    = new SignalReceiver().SetOnSignalCallback(MentalHealthChangePopup);

        statUpPopupQueue                            = new Queue<Signal>();
        showingStatUp                               = false;

        expPopupQueue                               = new Queue<Signal>();
        canSendNextEXPPopup                         = true;
    }

    private void OnEnable()
    {
        partymanagement_expadded_stream.ConnectReceiver(partymanagement_expadded_receiver);
        partymanagement_statleveledup_stream.ConnectReceiver(partymanagement_statleveledup_receiver);
        partymanagement_unitstatchanged_stream.ConnectReceiver(partymanagement_unitstatchanged_receiver);
    }

    private void OnDisable()
    {
        partymanagement_expadded_stream.DisconnectReceiver(partymanagement_expadded_receiver);
        partymanagement_statleveledup_stream.DisconnectReceiver(partymanagement_statleveledup_receiver);
        partymanagement_unitstatchanged_stream.DisconnectReceiver(partymanagement_unitstatchanged_receiver);
    }

    private void Update()
    {
        if (statUpPopupQueue.Count > 0 && !showingStatUp)
        {
            PopupLevelUp(statUpPopupQueue.Dequeue());
        }

        if (expPopupQueue.Count > 0 && canSendNextEXPPopup)
        {
            PopupEXP(expPopupQueue.Dequeue());
        }
    }

    #endregion

    #region Public Functions

    #endregion

    #region Private Functions

    private void MentalHealthChangePopup(Signal signal)
    {
        //Signal is object[]
        //info[0]   - Unit              - The Unit who had a change in a stat
        //info[1]   - Helpful.StatType  - The stat that changed
        //info[2]   - int               - The amount and direction of the change
        //info[3]   - int               - The new value of the stat

        object[] info           = signal.GetValueUnsafe<object[]>();
        Helpful.StatTypes stat  = (Helpful.StatTypes)info[1];

        if (stat == Helpful.StatTypes.COUNT) //TODO: More instances of Count being used as Current MH
        {
            Unit u              = (Unit)info[0];
            int diff            = (int)info[2];

            RectTransform uTran = u == BattleManager.instance.CurrentPlayerUnit.UnitInfo ? 
                BattleManager.instance.CurrentPlayerUnit.GetComponent<RectTransform>()
                : BattleManager.instance.CurrentEnemy.GetComponent<RectTransform>();

            TextMeshProUGUI t   = pooledPopups.Find(x => !x.gameObject.activeInHierarchy);

            if (t == null)
                t = CreateNewPoolItem();

            Vector2 spawnPoint = new Vector2(
                                    uTran.anchoredPosition.x + ((uTran.rect.size.x / 2f) * Random.Range(-.75f, .75f))
                                    , uTran.anchoredPosition.y + (uTran.rect.size.y * 1.1f)
                                );

            RectTransform tTran = t.rectTransform;

            t.gameObject.SetActive(true);
            t.font                              = UniversalInspectorVariables.instance.KGHappySolid;
            t.fontSize                          = 40f;
            t.color                             = diff > 0 ? Color.green : Color.red;
            t.text                              = string.Format("{0}{1} MH", diff > 0 ? "+" : "", diff.ToString());
            tTran.SetParent(uTran.parent);
            tTran.localScale                    = Vector3.one;
            tTran.localPosition                 = spawnPoint;

            UIAnimation moveUpAndFade;
            moveUpAndFade                       = UIAnimation.PositionY(tTran, tTran.anchoredPosition.y + 150, 1.2f);
            moveUpAndFade.Play();
            moveUpAndFade                       = UIAnimation.Color(t, Color.clear, 1.2f);
            moveUpAndFade.OnAnimationFinished   = (GameObject obj) => { tTran.SetParent(transform); obj.SetActive(false); };
            moveUpAndFade.Play();
        }
    }

    private void PopupEXP(Signal signal)
    {
        //Signal is object[]
        //info[0]   - Helpful.StatType  - The stat that earned EXP
        //info[1]   - int               - The amount of EXP earned for the action
        //info[2]   - Unit              - The unit that earns the EXP

        canSendNextEXPPopup = false;

        object[] info = signal.GetValueUnsafe<object[]>();

        RectTransform uTran;

        //TODO: This causes an error to be thrown on battle complete if something is queued to popup i think?
        //      I've only seen the error once so haven't fully looked into it
        if ((Unit)info[2] == BattleManager.instance.CurrentPlayerUnit.UnitInfo)
        {
            uTran = BattleManager.instance.CurrentPlayerUnit.GetComponent<RectTransform>();
        }
        else
            return; //TODO: Handle this better, probably with a different signal

        //Get the next unused popup text
        TextMeshProUGUI t = pooledPopups.Find(x => !x.gameObject.activeInHierarchy);

        if (t == null)
        {
            t = CreateNewPoolItem();
        }

        //TODO: Probably better to get the position right than to reparent this but?? how do i get the right position???
        //      this works for now and doesn't seem to have any performance issues if I cache the transform though
        //      probably just need to set the tTrans's anchored position

        Vector2 spawnPoint = new Vector2(
                                        uTran.anchoredPosition.x + ((uTran.rect.size.x / 2f) * Random.Range(-.75f, .75f))
                                        , uTran.anchoredPosition.y + (uTran.rect.size.y * 1.1f)
                                    );

        RectTransform tTran                 = t.rectTransform;

        t.gameObject.SetActive(true);
        t.font                              = UniversalInspectorVariables.instance.KGHappySolid;
        t.fontSize                          = 25f;
        t.color                             = Color.black;
        t.text                              = "+" + ((int)info[1]).ToString() + " " + ((Helpful.StatTypes)info[0]).GetShorthand() + " EXP";
        tTran.SetParent(uTran.parent);
        tTran.localScale                    = Vector3.one;
        tTran.localPosition                 = spawnPoint;

        UIAnimation moveUpAndFade;
        moveUpAndFade                       = UIAnimation.PositionY(tTran, tTran.anchoredPosition.y + 150, 1.2f);
        moveUpAndFade.Play();
        moveUpAndFade                       = UIAnimation.Color(t, Color.clear, 1.2f);
        moveUpAndFade.OnAnimationFinished   = (GameObject obj) => { tTran.SetParent(transform); obj.SetActive(false); };
        moveUpAndFade.Play();

        Invoke("AllowNextEXPPopup", .3f);
    }

    private void PopupLevelUp(Signal signal)
    {
        //Signal is object[]
        //info[0]   - Helpful.StatType  - The stat that Leveled up
        //info[1]   - Unit              - The unit that Leveled up

        showingStatUp = true;

        object[] info = signal.GetValueUnsafe<object[]>();

        RectTransform uTran;

        if ((Unit)info[1] == BattleManager.instance.CurrentPlayerUnit.UnitInfo)
        {
            uTran = BattleManager.instance.CurrentPlayerUnit.GetComponent<RectTransform>();
        }
        else
            return;

        //Get the next unused popup text
        TextMeshProUGUI t = pooledPopups.Find(x => !x.gameObject.activeInHierarchy);

        if (t == null)
        {
            t = CreateNewPoolItem();
        }

        Vector2 spawnPoint = new Vector2(
                                uTran.anchoredPosition.x
                                , uTran.anchoredPosition.y + uTran.rect.size.y + 35f
                            );

        RectTransform tTran                 = t.rectTransform;

        t.gameObject.SetActive(true);
        t.font                              = UniversalInspectorVariables.instance.KGHappy;
        t.fontSize                          = 40f;
        t.color                             = Color.black;
        t.text                              = ((Helpful.StatTypes)info[0]).GetStringValue().ToUpper() + " up!!";
        tTran.SetParent(uTran.parent);
        tTran.localScale                    = Vector3.one;
        tTran.localPosition                 = spawnPoint;

        RainbowColorCycle(t, .3f, delegate { showingStatUp = false; tTran.SetParent(transform); t.gameObject.SetActive(false); });
    }

    private void EnqueueLevelUpPopup(Signal signal)
    {
        statUpPopupQueue.Enqueue(signal);
    }

    private void EnqueueEXPPopup(Signal signal)
    {
        expPopupQueue.Enqueue(signal);
    }

    private void AllowNextEXPPopup()
    {
        canSendNextEXPPopup = true;
    }

    private void StopAll()
    {
        expPopupQueue.Clear();
        statUpPopupQueue.Clear();

        for (int i = 0; i < pooledPopups.Count; i++)
        {
            pooledPopups[i].gameObject.SetActive(false);
        }

        canSendNextEXPPopup = true;
        showingStatUp       = false;
    }

    private TextMeshProUGUI CreateNewPoolItem()
    {
        //TODO: either make a prefab for the popup or set all of the font/color/text size etc in here
        GameObject go = new GameObject(string.Format("Popup Text {0}", (pooledPopups.Count + 1).ToString()));

        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;

        TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();

        pooledPopups.Add(t);

        ContentSizeFitter c = go.AddComponent<ContentSizeFitter>();
        c.horizontalFit     = ContentSizeFitter.FitMode.PreferredSize;
        c.verticalFit       = ContentSizeFitter.FitMode.PreferredSize;

        CanvasGroup can     = go.AddComponent<CanvasGroup>();
        can.blocksRaycasts  = false;
        can.interactable    = false;

        return t;
    }

    private void RainbowColorCycle(Graphic g, float t, System.Action<UnityEngine.GameObject> onEnd)
    {
        UIAnimation cycle;
        cycle = UIAnimation.Color(g, Color.red, t);
        cycle.OnAnimationFinished =
            delegate {
                cycle = UIAnimation.Color(g, new Color(255, 127, 0), t);
                cycle.OnAnimationFinished =
                    delegate {
                        cycle = UIAnimation.Color(g, Color.yellow, t);
                        cycle.OnAnimationFinished =
                            delegate {
                                cycle = UIAnimation.Color(g, Color.green, t);
                                cycle.OnAnimationFinished =
                                    delegate {
                                        cycle = UIAnimation.Color(g, Color.blue, t);
                                        cycle.OnAnimationFinished =
                                            delegate {
                                                cycle = UIAnimation.Color(g, new Color(75, 0, 130), t);
                                                cycle.OnAnimationFinished =
                                                    delegate {
                                                        cycle = UIAnimation.Color(g, new Color(127, 0, 255), t);
                                                        cycle.OnAnimationFinished = delegate { onEnd?.Invoke(g.gameObject); } ;
                                                        cycle.Play();
                                                    };
                                                cycle.Play();
                                            };
                                        cycle.Play();
                                    };
                                cycle.Play();
                            };
                        cycle.Play();
                    };
                cycle.Play();
            };
        cycle.Play();
    }

    #endregion
}
