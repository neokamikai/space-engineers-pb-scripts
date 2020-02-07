#region pre-script
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;
namespace IngameScript.DrillsGroupToggle
{
  public class Program : MyGridProgram
  {

    #endregion
    //To put your code in a PB copy from this comment...
    #region SCRIPT
    const float DIGMODE_THRUSTER_OVERRIDE = 5000;
    private readonly string[] matDisplayIds = new string[] { "Iron", "Silicon", "Silver", "Gold", "Cobalt", "Nickel", "Uranium", "Ice", "Stone" };
    public Program()
    {
      var drills = new List<IMyShipDrill>();
      GridTerminalSystem.GetBlocksOfType(drills);
      onChar = drills.Where(d => d.Enabled).Count() > drills.Where(d => !d.Enabled).Count() ? "-" : "";
      Runtime.UpdateFrequency = UpdateFrequency.Update10;
    }
    const double INVQTY_MUTIPLIER = 1000000;
    const int AVAILABLE_AMOUNT_LENGTH = 10;
    string onChar = "";
    public void Save()
    {

    }

    public IMyTextPanel GetTextPanelWithName(string name)
    {
      IMyTextPanel p = GridTerminalSystem.GetBlockWithName(name) as IMyTextPanel;
      if (p == null) Echo($"Could not find TextPanel named [{name}]");
      return p;
    }
    public IMyBlockGroup GetGroupWithName(string name)
    {
      var g = GridTerminalSystem.GetBlockGroupWithName(name);
      if (g == null) Echo($"Could not find a group named \"{name}\"");
      else
        Echo($"Found Group: {name}");
      return g;
    }
    bool digMode = false;
    public void UpdateOreCount(IMyInventory inv, ref Dictionary<string, Dictionary<string, object>> info)
    {
      var items = inv.GetItems();
      foreach (IMyInventoryItem item in items)
      {
        var uid = item.Content.SubtypeId.ToString();
        if (!matDisplayIds.Contains(uid)) continue;
        if (!info.ContainsKey(uid))
          info.Add(uid, new Dictionary<string, object>() {
              { "name", item.Content.SubtypeId },
              { "iqty",item.Amount.RawValue / INVQTY_MUTIPLIER },
            });
        else
          info[uid]["iqty"] = ((double)info[uid]["iqty"]) + (item.Amount.RawValue / INVQTY_MUTIPLIER);
      }
    }
    public string FormatItemDisplayName(string s)
    {
      return System.Text.RegularExpressions.Regex.Replace(s, "([A-Z])", " $1").Replace("_", " ").Trim();
    }
    public string FormatItemQty(double q)
    {
      string s = " ";
      if (q > 1000) { q /= 1000; s = "k"; }
      if (q > 1000) { q /= 1000; s = "m"; }
      if (q > 1000) { q /= 1000; s = "b"; }
      if (q > 1000) { q /= 1000; s = "t"; }
      if (q > 1000) { q /= 1000; s = "q"; }
      if (q > 1000)
        return $"+99.999.99q units";
      return $"{q.ToString("#,##0.00")}{s}";
    }

