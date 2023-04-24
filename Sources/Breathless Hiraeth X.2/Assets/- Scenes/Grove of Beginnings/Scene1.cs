using System;

using UnityEngine;

using Simplex;


namespace Game
{
    public class Scene0 : MonoBehaviour
    {
        private async void Start()
        {
            await GeneralUtilities.DelayMS(8000);
            UI.Hud.Instance.Tip("Move around with [W][A][S][D]");
        }
    }
}