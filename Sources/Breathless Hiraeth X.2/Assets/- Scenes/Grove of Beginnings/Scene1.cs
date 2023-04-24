using System;

using UnityEngine;

using Simplex;


namespace Game
{
    public class Scene1 : MonoBehaviour
    {
        private int sequence;

        private async void Start()
        {
            sequence = 0;

            await GeneralUtilities.DelayMS(8000);
            UI.Hud.Instance.Tip("Move around with [W][A][S][D]");

            sequence = 1;
        }
        private void Update()
        {
            switch (sequence)
            {
                case 0: return;
                case 1:
                    Monolith.encounters[1].gameObject.SetActive(false);
                    sequence = 2;
                    return;
                case 2:
                    if (Monolith.Pressure > 20) Monolith.Pressure = 20;
                    if (Monolith.fairy.CurrentPosition < 4) return;
                    Monolith.encounters[1].gameObject.SetActive(true);
                    Monolith.Player.Health = Progress.hearts;
                    Progress.abilities = 2;
                    UI.Hud.Instance.UpdateAbilities();
                    UI.Hud.Instance.Banner("Unlocked Abilities\n1: Forest Glide\n2: Inferno Eruption");
                    UI.Hud.Instance.Tip("Use abilities with [1][2][3][4]\nConfirm use with [Left Click]\nCancel with [Right Click]", 8000);
                    sequence = 3;
                    return;
                case 3:
                    if (!Monolith.Player.Breathing) return;
                    UI.Hud.Instance.Tip("Use an ability to end the moment of respite\nThe ability used will be extra powerful", 8000);
                    sequence = 4;
                    return;
            }
        }
    }
}