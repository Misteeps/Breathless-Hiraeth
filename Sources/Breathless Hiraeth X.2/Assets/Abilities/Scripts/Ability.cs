using System;

using UnityEngine;
using UnityEngine.Rendering.Universal;

using Simplex;


namespace Game
{
    public abstract class Ability : MonoBehaviour
    {
        public DecalProjector aimDecal;

        public virtual void Aim()
        {
            Ray ray = Monolith.Camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 100, 3145728)) aimDecal.enabled = false;
            else
            {
                transform.position = hit.point;
                aimDecal.enabled = true;
            }
        }
        public virtual void Destroy()
        {
            aimDecal.enabled = false;
            Destroy(gameObject);
        }
        public abstract void Cast();
    }
}