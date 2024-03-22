using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Gameboard.Persistance;

namespace Gameboard.Examples
{
    public class HarmCommand : ACommand
    {

        private Player mPlayer;
        private int mAmount;

        public HarmCommand(Player target, int amount)
        {
            mPlayer = target;
            mAmount = amount;
        }

        override
        public void ExecuteImpl()
        {
            mPlayer.SubtractHitpoints(mAmount);
        }

        override
        public void UnExecuteImpl()
        {
            mPlayer.AddHitpoints(mAmount);
        }
    }
}