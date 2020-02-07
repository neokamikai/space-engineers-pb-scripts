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
namespace IngameScript.ResourceCounter
{
	public class Program : MyGridProgram
	{

		#endregion
		//To put your code in a PB copy from this comment...

		#region SCRIPT
		const double INVQTY_MUTIPLIER = 1000000;
		const int AVAILABLE_AMOUNT_LENGTH = 20;
		private readonly string[] oreDisplayIds = new string[] { "IronOre", "SiliconOre", "SilverOre", "GoldOre", "CobaltOre", "NickelOre", "UraniumOre", "IceOre" };
		private readonly string[] matDisplayIds = new string[] { "IronIngot", "SiliconIngot", "SilverIngot", "GoldIngot", "CobaltIngot", "NickelIngot" };
		private readonly string[] cmpDisplayIds = new string[] { };
		private readonly string[] toolDisplayIds = new string[] { };
		private readonly string[] ammoDisplayIds = new string[] { };
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
		public Dictionary<string, object> CreateCountManager(string uid, string tid, MyInventoryItem item)
		{
			return new Dictionary<string, object>() {
			  { "name", uid },
			  { "type", IsOre(tid)?"Ore": IsComponent(tid)?"Component":IsMaterial(tid)?"Ingot":"Other" },
			  { "fullname", uid+tid},
			  { "iqty", 0D },//Container Count
			  { "iqtyRef", 0D},//Refinery Count
			  { "iqtyRea", 0D},//Reactor Count
			  { "iqtyAsb", 0D}//Assembler Count
			};
		}
		public void UpdateCount(ref Dictionary<string, Dictionary<string, object>> info, string uid, string tid, MyInventoryItem item, string key)
		{
			if (!info.ContainsKey(uid))
				info.Add(uid, CreateCountManager(uid, tid, item));
			info[uid][key] = ((double)info[uid][key]) + (item.Amount.RawValue / INVQTY_MUTIPLIER);
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
		public bool IsComponent(string s) { return IsMatch(s, "^(.+Component)$") || cmpDisplayIds.Contains(s); }
		public bool IsMaterial(string s) { return IsMatch(s, "^(.+Ingot)$") || matDisplayIds.Contains(s); }
		public bool IsTool(string s) { return toolDisplayIds.Contains(s); }
		public bool IsAmmo(string s) { return ammoDisplayIds.Contains(s); }
		public void Main(string argument)
		{
			//Variables
			var info = new Dictionary<string, Dictionary<string, object>>();
			var crates = new List<IMyCargoContainer>();
			var refineries = new List<IMyRefinery>();
			var assemblers = new List<IMyAssembler>();
			var reactors = new List<IMyReactor>();
			IMyTextPanel oreDisplay;
			IMyTextPanel matDisplay;
			IMyTextPanel cmpDisplay;
			IMyTextPanel refDisplay;
			IMyTextPanel asbDisplay;
			IMyTextPanel reaDisplay;
			IMyTextPanel allDisplay;
			IMyTextPanel debugDisplay;
			string debugStr = "";
			//Get display instances
			oreDisplay = GetTextPanelWithName("lcdOreDisplay");
			matDisplay = GetTextPanelWithName("lcdMaterialDisplay");
			cmpDisplay = GetTextPanelWithName("lcdComponentDisplay");
			refDisplay = GetTextPanelWithName("lcdRefineryDisplay");
			reaDisplay = GetTextPanelWithName("lcdReactorDisplay");
			asbDisplay = GetTextPanelWithName("lcdAssemblerDisplay");
			allDisplay = GetTextPanelWithName("lcdAllItemsDisplay");
			debugDisplay = GetTextPanelWithName("lctDebugDisplay");
			//Fill Cargo Container List
			GridTerminalSystem.GetBlocksOfType(crates);
			GridTerminalSystem.GetBlocksOfType(refineries);
			GridTerminalSystem.GetBlocksOfType(assemblers);
			GridTerminalSystem.GetBlocksOfType(reactors);

			//Get inventories
			Echo($"{crates.Count} Cargo Containers");
			foreach (IMyCargoContainer cc in crates)
			{
				if (cc.CubeGrid.CustomName != Me.CubeGrid.CustomName) continue;
				IMyInventory inv = cc.GetInventory(0);
				//var items = inv.GetItems();
				var items = new List<MyInventoryItem>();
				inv.GetItems(items);
				items.Sort(SortItems);
				foreach (MyInventoryItem item in items)
				{
					var tid = item.Type.TypeId.ToString().Split('_').Last();
					var uid = item.Type.SubtypeId.ToString() + tid;
					UpdateCount(ref info, uid, tid, item, "iqty");

				}
			}
			Echo($"{refineries.Count} Refineries");
			foreach (var refinery in refineries)
			{
				if (refinery.CubeGrid.CustomName != Me.CubeGrid.CustomName) continue;
				for (int i = 0; i < refinery.InventoryCount; i++)
				{
					var inv = refinery.GetInventory(i);
					//var items = inv.GetItems();
					var items = new List<MyInventoryItem>();
					inv.GetItems(items);
					foreach (MyInventoryItem item in items)
					{
						var tid = item.Type.TypeId.ToString().Split('_').Last();
						var uid = item.Type.SubtypeId.ToString() + tid;
						UpdateCount(ref info, uid, tid, item, "iqtyRef");
					}
				}
			}
			Echo($"{reactors.Count} Reactors");
			foreach (var reactor in reactors)
			{
				if (reactor.CubeGrid.CustomName != Me.CubeGrid.CustomName) continue;
				for (int i = 0; i < reactor.InventoryCount; i++)
				{
					var inv = reactor.GetInventory(i);
					//var items = inv.GetItems();
					var items = new List<MyInventoryItem>();
					inv.GetItems(items);
					foreach (MyInventoryItem item in items)
					{
						var tid = item.Type.TypeId.ToString().Split('_').Last();
						var uid = item.Type.SubtypeId.ToString() + tid;
						UpdateCount(ref info, uid, tid, item, "iqtyRea");
					}
				}
			}
			Echo($"{reactors.Count} Assemblers");
			foreach (var assembler in assemblers)
			{
				if (assembler.CubeGrid.CustomName != Me.CubeGrid.CustomName) continue;
				if (assembler.DisassembleRatio > 0) continue;
				for (int i = 0; i < assembler.InventoryCount; i++)
				{
					var inv = assembler.GetInventory(i);
					//var items = inv.GetItems();
					var items = new List<MyInventoryItem>();
					inv.GetItems(items);
					foreach (MyInventoryItem item in items)
					{
						var tid = item.Type.TypeId.ToString().Split('_').Last();
						var uid = item.Type.SubtypeId.ToString() + tid;
						UpdateCount(ref info, uid, tid, item, "iqtyAsb");
					}
				}
			}

			string sHeader = "Available".PadLeft(AVAILABLE_AMOUNT_LENGTH, ' ') + "|Resource Name\n" + "".PadLeft(100, '-');
			string s = "", sAll =
			  $"All Items\n{sHeader}\n",
			  sOre = $"Ores\n{sHeader.Replace("Available".PadLeft(AVAILABLE_AMOUNT_LENGTH, ' '), "On Containers".PadLeft(14, ' ') + "|" + "On Refineries".PadLeft(14, ' '))}\n",
			  sMat = $"Materials\n{sHeader}\n",
			  sCmp = $"Components\n{sHeader}\n",
			  sRea = $"Reactors\n{sHeader}\n",
			  sAsb = $"Assemblers\n{sHeader}\n",
			  sRef = $"Assemblers\n{sHeader}\n";

			var sorted = info.ToList();
			sorted.Sort((a, b) => (double)a.Value["iqty"] > (double)b.Value["iqty"] ? -1 : (double)a.Value["iqty"] < (double)b.Value["iqty"] ? 1 : 0);
			//sorted.Reverse();
			foreach (var kvp in sorted)
			{
				var name = FormatItemDisplayName($"{kvp.Value["name"]}");
				if (IsOre(kvp.Key))
					s = $"{FormatItemQty((double)kvp.Value["iqty"]).PadLeft(14, ' ')}|{FormatItemQty((double)kvp.Value["iqtyRef"]).PadLeft(14, ' ')}|{name} ({kvp.Key})" + $"\n";
				else
					s = $"{FormatItemQty((double)kvp.Value["iqty"]).PadLeft(AVAILABLE_AMOUNT_LENGTH, ' ')}|{name} ({kvp.Key})" + $"\n";

				if (IsOre(kvp.Key)) sOre += s;
				else if (IsMaterial(kvp.Key)) sMat += s;
				else if (IsComponent(kvp.Key)) sCmp += s;
				if (!IsOre(kvp.Key) && !IsMaterial(kvp.Key) && !IsComponent(kvp.Key))
					sAll += s;

				if ((double)kvp.Value["iqtyRea"] > 0)
					sRea += $"{FormatItemQty((double)kvp.Value["iqtyRea"]).PadLeft(AVAILABLE_AMOUNT_LENGTH, ' ')}|{name} ({kvp.Key})" + $"\n";
				if ((double)kvp.Value["iqtyRef"] > 0)
					sRef += $"{FormatItemQty((double)kvp.Value["iqtyRef"]).PadLeft(AVAILABLE_AMOUNT_LENGTH, ' ')}|{name} ({kvp.Key})" + $"\n";
				if ((double)kvp.Value["iqtyAsb"] > 0)
					sRef += $"{FormatItemQty((double)kvp.Value["iqtyAsb"]).PadLeft(AVAILABLE_AMOUNT_LENGTH, ' ')}|{name} ({kvp.Key})" + $"\n";
			}
			foreach (var id in matDisplayIds.Where(a => !info.ContainsKey(a)))
			{
				var name = FormatItemDisplayName($"{id}");
				sMat += $"{"NONE".PadLeft(AVAILABLE_AMOUNT_LENGTH, ' ')} {name} ({id})" + $"\n";
			}

			if (oreDisplay != null) oreDisplay.WriteText(sOre, false);
			if (matDisplay != null) matDisplay.WriteText(sMat, false);
			if (cmpDisplay != null) cmpDisplay.WriteText(sCmp, false);
			if (refDisplay != null) refDisplay.WriteText(sRef, false);
			if (reaDisplay != null) reaDisplay.WriteText(sRea, false);
			if (asbDisplay != null) asbDisplay.WriteText(sAsb, false);
			if (allDisplay != null) allDisplay.WriteText(sAll, false);
			if (debugDisplay != null) debugDisplay.WriteText(debugStr, false);
		}

		private int SortItems(MyInventoryItem x, MyInventoryItem y)
		{
			return x.Amount < y.Amount ? -1 : x.Amount > y.Amount ? 1 : 0;
		}
		#endregion
		//to this comment.
		#region post-script
	}
}
#endregion