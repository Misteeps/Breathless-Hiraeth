using System;

using UnityEngine;

using Simplex;


namespace Game
{
    public class Normal3 : Ability
    {
        [SerializeField] private ParticleSystem explosion;


        public override async void Cast()
        {
            aimDecal.enabled = false;

            Monolith.Player.voidImplosionSmall.Play();
            await GeneralUtilities.DelayMS(1000);
            Debug.Log("Disappear");
            await GeneralUtilities.DelayMS(600);
            explosion.Play();
            await GeneralUtilities.DelayMS(2000);

            Destroy();
        }
    }
}