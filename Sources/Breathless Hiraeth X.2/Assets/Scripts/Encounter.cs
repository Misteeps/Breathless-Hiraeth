using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

using Simplex;


namespace Game
{
    public class Encounter : MonoBehaviour
    {
        public enum Status { Peaceful, Ambush, Patrol, Notice, Attack, Cleared }

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

        public Status startState;
        public float aggroTime;
        public float chaseRangeScale;
        public Wave[] waves = new Wave[0];
        public List<Monster> monsters = new List<Monster>(0);

        private float timer;
        private Status state;

        public float Range { get => trigger.radius; set => trigger.radius = value; }
        public float ChaseRange => Range * chaseRangeScale;
        public int CurrentWaveIndex { get; private set; }
        public Wave CurrentWave { get; private set; }
        public Status State
        {
            get => state;
            set
            {
                state = value;
                switch (State)
                {
                    case Status.Peaceful:
                        trigger.enabled = false;
                        enabled = true;
                        timer = 2;
                        Spawn(0);
                        foreach (Monster monster in monsters)
                            if (Vector3.Distance(transform.position, monster.transform.position) > Range)
                                monster.Move(RandomPoint(), RNG.Generic.Float(0.6f, 1f));
                            else
                                monster.Move(monster.transform.position, 1);
                        break;

                    case Status.Ambush:
                        trigger.enabled = true;
                        enabled = false;
                        break;

                    case Status.Patrol:
                        trigger.enabled = true;
                        enabled = true;
                        timer = 2;
                        Spawn(0);
                        if (!monsters.IsEmpty())
                            for (int i = 0; i < RNG.Generic.Int(1, monsters.Count); i++)
                            {
                                Monster monster = monsters[i];
                                if (Vector3.Distance(transform.position, monster.transform.position) > Range)
                                    monster.Move(RandomPoint(), RNG.Generic.Float(0.6f, 1f));
                                else
                                    monster.Move(monster.transform.position, 1);
                            }
                        break;

                    case Status.Notice:
                        trigger.enabled = false;
                        enabled = true;
                        timer = aggroTime;
                        foreach (Monster monster in monsters)
                            if (RNG.Generic.Bool())
                                monster.Move(Vector3.Lerp(monster.transform.position, Monolith.Player.transform.position, 0.2f), 0.5f);
                        break;

                    case Status.Attack:
                        trigger.enabled = false;
                        enabled = true;
                        timer = CurrentWave?.duration ?? 0;
                        Spawn(0);
                        break;

                    case Status.Cleared:
                        gameObject.SetActive(false);
                        trigger.enabled = false;
                        enabled = false;
                        // Reward
                        break;
                }
            }
        }


        private void Start() => Restart();
        private void Update()
        {
            switch (State)
            {
                case Status.Ambush:
                case Status.Cleared:
                default: break;

                case Status.Peaceful:
                case Status.Patrol:
                    timer -= Time.deltaTime;
                    if (timer < 0)
                    {
                        if (!monsters.IsEmpty())
                        {
                            timer = RNG.Generic.Float(0, 4);
                            RNG.Generic.From(monsters).Move(RandomPoint(), RNG.Generic.Float(0.1f, 0.4f));
                        }
                    }
                    break;

                case Status.Notice:
                    if (Vector3.Distance(Monolith.Player.transform.position, transform.position) > Range + 2) State = (CurrentWave == null) ? Status.Ambush : Status.Patrol;
                    else
                    {
                        timer -= Time.deltaTime;
                        if (timer < 0) State = Status.Attack;
                    }
                    break;

                case Status.Attack:
                    foreach (Monster monster in monsters)
                        try
                        {
                            monster.Move(Monolith.Player.transform.position);
                            if (Vector3.Distance(monster.transform.position, Monolith.Player.transform.position) < 2)
                                monster.Attack();
                        }
                        catch (Exception exception) { exception.Error($"Failed updating monster in encounter"); }

                    if (Vector3.Distance(Monolith.Player.transform.position, transform.position) > ChaseRange) State = Status.Patrol;
                    else if (CurrentWaveIndex < waves.Length)
                    {
                        timer -= Time.deltaTime;
                        if (timer < 0 || monsters.Count <= CurrentWave.threshold)
                            Spawn(CurrentWaveIndex + 1);
                    }
                    else if (monsters.Count == 0)
                        State = Status.Cleared;
                    break;
            }
        }

        public void Restart()
        {
            try
            {
                while (transform.childCount > 0)
                    Destroy(transform.GetChild(0).gameObject);
            }
            catch (Exception exception) { exception.Error($"Failed restarting encounter"); }

            timer = 0;
            monsters = new List<Monster>();
            CurrentWaveIndex = -1;
            CurrentWave = null;
            State = startState;
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
                            Monster monster = Instantiate(type.prefab, transform).GetComponent<Monster>();
                            monster.transform.position = RandomPoint();
                            monster.encounter = this;
                            monsters.Add(monster);
                        }
                    }
                    catch (Exception exception) { exception.Error($"Failed spawning monsters in wave {CurrentWaveIndex:info}"); }
            }
        }

        private Vector3 RandomPoint()
        {
            Vector3 point = UnityEngine.Random.insideUnitSphere * Range + transform.position;
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, Range * 4, NavMesh.AllAreas))
                return hit.position;

            ConsoleUtilities.Warn($"Encounter could not get random point with {point:info}");
            return transform.position;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, ChaseRange);
        }
    }
}