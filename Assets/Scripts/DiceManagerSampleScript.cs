using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gameboard.Tools.Dice;
using UnityEngine;

public class DiceManagerSampleScript : MonoBehaviour
{
    public GameboardDiceManager diceManager;

    // Update is called once per frame
    public void Roll1s()
    {
        //If they are all the same dice size
        // diceManager.RollMultipleDeterminedValueDice(new int[] { 1, 2, 3, 4, 5, 6 });

        diceManager.RollMultipleDeterminedValueDice(diceManager.dice.Select(die => new MultiDiceRoll()
        {
            face = int.Parse(die.id.Split('_')[1]),
            die = die
        }
        ).ToList());
    }

    public void RollMax()
    {
        diceManager.RollMultipleDeterminedValueDice(diceManager.dice.Select(die => new MultiDiceRoll()
        {
            face = die.sides.Count,
            die = die
        }
        ).ToList());
    }
}
