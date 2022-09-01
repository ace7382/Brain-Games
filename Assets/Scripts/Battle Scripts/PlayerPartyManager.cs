using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;
using System.Linq;

public class PlayerPartyManager : MonoBehaviour
{
    #region Singleton

    public static PlayerPartyManager    instance = null;

    #endregion

    #region Inspector Variables

    public List<Unit>                   partyBattleUnits;

    #endregion

    #region Private Variables

    private Dictionary<Item, int>       partyItems;

    #endregion

    #region Signal Variables

    private SignalReceiver              partymanagement_awardexperience_receiver;
    private SignalStream                partymanagement_awardexperience_stream;

    #endregion

    #region Public Properties

    public Dictionary<Item, int>        PartyItems          { get { return partyItems; } }
    public List<Unit>                   InjuredPartyMembers { get { return partyBattleUnits.Where(x => x.GetStat(Helpful.StatTypes.MaxHP) != x.CurrentHP).ToList(); } }
    public List<Unit>                   KOedPartyMembers    { get { return partyBattleUnits.Where(x => x.CurrentHP == 0).ToList(); } }
    public List<Unit>                   AlivePartyMembers   { get { return partyBattleUnits.Where(x => x.CurrentHP > 0).ToList(); } }
    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        partymanagement_awardexperience_stream      = SignalStream.Get("PartyManagement", "AwardExperience");

        partymanagement_awardexperience_receiver    = new SignalReceiver().SetOnSignalCallback(ProcessEXP);

        partyItems                                  = new Dictionary<Item, int>();
    }

    private void OnEnable()
    {
        partymanagement_awardexperience_stream.ConnectReceiver(partymanagement_awardexperience_receiver);
    }

    private void OnDisable()
    {
        partymanagement_awardexperience_stream.DisconnectReceiver(partymanagement_awardexperience_receiver);
    }

    private void Start()
    {
        for (int i = 0; i < partyBattleUnits.Count; i++)
        {
            partyBattleUnits[i].Init();

            //TODO: Remove this vvvvvv

            object[] data = new object[3];
            data[0] = Helpful.StatTypes.Level;
            data[1] = Random.Range(25, 200);
            data[2] = partyBattleUnits[i];

            Signal.Send("PartyManagement", "AwardExperience", data);

            //^^^^^^^^^^^^^^^^^^^^^^^^
        }

        //TODO - Remove, make "always in inventory" items put 0 in at the start
        List<Item> c = new List<Item>(Resources.LoadAll<Item>("Scriptable Objects/Items"));

        for (int i = 0; i < c.Count; i++)
        {
            AddItemToInventory(c[i], Random.Range(1, 3));
        }
        //^^^^^^^^
    }

    #endregion

    #region Public Functions

    public Unit GetFirstLivingUnit()
    {
        return partyBattleUnits.Find(x => x.CurrentHP > 0);
    }

    public void AddItemToInventory(Item it, int count)
    {
        if (partyItems.ContainsKey(it))
            partyItems[it] += count;
        else
            partyItems.Add(it, count);
    }

    public void RemoveItemFromInventory(Item it, int count)
    {
        if (partyItems.ContainsKey(it))
        {
            if (partyItems[it] >= count)
            {
                partyItems[it] -= count;

                if (partyItems[it] <= 0 && !it.AlwaysInInventory)
                    partyItems.Remove(it);
            }
        }
    }

    public int GetInventoryCount(Item i)
    {
        if (PartyItems.ContainsKey(i))
            return PartyItems[i];

        return 0;
    }

    #endregion

    #region Private Functions

    private void ProcessEXP(Signal signal)
    {
        //Signal is object[]
        //info[0]   - Helpful.StatType  - The stat that earned EXP
        //info[1]   - int               - The amount of EXP earned for the action
        //info[2]   - Unit              - The unit that earns the EXP

        object[] info = signal.GetValueUnsafe<object[]>();

        //TODO: Request Modifiers
        //TODO: Store and Process MOdifiers

        partyBattleUnits.Find(x => x == (Unit)info[2]).AddEXP((Helpful.StatTypes)info[0], (int)info[1]);
    }

    #endregion
}
