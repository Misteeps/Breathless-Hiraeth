using System;

using UnityEngine;

using Simplex;


namespace Game
{
    public class Normal3 : Ability
    {
        [SerializeField] private ParticleSystem explosion;

        [Header("Audio")]
        [SerializeField] private AudioClip implosionAudio;
        [SerializeField] private AudioClip explosionAudio;


        public override async void Cast()
        {
            aimDecal.enabled = false;
            Monolith.Player.audio.PlayOneShot(implosionAudio, 0.5f);
            Monolith.Player.animator.CrossFade("Ability Normal 3", 0.1f);

            Monolith.Player.voidImplosionSmall.Play();
            await GeneralUtilities.DelayMS(1000);
            Monolith.Player.gameObject.SetActive(false);

            Vector3 start = Monolith.Player.transform.position;
            Vector3 end = transform.position;
            new Transition(() => 0, value => Monolith.Player.transform.position = Vector3.Lerp(start, end, value), 0, 1, "Teleport Player").Curve(Function.Quadratic, Direction.InOut, 600).Start();
            await GeneralUtilities.DelayMS(600);

            Monolith.Player.transform.position = transform.position;
            Monolith.Player.gameObject.SetActive(true);
            Monolith.Player.Combat = !Inputs.Sprint.Held;
            Monolith.Player.audio.PlayOneShot(explosionAudio, 0.5f);
            explosion.Play();

            for (int i = 0; i < Monolith.encounters.Length; i++)
            {
                Encounter encounter = Monolith.encounters[i];
                if (!encounter.gameObject.activeSelf || Vector3.Distance(transform.position, encounter.transform.position) > encounter.ChaseRange) continue;
                foreach (Monster monster in encounter.monsters)
                    if (Vector3.Distance(transform.position, monster.transform.position) < 3)
                        monster.TakeDamage(10 + (Progress.magic * 8));
            }

            await GeneralUtilities.DelayMS(2000);

            Destroy();
        }
    }
}