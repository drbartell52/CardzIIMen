using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Gameboard.Persistance;

namespace Gameboard.Examples
{
    public class HealCommand : ACommand
    {
        private Player mPlayer;
        private int mAmount;

        public HealCommand(Player target, int amount)
        {
            mPlayer = target;
            mAmount = amount;
        }

        override
        public void ExecuteImpl()
        {
            mPlayer.AddHitpoints(mAmount);
        }

        override
        public void UnExecuteImpl()
        {
            mPlayer.SubtractHitpoints(mAmount);
        }
    }
}