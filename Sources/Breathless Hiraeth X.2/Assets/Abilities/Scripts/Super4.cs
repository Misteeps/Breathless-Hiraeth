using System;

using UnityEngine;

using Simplex;


namespace Game
{
    public class Super4 : Ability
    {
        [SerializeField] private ParticleSystem explosion;
        [SerializeField] private ParticleSystem dome;
        [SerializeField] private ParticleSystem rain;

        [Header("Audio")]
        [SerializeField] private AudioSource castAudio;
        [SerializeField] private AudioSource rainAudio;


        public override void Aim()
        {
            transform.position = Monolith.Player.transform.position;
            transform.rotation = Monolith.Player.transform.rotation;
        }
        public override async void Cast()
        {
            Monolith.Player.castingSuper = true;
            aimDecal.enabled = false;

            castAudio.Play();
            Monolith.Player.animator.CrossFade("Ability Super 4", 0.1f);
            await GeneralUtilities.DelayMS(1400);
            Monolith.Player.celestialBreath.Play();
            await GeneralUtilities.DelayMS(400);

            Monolith.Player.celestialBurst.Play();
            explosion.Play();
            dome.Play();
            Monolith.Player.Breathing = false;
            await GeneralUtilities.DelayMS(200);

            rainAudio.Play();
            rainAudio.Transition(AudioSourceField.Volume, Unit.X, 0, 0.5f).Curve(Function.Linear, Direction.In, 800).Start();
            var rainMain = rain.main;
            rainMain.loop = true;
            rain.Play();
            dome.Pause();

            int damage = Mathf.RoundToInt(1 + (Progress.magic / 2));
            for (int tick = 0; tick < 40; tick++)
            {
                await GeneralUtilities.DelayMS(200);
                for (int i = 0; i < Monolith.encounters.Length; i++)
                {
                    Encounter encounter = Monolith.encounters[i];
                    if (!encounter.gameObject.activeSelf || Vector3.Distance(transform.position, encounter.transform.position) > encounter.ChaseRange) continue;
                    foreach (Monster monster in encounter.monsters)
                        if (Vector3.Distance(transform.position, monster.transform.position) < 24)
                            monster.TakeDamage(damage);
                }

                if (tick > 35) rainMain.loop = false;
            }

            dome.Play();
            rainAudio.Transition(AudioSourceField.Volume, Unit.X, 0.5f, 0).Curve(Function.Linear, Direction.In, 800).Start();
            await GeneralUtilities.DelayMS(2000);

            Destroy();
        }
    }
}