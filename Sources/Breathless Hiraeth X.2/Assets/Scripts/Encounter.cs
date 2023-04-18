using System;

using UnityEngine;
using UnityEngine.AI;

using Simplex;


namespace Game
{
    public class Encounter : MonoBehaviour
    {
        #region Wave
        [Serializable]
        public class Wave
        {

        }
        #endregion Wave


        public SphereCollider trigger;

        public bool patrol;
        public Wave[] waves;

        public float Range { get => trigger.radius; set => trigger.radius = value; }

        private int currentWave;
        public Wave CurrentWave => waves[currentWave];
        public int MonsterCount { get; private set; }
    }
}