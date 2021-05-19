#if ENABLE_CONSOLE

using UnityEngine;
using Sacristan.Ahhnold.Core;
using System.Collections.Generic;

public class Console : Sacristan.Ahhnold.Runtime.Console
{
    public override CommandRegistration[] RegistrableCommands => new CommandRegistration[] {
        new CommandRegistration("version", VersionAction, "Outputs game version"),
        new CommandRegistration("quit", QuitAction, "Quit Game"),
        new CommandRegistration("fow", FowAction, "Fog of War on/off"),
        new CommandRegistration("immortal", ImmortalUnitsAction, "Immortal player units"),
    };

    #region Command handlers
    static void QuitAction(string[] args)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBPLAYER
        Application.OpenURL(webplayerQuitURL);
#else
        Application.Quit();
#endif
    }

    static void VersionAction(string[] args)
    {
        ConsoleController.Log(string.Format("version: {0}", Application.version));
    }

    static void ImmortalUnitsAction(string[] args)
    {
        List<Unit_Health> units = new List<Unit_Health>(FindObjectsOfType<Unit_Health>());
        units = units.FindAll(x => x.owningFaction == FactionType.Player);

        for (int i = 0; i < units.Count; i++)
        {
            units[i].Immortal = true;
        }

        ConsoleController.Log($"Marked {units.Count} player units immortal!");
    }

    static void FowAction(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrEmpty(args[0]))
        {
            ConsoleController.LogError("missing parameter");
        }

        else if (args.Length > 1)
        {
            ConsoleController.LogError("more than 1 parameters");
        }
        else
        {
            string val = args[0].ToLower();
            bool on = val == "on";
            bool off = val == "off";

            if (on || off)
            {
                Projector darkProjector = GameObject.Find("FogCameraContainer/DarkProjector").GetComponent<Projector>(); //TODO: not nice
                darkProjector.enabled = on;

                if (on) ConsoleController.Log("Fog of War ON");
                else ConsoleController.Log("Fog of War OFF");
            }
            else
            {
                ConsoleController.LogError("param1 should be on or off");
            }
        }
    }

    #endregion
}

#endif