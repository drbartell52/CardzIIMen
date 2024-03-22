using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Gameboard.Persistance;
using UnityEngine.UI;

namespace Gameboard.Examples
{
    public class PlayerBehavior : MonoBehaviour
    {
        public string PlayerName;
        public int Hitpoints;
        public Text LabelText;

        private Player mPlayer;

        void Awake()
        {
            mPlayer = new Player(PlayerName, Hitpoints);
            // Register the players as a stateful element to be tracked.
            GameCaretaker.GetInstance().RegisterOriginator(mPlayer);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            LabelText.text = mPlayer.GetName() + ": " + mPlayer.GetHitpoints();
        }

        public void HarmPlayer()
        {
            HarmCommand command = new HarmCommand(mPlayer, 10);
            GameCaretaker.GetInstance().Execute(command);
        }

        public void HealPlayer()
        {
            HealCommand command = new HealCommand(mPlayer, 10);
            GameCaretaker.GetInstance().Execute(command);
        }
    }
}
