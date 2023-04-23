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
            aimDecal.enabled = false;
            Monolith.Player.animator.CrossFade("Ability Normal 2", 0.1f);

            explosion.Play();
            await GeneralUtilities.DelayMS(1360);

            for (int i = 0; i < Monolith.encounters.Length; i++)
            {
                Encounter encounter = Monolith.encounters[i];
                if (!encounter.gameObject.activeSelf || Vector3.Distance(transform.position, encounter.transform.position) > encounter.ChaseRange) continue;
                foreach (Monster monster in encounter.monsters)
                    if (Vector3.Distance(transform.position, monster.transform.position) < 6)
                        monster.TakeDamage(40 + (Progress.magic * 15));
            }

            await GeneralUtilities.DelayMS(2000);

            Destroy();
        }
    }
}