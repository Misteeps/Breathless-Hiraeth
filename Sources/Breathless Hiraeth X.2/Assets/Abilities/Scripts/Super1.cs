using System;

using UnityEngine;
using UnityEngine.AI;

using Simplex;


namespace Game
{
    public class Super1 : Ability, IBramble
    {
        [SerializeField] private GameObject bramblePrefab;
        private ParticleSystem[] brambles;
        public bool Speeding { get; set; }


        public override void Aim()
        {
            transform.position = Monolith.Player.transform.position;
            transform.rotation = Monolith.Player.transform.rotation;
        }
        public override async void Cast()
        {
            aimDecal.enabled = false;
            Monolith.Player.animator.CrossFade("Ability Normal 1", 0.1f);
            await GeneralUtilities.DelayMS(600);
            Monolith.Player.Breathing = false;

            if (IBramble.Active != null)
                IBramble.Active.Speeding = false;

            IBramble.Active = this;

            SpawnBramble();
            DestroyBramble(6000);

            while (Speeding)
            {
                await GeneralUtilities.DelayFrame(1);
                if (Monolith.Player.lockActions) continue;
                bool inRange = false;
                for (int i = 0; i < brambles.Length; i++)
                {
                    ParticleSystem bramble = brambles[i];
                    if (bramble == null) continue;
                    if (Vector3.Distance(brambles[i].transform.position, Monolith.Player.transform.position) < 4)
                    {
                        inRange = true;
                        break;
                    }
                }
                Monolith.Player.speedModifier = (inRange) ? 2 : 1;
            }

            DestroyBramble(0);
            await GeneralUtilities.DelayMS(2000);

            Destroy();
        }

        private async void SpawnBramble()
        {
            brambles = new ParticleSystem[10];
            Speeding = true;

            for (int i = 0; i < brambles.Length; i++)
            {
                Vector3 position = transform.position + (transform.forward * (4 * i));
                if (Physics.Raycast(position + new Vector3(0, 500, 0), Vector3.down, out RaycastHit hit, 1000, 3145728))
                    position = hit.point;
                ParticleSystem bramble = Instantiate(bramblePrefab, position, new Quaternion(), transform).GetComponent<ParticleSystem>();
                brambles[i] = bramble;
                bramble.Play();
                NavMeshObstacle obstacle = bramble.GetComponent<NavMeshObstacle>();
                new Transition(() => 0, value => obstacle.radius = value, 0, 3.6f).Curve(Function.Quadratic, Direction.Out, 2f).Start();
                await GeneralUtilities.DelayMS(200);
            }

            for (int i = 0; i < brambles.Length; i++)
            {
                brambles[i].Pause();
                await GeneralUtilities.DelayMS(200);
            }
        }
        private async void DestroyBramble(int milliseconds)
        {
            await GeneralUtilities.DelayMS(milliseconds);

            for (int i = 0; i < brambles.Length; i++)
            {

                ParticleSystem bramble = brambles[i];
                if (bramble == null) continue;
                bramble.Play();
                brambles[i] = null;
                await GeneralUtilities.DelayMS(200);
            }

            Speeding = false;
        }
    }
}