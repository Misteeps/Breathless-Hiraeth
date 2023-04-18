using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

using Simplex;


namespace Game
{
    public class Encounter : MonoBehaviour
    {
        #region Type
        [Serializable]
        public class Type
        {
            public GameObject prefab;
            public int count;

#if UNITY_EDITOR
            public Encounter encounter;
            public Type(Encounter encounter)
            {
                this.encounter = encounter;
                count = 5;
            }
#endif
        }
        #endregion Type
        #region Wave
        [Serializable]
        public class Wave
        {
            public Type[] types;
            public int threshold;
            public float duration;

#if UNITY_EDITOR
            public Encounter encounter;
            public Wave(Encounter encounter)
            {
                this.encounter = encounter;
                types = new Type[0];
                threshold = 10;
                duration = 30;
            }
#endif
        }
        #endregion Wave


        public SphereCollider trigger;

        public bool patrol;
        public float aggroTime;
        public Wave[] waves = new Wave[0];
        public List<Monster> monsters = new List<Monster>(0);

        private float timer;

        public float Range { get => trigger.radius; set => trigger.radius = value; }
        public int CurrentWaveIndex { get; private set; }
        public Wave CurrentWave { get; private set; }


        private void Start() => Clear();
        private void Update()
        {
            foreach (Monster monster in monsters)
                try { monster.Attack(Monolith.Player.transform.position); }
                catch (Exception exception) { exception.Error($"Failed updating monster in encounter"); }

            if (CurrentWaveIndex < waves.Length)
            {
                timer -= Time.deltaTime;
                if (timer < 0 || monsters.Count <= CurrentWave.threshold)
                    Spawn(CurrentWaveIndex + 1);
            }
            else if (monsters.Count == 0)
            {
                enabled = false;
                gameObject.SetActive(false);
                // Reward?
            }
        }

        public void Clear()
        {
            try
            {
                while (transform.childCount > 0)
                    Destroy(transform.GetChild(0).gameObject);
            }
            catch (Exception exception) { exception.Error($"Failed clearing encounter of all monsters"); }

            monsters = new List<Monster>();
            timer = 0;
            CurrentWave = null;
            CurrentWaveIndex = -1;
            if (patrol) Spawn(0);

            trigger.enabled = true;
            enabled = false;
        }
        public void Spawn(int wave)
        {
            if (wave <= CurrentWaveIndex) return;
            if (wave != CurrentWaveIndex + 1) ConsoleUtilities.Warn($"Encounter skipped waves! {CurrentWaveIndex:info} > {wave:info}");
            CurrentWaveIndex = wave;

            if (!waves.OutOfRange(CurrentWaveIndex))
            {
                CurrentWave = waves[CurrentWaveIndex];
                timer = CurrentWave.duration;

                foreach (Type type in CurrentWave.types)
                    try
                    {
                        for (int i = 0; i < type.count; i++)
                        {
                            Vector3 point = UnityEngine.Random.insideUnitSphere * Range + transform.position;
                            if (!NavMesh.SamplePosition(point, out NavMeshHit hit, Range * 4, NavMesh.AllAreas))
                            {
                                ConsoleUtilities.Warn($"Encounter could not spawn enemy at {point:info} in wave {CurrentWaveIndex:info}");
                                continue;
                            }

                            Monster monster = Instantiate(type.prefab, transform).GetComponent<Monster>();
                            monster.transform.position = hit.position;
                            monsters.Add(monster);
                        }
                    }
                    catch (Exception exception) { exception.Error($"Failed spawning monsters in wave {CurrentWaveIndex:info}"); }
            }
        }
    }
}