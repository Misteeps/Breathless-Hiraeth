using System;

using UnityEngine;
using UnityEngine.Rendering.Universal;

using Simplex;


namespace Game
{
    public abstract class Ability : MonoBehaviour
    {
        public static Vector3 VFXOffset => new Vector3(0, 0.15f, 0);

        public DecalProjector aimDecal;
        public bool Aiming
        {
            get => enabled;
            set
            {
                enabled = value;
                aimDecal.enabled = value;
            }
        }


        public virtual void Aim()
        {
            Ray ray = Monolith.Camera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 100, 3145728)) aimDecal.enabled = false;
            else
            {
                transform.position = hit.point + VFXOffset;
                transform.rotation = Monolith.Player.transform.rotation;
            }
        }
        public virtual void Destroy()
        {
            Aiming = false;
            Destroy(gameObject);
        }
        public abstract void Cast();
    }
}