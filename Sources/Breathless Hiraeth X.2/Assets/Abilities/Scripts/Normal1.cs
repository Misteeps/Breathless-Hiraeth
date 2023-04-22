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
            Monolith.Player.animator.CrossFade("Ability Normal 1", 0.1f);

            await GeneralUtilities.DelayMS(1000);

            Destroy();
        }
    }
}