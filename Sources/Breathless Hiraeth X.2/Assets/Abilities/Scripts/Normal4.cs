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
            Aiming = false;

            Monolith.Player.swordSummon.Play();
            await GeneralUtilities.DelayMS(400);

            var summonMain = Monolith.Player.swordSummon.main;
            var rainMain = rain.main;
            summonMain.loop = true;
            rainMain.loop = true;

            rain.Play();
            for (int tick = 0; tick < 25; tick++)
            {
                await GeneralUtilities.DelayMS(200);
                for (int i = 0; i < Monolith.encounters.Length; i++)
                {
                    Encounter encounter = Monolith.encounters[i];
                    if (!encounter.gameObject.activeSelf || Vector3.Distance(transform.position, encounter.transform.position) > encounter.ChaseRange) continue;
                    foreach (Monster monster in encounter.monsters)
                        if (Vector3.Distance(transform.position, monster.transform.position) < 7)
                            monster.TakeDamage(1 + Progress.magic);
                }

                if (tick > 36) summonMain.loop = false;
                if (tick > 42) rainMain.loop = false;
            }

            await GeneralUtilities.DelayMS(1500);

            Destroy();
        }
    }
}