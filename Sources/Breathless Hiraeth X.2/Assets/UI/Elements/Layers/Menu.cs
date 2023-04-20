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

            Div map = viewport.Create<Div>("gui", "background1", "map", "flexible");

            Div abilities = viewport.Create<Div>("gui", "background2", "abilities");

            Div stats = viewport.Create<Div>("gui", "background2", "stats");
            stats.Create<LabelInput>("stat", "icon").Name("hearts").Bind(() => $"+{Progress.hearts}", null);
            stats.Create<LabelInput>("stat", "icon").Name("damage").Bind(() => $"+{Progress.damage}", null);
            stats.Create<LabelInput>("stat", "icon").Name("ability").Bind(() => $"+{Progress.ability}", null);
            stats.Create<LabelInput>("stat", "icon").Name("speed").Bind(() => $"+{Progress.speed}", null);
            stats.Create<LabelInput>("stat", "icon").Name("cooldown").Bind(() => $"+{Progress.cooldown}", null);
            stats.Create<VerticalSpace>().Size(Size.Huge);
            stats.Create<LabelInput>("stat", "icon").Name("memories").Bind(() => $"+{Progress.memories}", null);

            Div options = this.Create<Div>("gui", "background3", "options");
            options.Create<Button>("gui", "rectangle", "green").Modify("Resume").Bind(_ => UI.Menu.Hide());
            options.Create<Button>("gui", "rectangle", "yellow").Modify("Settings").Bind(_ => { UI.Menu.Hide(); UI.Settings.Show(); });
            options.Create<Button>("gui", "rectangle", "red").Modify("Quit").Bind(_ => { Game.Progress.Save(); Game.Settings.Save(); GeneralUtilities.Quit(); });

            Label saved = this.Create<LabelInput>("saved").Bind(() => $"Last Saved: {Progress.lastSaved.ToShortTimeString()}", null);
        }
    }
}