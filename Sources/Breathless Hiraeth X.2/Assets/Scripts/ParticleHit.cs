using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Simplex;


namespace Game
{
    public class ParticleHit : MonoBehaviour
    {
        [SerializeField] private new ParticleSystem particleSystem;
        [SerializeField] private GameObject hitPrefab;
        [SerializeField, Range(0, 1)] private float lifespan;


        private void OnParticleCollision(GameObject ground)
        {
            List<ParticleCollisionEvent> collisions = new List<ParticleCollisionEvent>();
            particleSystem.GetCollisionEvents(ground, collisions);
            foreach (ParticleCollisionEvent collision in collisions)
                try
                {
                    GameObject gameObject = Instantiate(hitPrefab, collision.intersection, new Quaternion(), transform);
                    Destroy(gameObject, lifespan);
                }
                catch (Exception exception) { exception.Error($"Failed spawning particle hit from {gameObject:ref}"); }
        }
    }
}