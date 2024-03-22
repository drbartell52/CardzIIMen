using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Gameboard;
using Gameboard.EventArgs;
using Gameboard.Tools.Dice;

public class DiceRollerSample : MonoBehaviour
{
    public GameboardDiceRoller roller;
    public Text Results;

    private UserPresenceController userPresenceController;

    void Start()
    {
        GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
        userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
        userPresenceController.OnUserPresence += OnUserPresence;

        roller.DiceRollCompleted += RollComplete;
        roller.DiceRollTotalChanged += RollValueChanged;
    }

    void RollComplete(int result)
    {
        Results.text = $"RollComplete! Total: {result}";
    }

    void RollValueChanged(int result)
    {
        Results.text = $"DiceRollTotalChanged! Total: {result}";
    }

    public void RollDice()
    {
        roller.RollDice(new int[7] { 4, 6, 6, 8, 10, 12, 20 });
    }

    public void ReRollCurrentDice()
    {
        roller.ReRollCurrentDice();
    }

    public void ClearDice()
    {
        roller.ClearDice();
    }

    private void OnUserPresence(GameboardUserPresenceEventArgs userPresence)
    {
        GameboardLogging.Log($"OnUserPresence '{userPresence.changeValue}'");
        switch (userPresence.changeValue)
        {
            case DataTypes.UserPresenceChangeTypes.ADD:
                break;
            case DataTypes.UserPresenceChangeTypes.REMOVE:
                roller.ClearDiceForUser(userPresence.userId);
                break;
            case DataTypes.UserPresenceChangeTypes.CHANGE_POSITION:
            case DataTypes.UserPresenceChangeTypes.CHANGE:
                roller.RollDiceForUser(new int[7] { 4, 6, 6, 8, 10, 12, 20 }, userPresence.userId);
                break;
        }
    }
}
