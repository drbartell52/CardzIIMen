using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Gameboard.Persistance;

namespace Gameboard.Examples
{
    public class Player : IOriginator
    {
        private string mName;
        private int mHitpoints;
        public Action<int> OnHitpointChange;

        public Player(string name, int hitpoints)
        {
            mName = name;
            mHitpoints = hitpoints;
        }

        public void AddHitpoints(int delta)
        {
            mHitpoints += delta;
            OnHitpointChange?.Invoke(mHitpoints);
        }

        public void SubtractHitpoints(int delta)
        {
            mHitpoints -= delta;
            mHitpoints = Mathf.Max(mHitpoints, 0);
            OnHitpointChange?.Invoke(mHitpoints);
        }

        public string GetName()
        {
            return mName;
        }

        public int GetHitpoints()
        {
            return mHitpoints;
        }

        public void UpdateName(string name)
        {
            mName = name;
        }

        public IMemento CreateMemento()
        {
            return new PlayerMemento(mName, mHitpoints);
        }

        public void SetMemento(IMemento memento)
        {
            if (!(memento is PlayerMemento))
            {
                return;
            }

            PlayerMemento playerMemento = memento as PlayerMemento;

            mHitpoints = playerMemento.GetHitpoints();
            mName = playerMemento.GetName();
        }

        public void Clear()
        {
            // Handle any clear needed.
        }

        private class PlayerMemento : IMemento
        {
            private int mHitpoints;
            private string mName;

            public PlayerMemento(string name, int hitpoints)
            {
                mHitpoints = hitpoints;
                mName = name;
            }

            public int GetHitpoints()
            {
                return mHitpoints;
            }

            public string GetName()
            {
                return mName;
            }
        }
    }
}