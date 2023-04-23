using System;

using UnityEngine;

using Simplex;


namespace Game
{
    public class Super3 : Ability
    {
        [SerializeField] private ParticleSystem explosion;
        [SerializeField] private GameObject lightningPrefab;


        public override async void Cast()
        {
            aimDecal.enabled = false;

            Monolith.Player.animator.CrossFade("Ability Super 3", 0.1f);
            Monolith.Player.voidImplosionLarge.Play();
            await GeneralUtilities.DelayMS(800);
            Monolith.Player.Breathing = false;
            Monolith.Player.gameObject.SetActive(false);

            Vector3 start = Monolith.Player.transform.position;
            Vector3 end = transform.position;

            int ticks = (Mathf.RoundToInt(Vector3.Distance(start, end) / 5));
            int delay = (Mathf.RoundToInt(800f / ticks));
            for (int tick = 0; tick < ticks; tick++)
            {
                Vector3 position = Vector3.Lerp(start, end, Mathf.InverseLerp(0, ticks, tick));
                if (Physics.Raycast(position + new Vector3(0, 500, 0), Vector3.down, out RaycastHit hit, 1000, 3145728))
                    position = hit.point;
                LightningStrike(position, tick * delay);
            }

            await GeneralUtilities.DelayMS(200);

            new Transition(() => 0, value => Monolith.Player.transform.position = Vector3.Lerp(start, end, value), 0, 1, "Teleport Player").Curve(Function.Quadratic, Direction.InOut, 880).Start();
            await GeneralUtilities.DelayMS(800);

            Monolith.Player.transform.position = transform.position;
            Monolith.Player.gameObject.SetActive(true);
            Monolith.Player.Combat = !Inputs.Sprint.Held;
            explosion.Play();

            for (int i = 0; i < Monolith.encounters.Length; i++)
            {
                Encounter encounter = Monolith.encounters[i];
                if (!encounter.gameObject.activeSelf || Vector3.Distance(transform.position, encounter.transform.position) > encounter.ChaseRange) continue;
                foreach (Monster monster in encounter.monsters)
                    if (Vector3.Distance(transform.position, monster.transform.position) < 3)
                        monster.TakeDamage(20 + (Progress.magic * 10));
            }

            await GeneralUtilities.DelayMS(2000);

            Destroy();
        }

        private async void LightningStrike(Vector3 position, int delay)
        {
            await GeneralUtilities.DelayMS(delay);
            ParticleSystem lightning = Instantiate(lightningPrefab, position, new Quaternion(), transform).GetComponent<ParticleSystem>();
            lightning.Play();
            await GeneralUtilities.DelayMS(300);
            for (int i = 0; i < Monolith.encounters.Length; i++)
            {
                Encounter encounter = Monolith.encounters[i];
                if (!encounter.gameObject.activeSelf || Vector3.Distance(position, encounter.transform.position) > encounter.ChaseRange) continue;
                foreach (Monster monster in encounter.monsters)
                    if (Vector3.Distance(position, monster.transform.position) < 4)
                        monster.TakeDamage(10 + (Progress.magic * 8));
            }
        }
    }
}