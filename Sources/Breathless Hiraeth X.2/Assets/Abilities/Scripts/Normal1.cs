using System;

using UnityEngine;
using UnityEngine.AI;

using Simplex;


namespace Game
{
    public interface IBramble
    {
        public static IBramble Active;
        public bool Speeding { get; set; }
    }

    public class Normal1 : Ability, IBramble
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

            if (IBramble.Active != null)
                IBramble.Active.Speeding = false;

            IBramble.Active = this;
            brambles = new ParticleSystem[10];
            Speeding = true;

            SpawnBrambles();
            DestroyBrambles(6000);

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

            DestroyBrambles(0);
            await GeneralUtilities.DelayMS(2000);

            Destroy();
        }

        private async void SpawnBramble(int index, Vector3 position)
        {
            if (Physics.Raycast(position + new Vector3(0, 500, 0), Vector3.down, out RaycastHit hit, 1000, 3145728))
                position = hit.point;

            ParticleSystem bramble = Instantiate(bramblePrefab, position, new Quaternion(), transform).GetComponent<ParticleSystem>();
            brambles[index] = bramble;
            bramble.Play();

            NavMeshObstacle obstacle = bramble.GetComponent<NavMeshObstacle>();
            new Transition(() => 0, value => obstacle.radius = value, 0, 2.4f).Curve(Function.Quadratic, Direction.Out, 2f).Start();
            obstacle.carving = true;

            if (index % 3 == 0)
                bramble.GetComponent<AudioSource>().Play();

            await GeneralUtilities.DelayMS(2000);
            bramble.Pause();
        }
        private async void SpawnBrambles()
        {
            for (int i = 0; i < brambles.Length; i++)
            {
                SpawnBramble(i, transform.position + (transform.forward * (4 * i)));
                await GeneralUtilities.DelayMS(200);
            }
        }

        private void DestroyBramble(int index, ParticleSystem bramble)
        {
            if (bramble == null) return;
            brambles[index] = null;
            bramble.Play();

            NavMeshObstacle obstacle = bramble.GetComponent<NavMeshObstacle>();
            obstacle.carving = false;
        }
        private async void DestroyBrambles(int milliseconds)
        {
            await GeneralUtilities.DelayMS(milliseconds);

            for (int i = 0; i < brambles.Length; i++)
            {
                DestroyBramble(i, brambles[i]);
                await GeneralUtilities.DelayMS(200);
            }

            Speeding = false;
        }
    }
}