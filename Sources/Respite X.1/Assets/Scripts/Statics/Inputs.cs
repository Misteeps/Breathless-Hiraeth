using System;
using System.Text.RegularExpressions;

using UnityEngine;

using Simplex;


namespace Game
{
    public static class Inputs
    {
        public static Settings.Keybind Click => Settings.click;
        public static Settings.Keybind Escape => Settings.escape;

        public static Settings.Keybind MoveUp => Settings.moveUp;
        public static Settings.Keybind MoveDown => Settings.MoveDown;
        public static Settings.Keybind MoveLeft => Settings.moveLeft;
        public static Settings.Keybind MoveRight => Settings.moveRight;
        public static Settings.Keybind Sprint => Settings.sprint;
        public static Settings.Keybind Jump => Settings.jump;

        public static Settings.Keybind Breath => Settings.breath;
        public static Settings.Keybind Attack => Settings.attack;
        public static Settings.Keybind CancelAbility => Settings.cancelAbility;
        public static Settings.Keybind Ability1 => Settings.ability1;
        public static Settings.Keybind Ability2 => Settings.ability2;
        public static Settings.Keybind Ability3 => Settings.ability3;
        public static Settings.Keybind Ability4 => Settings.ability4;

        public static Settings.Keybind ZoomIn => Settings.zoomIn;
        public static Settings.Keybind ZoomOut => Settings.zoomOut;


        public static Vector3 worldCursor;
        public static Vector2 screenCursor;
        public static float MouseX => Input.mousePosition.x;
        public static float MouseY => Input.mousePosition.y;
        public static bool ScrollUp => Input.mouseScrollDelta.y > 0;
        public static bool ScrollDown => Input.mouseScrollDelta.y < 0;


        public static void UpdateWorldCursor(float distance) => worldCursor = Monolith.Camera.ScreenToWorldPoint(new Vector3(MouseX, MouseY, distance));
        public static void UpdateScreenCursor(float width, float height)
        {
            Vector2 position = Monolith.Camera.ScreenToViewportPoint(new Vector3(MouseX, MouseY, 0));
            float x = Mathf.Lerp(0, width, position.x);
            float y = Mathf.Lerp(height, 0, position.y);
            screenCursor = new Vector2(x, y);
        }

        public static string KeycodeString(this KeyCode keycode)
        {
            switch (keycode)
            {
                default: return Regex.Replace(keycode.ToString(), "([a-z0-9])([A-Z0-9])", "$1 $2");
                case KeyCode.None: return string.Empty;
                case KeyCode.Mouse0: return "Left Click";
                case KeyCode.Mouse1: return "Right Click";
                case KeyCode.Mouse2: return "Scroll Click";
                case KeyCode.Mouse3: return "Mouse Thumb 1";
                case KeyCode.Mouse4: return "Mouse Thumb 2";
                case KeyCode.Return: return "Enter";
                case KeyCode.Alpha0: return "0";
                case KeyCode.Alpha1: return "1";
                case KeyCode.Alpha2: return "2";
                case KeyCode.Alpha3: return "3";
                case KeyCode.Alpha4: return "4";
                case KeyCode.Alpha5: return "5";
                case KeyCode.Alpha6: return "6";
                case KeyCode.Alpha7: return "7";
                case KeyCode.Alpha8: return "8";
                case KeyCode.Alpha9: return "9";
                case KeyCode.BackQuote: return "`";
                case KeyCode.Minus: return "-";
                case KeyCode.Equals: return "=";
                case KeyCode.LeftBracket: return "[";
                case KeyCode.RightBracket: return "]";
                case KeyCode.Backslash: return "\\";
                case KeyCode.Semicolon: return ";";
                case KeyCode.Quote: return "'";
                case KeyCode.Comma: return ",";
                case KeyCode.Period: return ".";
                case KeyCode.Slash: return "/";
                case (KeyCode)541: return "Scroll Up";
                case (KeyCode)542: return "Scroll Down";
            }
        }
    }
}