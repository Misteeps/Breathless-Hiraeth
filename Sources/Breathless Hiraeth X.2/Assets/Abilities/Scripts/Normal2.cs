using System;

using UnityEngine;

using Simplex;


namespace Game
{
    public class Normal2 : Ability
    {
        [SerializeField] private ParticleSystem explosion;


        public override async void Cast()
        {
            Aiming = false;

            explosion.Play();
            await GeneralUtilities.DelayMS(1360);
            Debug.Log("Damage");
            await GeneralUtilities.DelayMS(2000);

            Destroy();
        }
    }
}