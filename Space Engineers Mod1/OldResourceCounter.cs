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
namespace IngameScript.OldResourceCounter
{
  public class Program : MyGridProgram
  {

    #endregion
    //To put your code in a PB copy from this comment...
    #region SCRIPT
    const double INVQTY_MUTIPLIER = 1000000;
    const int AVAILABLE_AMOUNT_LENGTH = 20;
    private readonly string[] oreDisplayIds = new string[] { "Uranium", "Ice" };
    private readonly string[] matDisplayIds = new string[] { "Iron", "Silicon", "Silver", "Gold", "Cobalt", "Nickel", "Magnesium", "Platinum", "Stone" };
    private readonly string[] cmpDisplayIds = new string[] { "Reactor", "MetalGrid", "Canvas", "PowerCell", "Detector",
"GravityGenerator", "Superconductor", "RadioCommunication", "SolarCell", "Display", "BulletproofGlass", "LargeTube",
"Girder", "Computer", "Thrust", "Motor", "SmallTube", "Construction", "InteriorPlate", "SteelPlate" };
    private readonly string[] toolDisplayIds = new string[] { };
    private readonly string[] ammoDisplayIds = new string[] { "Nato_25x184mm", "Missile200mm", "Nato_5p56x45mm" };
    public Program()
    {
      Runtime.UpdateFrequency = UpdateFrequency.Update10;
    }
    public void Save()
    {

    }
    public IMyTextPanel GetTextPanelWithName(string name)
    {
      IMyTextPanel p = GridTerminalSystem.GetBlockWithName(name) as IMyTextPanel;
      if (p == null) Echo($"Could not find TextPanel named [{name}]");
      return p;
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
    public bool IsMatch(string s, string p, bool ignoreCase = true)
    {
      return System.Text.RegularExpressions.Regex.IsMatch(s, p, ignoreCase ? System.Text.RegularExpressions.RegexOptions.IgnoreCase : System.Text.RegularExpressions.RegexOptions.None);
    }
    public bool IsOre(string s) { return IsMatch(s, "^(.+Ore)$") || oreDisplayIds.Contains(s); }
    public bool IsComponent(string s) { return cmpDisplayIds.Contains(s); }
    public bool IsMaterial(string s) { return matDisplayIds.Contains(s); }
    public bool IsTool(string s) { return toolDisplayIds.Contains(s); }
    public bool IsAmmo(string s) { return ammoDisplayIds.Contains(s); }
    public void Main(string argument)
    {
      //Variables
      var info = new Dictionary<string, Dictionary<string, object>>();
      List<IMyTerminalBlock> crates = new List<IMyTerminalBlock>();
      IMyTextPanel oreDisplay;
      IMyTextPanel matDisplay;
      IMyTextPanel cmpDisplay;
      IMyTextPanel allDisplay;
      IMyTextPanel debugDisplay;
      string debugStr = "";
      //Get display instances
      oreDisplay = GetTextPanelWithName("lcdOreDisplay");
      matDisplay = GetTextPanelWithName("lcdMaterialDisplay");
      cmpDisplay = GetTextPanelWithName("lcdComponentDisplay");
      allDisplay = GetTextPanelWithName("lcdAllItemsDisplay");
      debugDisplay = GetTextPanelWithName("lctDebugDisplay");
      //Fill Cargo Container List
      GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(crates);

      //Get inventories
      foreach (IMyCargoContainer cc in crates)
      {
        if (cc.CubeGrid.CustomName != Me.CubeGrid.CustomName) continue;
        IMyInventory inv = cc.GetInventory(0);
        var items = inv.GetItems();
        items.Sort(SortItems);
        foreach (IMyInventoryItem item in items)
        {
          var uid = item.Content.SubtypeId.ToString();
          if (!info.ContainsKey(uid))
            info.Add(uid, new Dictionary<string, object>() {
              { "name", item.Content.SubtypeId },
              { "iqty",item.Amount.RawValue / INVQTY_MUTIPLIER },
            });
          else
            info[uid]["iqty"] = ((double)info[uid]["iqty"]) + (item.Amount.RawValue / INVQTY_MUTIPLIER);
        }
      }
      string sHeader = "Available  ".PadLeft(AVAILABLE_AMOUNT_LENGTH, ' ') + " Resource Name";
      string s = "", sAll =
        $"All Items\n{sHeader}\n",
        sOre = $"Ores\n{sHeader}\n",
        sMat = $"Materials\n{sHeader}\n",
        sCmp = $"Components\n{sHeader}\n";
      var sorted = info.ToList();
      sorted.Sort((a, b) => (double)a.Value["iqty"] > (double)b.Value["iqty"] ? 1 : (double)a.Value["iqty"] < (double)b.Value["iqty"] ? -1 : 0);
      foreach (var kvp in sorted)
      {
        String name = FormatItemDisplayName($"{kvp.Value["name"]}");
        s = $"{FormatItemQty((double)kvp.Value["iqty"]).PadLeft(AVAILABLE_AMOUNT_LENGTH, ' ')} {name} ({kvp.Key})" + $"\n";
        if (IsOre(kvp.Key)) sOre += s;
        else if (IsMaterial(kvp.Key)) sMat += s;
        else if (IsComponent(kvp.Key)) sCmp += s;
        if (!IsOre(kvp.Key) && !IsMaterial(kvp.Key) && !IsComponent(kvp.Key))
          sAll += s;


      }


      foreach (var id in matDisplayIds.Where(a => !info.ContainsKey(a)))
      {
        var name = FormatItemDisplayName($"{id}");
        sMat += $"{"NONE".PadLeft(AVAILABLE_AMOUNT_LENGTH, ' ')} {name} ({id})" + $"\n";
      }

      if (oreDisplay != null) oreDisplay.WritePublicText(sOre, false);
      if (matDisplay != null) matDisplay.WritePublicText(sMat, false);
      if (cmpDisplay != null) cmpDisplay.WritePublicText(sCmp, false);
      if (allDisplay != null) allDisplay.WritePublicText(sAll, false);
      if (debugDisplay != null) debugDisplay.WritePublicText(debugStr, false);
    }

    private int SortItems(IMyInventoryItem x, IMyInventoryItem y)
    {
      return x.Amount < y.Amount ? 1 : x.Amount > y.Amount ? -1 : 0;
    }
    #endregion
    //to this comment.
    #region post-script
  }
}
#endregion