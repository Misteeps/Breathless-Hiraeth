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

        public string guid = Guid.NewGuid().ToString();
        public string reward;
        public float difficulty;
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
                            monster.Move(RandomPoint(), RNG.Generic.Float(0.8f, 1f));
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
                        foreach (Monster monster in monsters)
                            monster.Move(RandomPoint(), RNG.Generic.Float(0.8f, 1f));
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
                        Monolith.Player.EnterEncounter(this);
                        break;

                    case Status.Cleared:
                        trigger.enabled = false;
                        enabled = true;
                        Reward();
                        break;
                }
            }
        }


        private void Start() => Restart();
        private void Update()
        {
            if (Time.timeScale < 0.1f) return;

            switch (State)
            {
                case Status.Ambush:
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
                    if (!Monolith.Player.gameObject.activeSelf) return;
                    Monolith.Pressure += monsters.Count / Monolith.PressureScale * Time.deltaTime;
                    List<Monster> dead = new List<Monster>();

                    foreach (Monster monster in monsters)
                        try
                        {
                            if (monster.health <= 0) dead.Add(monster);
                            else
                            {
                                monster.Move(Monolith.Player.transform.position);
                                if (Vector3.Distance(monster.transform.position, Monolith.Player.transform.position) < 2)
                                    monster.Attack();
                            }
                        }
                        catch (Exception exception) { exception.Error($"Failed updating monster in encounter"); }

                    foreach (Monster monster in dead)
                        monsters.Remove(monster);

                    if (Vector3.Distance(Monolith.Player.transform.position, transform.position) > ChaseRange)
                    {
                        State = Status.Patrol;
                        Monolith.Player.LeaveEncounter(this);
                    }
                    else if (CurrentWaveIndex < waves.Length)
                    {
                        timer -= Time.deltaTime;
                        if (timer < 0 || monsters.Count <= CurrentWave.threshold)
                            Spawn(CurrentWaveIndex + 1);
                    }
                    else if (monsters.Count == 0)
                    {
                        State = Status.Cleared;
                        Monolith.Player.LeaveEncounter(this);
                    }
                    break;

                case Status.Cleared:
                    if (transform.childCount <= 0)
                    {
                        enabled = false;
                        gameObject.SetActive(false);
                    }
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
                            monster.Bind(this);
                            monsters.Add(monster);
                        }
                    }
                    catch (Exception exception) { exception.Error($"Failed spawning monsters in wave {CurrentWaveIndex:info}"); }
            }
        }
        public async void Reward()
        {
            if (string.IsNullOrEmpty(reward)) return;
            if (Progress.guids.Contains(guid)) return;

            await GeneralUtilities.DelayMS(2000);
            Progress.guids.Add(guid);

            switch (reward)
            {
                case "Heart": Progress.hearts++; Monolith.Player.Health++; Monolith.Player.heartsUpgrade.Play(); UI.Hud.Instance.Banner("Hearts Increased"); break;
                case "Memory": Progress.memories++; Monolith.Player.memoriesUpgrade.Play(); UI.Hud.Instance.Banner("Memory Found"); break;
                case "Damage": Progress.damage++; Monolith.Player.statsUpgrade.Play(); UI.Hud.Instance.Banner("Damage Up"); break;
                case "Magic": Progress.magic++; Monolith.Player.statsUpgrade.Play(); UI.Hud.Instance.Banner("Magic Up"); break;
                case "Speed": Progress.speed++; Monolith.Player.statsUpgrade.Play(); UI.Hud.Instance.Banner("Speed Up"); break;
                case "Cooldown": Progress.cooldown++; Monolith.Player.statsUpgrade.Play(); UI.Hud.Instance.Banner("Cooldowns Decrease"); break;
            }

            Progress.Save();
        }

        private Vector3 RandomPoint()
        {
            Vector3 point = UnityEngine.Random.insideUnitSphere * Range + transform.position;
            if (NavMesh.SamplePosition(point, out NavMeshHit hit, Range * 4, NavMesh.AllAreas))
                return hit.position;

            ConsoleUtilities.Warn($"Encounter could not get random point with {point:info}");
            return transform.position;
        }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, ChaseRange);
        }
#endif
    }
}