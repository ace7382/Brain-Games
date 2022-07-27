using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCharger : MonoBehaviour
{
    #region Public Enums

    public enum AbilityChargeActions
    {
        CORRECT_RESPONSE,
        INCORRECT_RESPONSE,
        TILE_ROTATED,
    }

    #endregion

    #region Private Classes

    private class AbilityActionResponse
    {
        public Ability  ability;
        public int      chargeActionID; //Charge Action ID: TODO MAKE ENUM?? 0 - standard charge, 1 - reset //Made int so i can add other responses

        public AbilityActionResponse(Ability ab, int rtype)
        {
            ability         = ab;
            chargeActionID  = rtype;
        }
    }

    #endregion

    #region Singleton

    public static AbilityCharger instance = null; //Does NOT persist between scenes currently though

    #endregion

    #region Inspector Variables

    [SerializeField] private GameObject     abilityChargeParticlePrefab;

    #endregion
    
    #region Private Variables

    private List<AbilityActionResponse>     chargeTargets;

    #endregion

    #region Signal Variables

    private SignalReceiver                  battle_correctresponse_receiver;
    private SignalStream                    battle_correctresponse_stream;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        chargeTargets                       = new List<AbilityActionResponse>();

        battle_correctresponse_stream       = SignalStream.Get("Battle", "AbilityChargeGenerated");

        battle_correctresponse_receiver     = new SignalReceiver().SetOnSignalCallback(GenerateCharge);
    }

    private void OnEnable()
    {
        battle_correctresponse_stream.ConnectReceiver(battle_correctresponse_receiver);
    }

    private void OnDisable()
    {
        battle_correctresponse_stream.DisconnectReceiver(battle_correctresponse_receiver);
    }

    private void OnDestroy()
    {
        OnDisable();
    }

    #endregion

    #region Public Functions

    public void AddChargeTarget(Ability t, int numOfChargesGenerated, int typeOfChargeGenerated)
    {
        //TODO: Maybe check to see if it's already a target?

        for (int i = 0; i < numOfChargesGenerated; i++)
        {
            AbilityActionResponse r = new AbilityActionResponse(t, typeOfChargeGenerated);

            chargeTargets.Add(r);
        }
    }

    #endregion

    #region Private Functions

    private void GenerateCharge(Signal signal)
    {
        //Signal is object[2]
        //info[0]   - AbilityChargeActions  - The type of charge generated
        //info[1]   - Vector2               - the origin point of the charge

        //Game actions will need to send signal with
        //  charge type - correct, incorrect, etc

        object[] info                           = signal.GetValueUnsafe<object[]>();

        AbilityChargeActions chargeGenerated    = (AbilityChargeActions)info[0];
        Vector2 chargePosition                  = (Vector2)info[1];

        Debug.Log(chargePosition);

        //This sends signal - Request Charge
        //  signal info will be charge typetype
        Signal.Send("Battle", "RequestMatchingAbilityChargers", chargeGenerated);

        //  All abilities/ability buttons will need to listen for this request
        //      if the ability has a reason to receive a charge of the requested kind
        //      ability will return itself

        //For each abilty received, find a corresponding button, and generate a charge particle
        for (int i = 0; i < chargeTargets.Count; i++)
        {
            AbilityButtonController tar = BattleManager.instance.PlayerAbilityButtons.Find(x => x.ButtonsAbility == chargeTargets[i].ability);

            if (tar == null)
            {
                tar = BattleManager.instance.EnemyAbilityButtons.Find(x => x.ButtonsAbility == chargeTargets[i].ability);
            }

            if (tar == null)
            {
                Debug.Log(string.Format("Ability: {0} is requesting a charge but cannot be found on a button", chargeTargets[i].ability.abilityName));
                continue;
            }

            RectTransform targetTransform               = (RectTransform)tar.transform;

            //Debug.Log(string.Format("Target Destination for {0} charge: {1}", chargeTargets[i].ability.abilityName, targetTransform.anchoredPosition));

            GameObject particle                         = Instantiate(abilityChargeParticlePrefab, GameObject.Find("Battle Canvas").transform);
            RectTransform tran                          = (RectTransform)particle.transform;
            //tran.anchoredPosition                       = Vector2.zero;
            tran.position                               = chargePosition;
            tran.localScale                             = Vector3.one;

            MoveToTarget mover                          = particle.GetComponent<MoveToTarget>();
            mover.target                                = targetTransform;

            AbilityChargerParticleController control    = particle.GetComponent<AbilityChargerParticleController>();
            control.ability                             = tar.ButtonsAbility;
            control.chargeActionID                      = chargeTargets[i].chargeActionID;
            
            particle.GetComponent<Image>().color        = chargeTargets[i].chargeActionID == 0 ? Color.green : Color.red;
        }

        chargeTargets.Clear();
    }

    #endregion
}
