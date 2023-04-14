using System;

using UnityEngine;

using Simplex;


namespace Game.UI
{
    public class Menu : Layer<Menu>
    {
        public Menu()
        {
            Div viewport = this.Create<Div>("viewport", "flexible");

            Div map = viewport.Create<Div>("gui", "background1", "map");
            Div stats = viewport.Create<Div>("gui", "background2", "stats");
            Div character = viewport.Create<Div>("gui", "background2", "character");

            Div options = this.Create<Div>("gui", "background3", "options");
            options.Create<Button>("gui", "rectangle", "green").Modify("Resume").Bind(_ => UI.Menu.Hide());
            options.Create<Button>("gui", "rectangle", "yellow").Modify("Settings").Bind(_ => { UI.Menu.Hide(); UI.Settings.Show(); });
            options.Create<Button>("gui", "rectangle", "red").Modify("Quit").Bind(_ => GeneralUtilities.Quit());
        }
    }
}