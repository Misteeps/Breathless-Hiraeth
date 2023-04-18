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
        public Wave[] waves = new Wave[0];
        public List<Monster> monsters = new List<Monster>(0);

        private int currentWave;
        public Wave CurrentWave => waves[currentWave];

        public float Range { get => trigger.radius; set => trigger.radius = value; }
    }
}