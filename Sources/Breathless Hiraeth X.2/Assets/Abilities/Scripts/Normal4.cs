using System;

using UnityEngine;

using Simplex;


namespace Game
{
    public class Normal4 : Ability
    {
        [SerializeField] private ParticleSystem rain;


        public override async void Cast()
        {
            aimDecal.enabled = false;

            Monolith.Player.swordSummon.Play();
            await GeneralUtilities.DelayMS(400);
            rain.Play();
            await GeneralUtilities.DelayMS(3000);

            Destroy();
        }
    }
}