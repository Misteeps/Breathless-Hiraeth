using System;

using UnityEngine;

using Simplex;


namespace Game
{
    public class Normal1 : Ability
    {
        public override async void Cast()
        {
            Aiming = false;

            await GeneralUtilities.DelayMS(1000);

            Destroy();
        }
    }
}