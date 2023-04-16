using System;

using UnityEngine;

using Simplex;


namespace Game
{
    public static class Progress
    {
        public static string playerScene;
        public static string fairyScene;
        public static int fairyPosition;
        public static int hearts;
        public static int memories;
        public static int abilities;


        public static void Load()
        {
            if (FileUtilities.Load("Save.ini", out FileUtilities.INI ini))
                try
                {
                    playerScene = (string)ini.FindItem("Player Scene").value;
                    fairyScene = (string)ini.FindItem("Fairy Scene").value;
                    fairyPosition = int.Parse((string)ini.FindItem("Fairy Position").value);
                    hearts = int.Parse((string)ini.FindItem("Hearts").value);
                    memories = int.Parse((string)ini.FindItem("Memories").value);
                    abilities = int.Parse((string)ini.FindItem("Abilities").value);
                }
                catch (Exception exception) { exception.Error($"Failed loading save file"); Defaults(); }
            else
            {
                Defaults();
                Save();
            }
        }
        public static void Save()
        {
            FileUtilities.INI ini = new FileUtilities.INI();
            ini.AddItem("Player Scene", playerScene);
            ini.AddItem("Fairy Scene", fairyScene);
            ini.AddItem("Fairy Position", fairyPosition);
            ini.AddItem("Hearts", hearts);
            ini.AddItem("Memories", memories);
            ini.AddItem("Abilities", abilities);

            FileUtilities.Save("Save.ini", ini);
        }

        private static void Defaults()
        {
            playerScene = "Grove of Beginnings";
            fairyScene = "Grove of Beginnings";
            fairyPosition = 0;
            hearts = 5;
            memories = 0;
            abilities = 0;
        }
    }
}