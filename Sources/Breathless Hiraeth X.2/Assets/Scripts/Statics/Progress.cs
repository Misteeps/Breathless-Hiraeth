using System;
using System.Collections.Generic;

using UnityEngine;

using Simplex;


namespace Game
{
    public static class Progress
    {
        public static DateTime lastSaved;

        public static string scene;
        public static int position;

        public static int hearts;
        public static int memories;
        public static int abilities;

        public static int damage;
        public static int magic;
        public static int speed;
        public static int cooldown;

        public static List<string> encounters;


        public static void Load()
        {
            lastSaved = DateTime.Now;
            encounters = new List<string>();

            if (FileUtilities.Load("Save.ini", out FileUtilities.INI ini))
                try
                {
                    foreach (FileUtilities.INI.Line line in ini.lines)
                        if (line is FileUtilities.INI.Item item)
                            switch (item.key)
                            {
                                case "Scene": scene = (string)item.value; break;
                                case "Position": position = int.Parse((string)item.value); break;

                                case "Hearts": hearts = int.Parse((string)item.value); break;
                                case "Memories": memories = int.Parse((string)item.value); break;
                                case "Abilities": abilities = int.Parse((string)item.value); break;

                                case "Damage": damage = int.Parse((string)item.value); break;
                                case "Magic": magic = int.Parse((string)item.value); break;
                                case "Speed": speed = int.Parse((string)item.value); break;
                                case "Cooldown": cooldown = int.Parse((string)item.value); break;

                                default: encounters.Add(item.key); break;
                            }
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

            ini.AddItem("Scene", scene);
            ini.AddItem("Position", position);

            ini.AddItem("Hearts", hearts);
            ini.AddItem("Memories", memories);
            ini.AddItem("Abilities", abilities);

            ini.AddItem("Damage", damage);
            ini.AddItem("Magic", magic);
            ini.AddItem("Speed", speed);
            ini.AddItem("Cooldown", cooldown);

            foreach (string encounter in encounters)
                ini.AddItem(encounter, true);

            if (FileUtilities.Save("Save.ini", ini))
                lastSaved = DateTime.Now;
        }

        private static void Defaults()
        {
            scene = "Grove of Beginnings";
            position = 0;

            hearts = 6;
            memories = 0;
            abilities = 0;

            damage = 0;
            magic = 0;
            speed = 0;
            cooldown = 0;

            encounters = new List<string>();
        }
    }
}