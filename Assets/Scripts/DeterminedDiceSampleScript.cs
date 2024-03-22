using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameboard.Tools.Dice;
using UnityEngine.UI;
using Gameboard;

public class DeterminedDiceSampleScript : MonoBehaviour
{
    public GameboardDiceManager diceManager;
    private Vector3 initPosition;
    private Rigidbody rb;

    //if you want to roll individual dice, instead of using the dice manager for multiple dice
    // public GameboardPredeterminedDice diceScript;
    // public int diceValue; 

    void Start()
    {
        //For individual dice
        // diceScript.BeforeDiceThrow += Reset;
        // diceScript.ThrowDice += ThrowDice;

        //When using the dice manager to throw multiple dice
        diceManager.BeforeDiceThrow += Reset;
        diceManager.ThrowMultipleDice += ThrowDice;

        initPosition = transform.position;

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // wait to roll until pressing space
    }

    //For individual dice
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space))
    //     {
    //         diceScript.RollDeterminedValue(deiceValue);
    //     }
    // }

    void ThrowDice()
    {
        var xTorque = Random.Range(0, 100);
        var yTorque = Random.Range(0, 100);
        var zTorque = Random.Range(0, 100);

        rb.useGravity = true;
        rb.AddTorque(xTorque, yTorque, zTorque);
    }

    void Reset()
    {
        GameboardLogging.Warning($"Resetting position to: {initPosition}");
        transform.position = initPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (rb.IsSleeping() && transform.hasChanged)
        {
            rb.useGravity = false;
            transform.hasChanged = false;
        }
    }
}
