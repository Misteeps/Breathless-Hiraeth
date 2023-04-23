using System;

using UnityEngine;

using Simplex;


namespace Game
{
    public class Super4 : Ability
    {
        [SerializeField] private ParticleSystem explosion;
        [SerializeField] private ParticleSystem dome;


        public override void Aim()
        {
            transform.position = Monolith.Player.transform.position;
            transform.rotation = Monolith.Player.transform.rotation;
        }
        public override async void Cast()
        {
            aimDecal.enabled = false;
            Monolith.Player.animator.CrossFade("Ability Normal 4", 0.1f);

            await GeneralUtilities.DelayMS(8000);

            Destroy();
        }
    }
}