    public void Main(string argument)
    {
      if (string.IsNullOrEmpty(argument)) Echo($"Argument: {argument}");
      var massPanel = GetTextPanelWithName("txtPanelMassLoad");
      if (massPanel == null) Echo("txtPanelMassLoad not found");
      IMyInteriorLight light;
      List<IMyTerminalBlock> crates = new List<IMyTerminalBlock>();
      var massPanelGroup = GetGroupWithName("MassLoadPanels");
      var reactorsGroup = GetGroupWithName("Reactors");
      if (string.IsNullOrEmpty(argument)) argument = "";
      var drills = new List<IMyShipDrill>();
      GridTerminalSystem.GetBlocksOfType(drills);
      light = (IMyInteriorLight)GridTerminalSystem.GetBlockWithName("Inventory Load Light");
      if (drills == null) return;
      if (drills.Count == 0) { Echo($"No drills found"); return; } else { Echo($"{drills.Count} Drills found"); }
      switch (argument)
      {
        case "toggle-down-thrusters":
          var thrusters = new List<IMyThrust>();
          var connectors = new List<IMyShipConnector>();
          GridTerminalSystem.GetBlocksOfType(thrusters);
          GridTerminalSystem.GetBlocksOfType(connectors);
          if (thrusters.Count == 0) { Echo($"No thrusters found"); return; }
          digMode = !digMode;
          //bool canExecute = !connectors.Any(c => c.);
          foreach (var t in thrusters)
          {
            if (!digMode)
            {
              t.Enabled = true;
              t.ThrustOverride = 0;
              continue;
            }
            if (t.GridThrustDirection.Y > 0)
            {
              t.Enabled = true;
              t.ThrustOverride = DIGMODE_THRUSTER_OVERRIDE;
            }
            else
            {
              t.Enabled = false;
            }
            Echo($"{t.CustomName}:{t.DisplayName}:{t.EntityId}:{t.GridThrustDirection}");
          }
          break;
        case "on":
          onChar = "-";
          foreach (IMyShipDrill drill in drills)
          {
            drill.Enabled = true;
          }
          break;
        case "off":
          onChar = "";
          foreach (IMyShipDrill drill in drills)
          {
            drill.Enabled = false;
          }
          break;
        case "toggle":
          onChar = onChar.Length == 0 ? "-" : "";
          foreach (IMyShipDrill drill in drills)
          {
            drill.Enabled = !drill.Enabled;
          }
          break;
        default:
          double totalVolume = 0, maxVolume = 0, drillsVolume = 0, drillsMaxVolume = 0, cratesVolume = 0, cratesMaxVolume = 0, 
            drillsEnabled = 0, drillsDisabled = 0, drillsWorking = 0, drillsNotWorking = 0, drillsFunctional =0, drillsNonFunctional = 0;
          var info = new Dictionary<string, Dictionary<string, object>>();
          foreach (IMyShipDrill drill in drills)
          {
            if (!IsOnSameGrid(drill)) continue;
            drill.ShowOnHUD = !drill.IsFunctional;
            var inventory = drill.GetInventory(0);
            var items = inventory.GetItems();
            if (drill.Enabled) ++drillsEnabled; else ++drillsDisabled;
            if (drill.IsWorking) ++drillsWorking; else ++drillsNotWorking;
            if (drill.IsFunctional) ++drillsFunctional; else ++drillsNonFunctional;
            var volume = new
            {
              current = inventory.CurrentVolume.RawValue / INVQTY_MUTIPLIER,
              max = inventory.MaxVolume.RawValue / INVQTY_MUTIPLIER
            };
            totalVolume += volume.current;
            maxVolume += volume.max;
            drillsVolume += volume.current;
            drillsMaxVolume += volume.max;
            UpdateOreCount(inventory, ref info);
            drill.ShowOnHUD = (!drill.IsFunctional);
            if (drill.CustomData.Contains("hide-if-empty") && items.Count == 0) drill.ShowInInventory = false;
            if (drill.CustomData.Contains("show-with-items") && items.Count > 0) drill.ShowInInventory = true;
            
          }
          GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(crates);
          if (crates != null)
          {
            foreach (IMyCargoContainer crate in crates)
            {
              if (!IsOnSameGrid(crate)) continue;

              var inventory = crate.GetInventory(0);
              var volume = new
              {
                current = inventory.CurrentVolume.RawValue / INVQTY_MUTIPLIER,
                max = inventory.MaxVolume.RawValue / INVQTY_MUTIPLIER
              };
              totalVolume += volume.current;
              maxVolume += volume.max;
              cratesVolume += volume.current;
              cratesMaxVolume += volume.max;
              UpdateOreCount(inventory, ref info);
            }
          }
          double percent = totalVolume / maxVolume,
            percentCargo = cratesVolume / cratesMaxVolume,
            percentDrills = drillsVolume / drillsMaxVolume;
          if (light != null)
          {
            if (maxVolume > 0)
            {
              var color1 = new { red = 0D, green = 255D, blue = 0D };
              var color2 = new { red = 255D, green = 0D, blue = 0D };
              double resultRed = color1.red + percent * (color2.red - color1.red);
              double resultGreen = color1.green + percent * (color2.green - color1.green);
              double resultBlue = color1.blue + percent * (color2.blue - color1.blue);
              light.Enabled = true;
              light.Color = new Color((float)resultRed, (float)resultGreen, (float)resultBlue);
            }
            else
            {
              light.Enabled = false;
            }
          }
          var s =
              $"Total Volume Usage: {percent.ToString("0.##%")}\n" +
              $"Total Volume: {totalVolume.ToString("#,##0.##")}/{maxVolume.ToString("#,##0.##")}\n" +
              $"Cargo Containers Usage: {percentCargo.ToString("0.##%")}\n" +
              $"Cargo Containers Volume: {cratesVolume.ToString("#,##0.##")}/{cratesMaxVolume.ToString("#,##0.##")}\n" +
              $"Drills' Container Usage: {percentDrills.ToString("0.##%")}\n" +
              $"Drills' Container Volume: {drillsVolume.ToString("#,##0.##")}/{drillsMaxVolume.ToString("#,##0.##")}\n";


          if (reactorsGroup != null)
          {
            var reactors = new List<IMyReactor>();
            reactorsGroup.GetBlocksOfType(reactors);
            int tmp = 0;
            foreach (IMyReactor reactor in reactors)
            {
              if (!IsOnSameGrid(reactor)) continue;
              var inventory = reactor.GetInventory();
              var name = reactor.DisplayName == null ? reactor.CustomName : reactor.DisplayName;
              double uranium = 0;
              foreach (var item in inventory.GetItems().Where(it => it.Content.SubtypeId.ToString() == "Uranium"))
              {
                uranium += item.Amount.RawValue / INVQTY_MUTIPLIER;
              }
              s += (tmp % 2 == 0 ? " | " : "") + $"{name}: {uranium.ToString("#,##0.##")}" + (tmp % 2 == 0 ? "\n" : "");

              ++tmp;
            }
          }
          //Display Ore info
          string sHeader = " Stored ".PadRight(AVAILABLE_AMOUNT_LENGTH, ' ') + "Resource Name";
          var cl = drills.Count.ToString().Length;
          s += $"{(onChar.Length > 0 ? "\nDrills Working " + onChar : "\nDrills Off      ")} - " +
            $"O:{drillsFunctional.ToString("0").PadLeft(cl, ' ')}/" +
            $"X:{drillsNonFunctional.ToString("0").PadLeft(cl, ' ')}/" +
            $"E:{drillsEnabled.ToString("0").PadLeft(cl, ' ')}/" +
            $"D:{drillsDisabled.ToString("0").PadLeft(cl, ' ')}/" +
            $"W:{drillsWorking.ToString("0").PadLeft(cl, ' ')}/" +
            $"N:{drillsNotWorking.ToString("0").PadLeft(cl, ' ')}/" +
            $"{drills.Count}\n";
          s += $"Ores\n{sHeader}\n";
          var sorted = info.ToList();
          sorted.Sort((a, b) => (double)a.Value["iqty"] > (double)b.Value["iqty"] ? -1 : (double)a.Value["iqty"] < (double)b.Value["iqty"] ? 1 : 0);
          sorted.Reverse();
          foreach (var kvp in sorted)
          {
            var name = FormatItemDisplayName($"{kvp.Value["name"]}");
            s += $"{FormatItemQty((double)kvp.Value["iqty"]).PadLeft(AVAILABLE_AMOUNT_LENGTH, ' ')} {name} ({kvp.Key})" + $"\n";
          }
          foreach (var id in matDisplayIds.Where(a => !info.ContainsKey(a)))
          {
            var name = FormatItemDisplayName($"{id}");
            s += $"{"NONE".PadLeft(AVAILABLE_AMOUNT_LENGTH, ' ')} {name} ({id})" + $"\n";
          }

          if (massPanel != null) massPanel.WritePublicText(" " + s.Replace("\n", "\n "), false);
          if (massPanelGroup != null)
          {
            var panels = new List<IMyTextPanel>();
            massPanelGroup.GetBlocksOfType(panels);
            foreach (var panel in panels)
            {
              panel.WritePublicText(s);
            }
          }
          onChar =
          onChar == "-" ? "\\" :
          onChar == "\\" ? "|" :
          onChar == "|" ? "/" :
          onChar == "/" ? "-" :
          ""
          ;
          break;
      }
    }

    private bool IsOnSameGrid(IMyTerminalBlock block)
    {
      return block.CubeGrid.EntityId == Me.CubeGrid.EntityId;
    }
    #endregion
    //to this comment.
    #region post-script
  }
}
#endregion