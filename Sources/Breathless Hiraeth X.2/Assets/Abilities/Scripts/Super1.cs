using System;

using UnityEngine;
using UnityEngine.AI;

using Simplex;


namespace Game
{
    public class Super1 : Ability, IBramble
    {
        [SerializeField] private GameObject decals;
        [SerializeField] private GameObject bramblePrefab;
        private ParticleSystem[] brambles;
        public bool Speeding { get; set; }


        public override void Aim()
        {
            transform.position = Monolith.Player.transform.position;
            transform.rotation = Monolith.Player.transform.rotation;
        }
        public override void Destroy()
        {
            decals.SetActive(false);
            Destroy(gameObject);
        }
        public override async void Cast()
        {
            aimDecal.enabled = false;
            decals.SetActive(false);

            Monolith.Player.animator.CrossFade("Ability Super 1", 0.1f);
            await GeneralUtilities.DelayMS(1400);
            Monolith.Player.natureBoost.Play();
            await GeneralUtilities.DelayMS(400);
            Monolith.Player.Breathing = false;

            if (IBramble.Active != null)
                IBramble.Active.Speeding = false;

            IBramble.Active = this;
            brambles = new ParticleSystem[40];
            Speeding = true;

            SpawnBrambles(0, 0);
            SpawnBrambles(5, 45);
            SpawnBrambles(10, 90);
            SpawnBrambles(15, 135);
            SpawnBrambles(20, 180);
            SpawnBrambles(25, 225);
            SpawnBrambles(30, 270);
            SpawnBrambles(35, 315);

            DestroyBrambles(0, 8000);
            DestroyBrambles(5, 8000);
            DestroyBrambles(10, 8000);
            DestroyBrambles(15, 8000);
            DestroyBrambles(20, 8000);
            DestroyBrambles(25, 8000);
            DestroyBrambles(30, 8000);
            DestroyBrambles(35, 8000);

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

            DestroyBrambles(0, 0);
            DestroyBrambles(5, 0);
            DestroyBrambles(10, 0);
            DestroyBrambles(15, 0);
            DestroyBrambles(20, 0);
            DestroyBrambles(25, 0);
            DestroyBrambles(30, 0);
            DestroyBrambles(35, 0);

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
            new Transition(() => 0, value => obstacle.radius = value, 0, 2f).Curve(Function.Quadratic, Direction.Out, 2f).Start();

            await GeneralUtilities.DelayMS(2000);
            bramble.Pause();
        }
        private async void SpawnBrambles(int startIndex, int rotation)
        {
            Vector3 direction = Quaternion.Euler(0, rotation, 0) * transform.forward;
            for (int i = startIndex; i < startIndex + 5; i++)
            {
                SpawnBramble(i, transform.position + (direction * (4 * (1 + i - startIndex))));
                await GeneralUtilities.DelayMS(200);
            }
        }

        private void DestroyBramble(int index, ParticleSystem bramble)
        {
            if (bramble == null) return;
            brambles[index] = null;
            bramble.Play();
        }
        private async void DestroyBrambles(int startIndex, int milliseconds)
        {
            await GeneralUtilities.DelayMS(milliseconds);

            for (int i = startIndex; i < startIndex + 5; i++)
            {
                DestroyBramble(i, brambles[i]);
                await GeneralUtilities.DelayMS(200);
            }

            Speeding = false;
        }
    }
}