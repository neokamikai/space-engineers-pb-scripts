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
namespace IngameScript.InventoryItemDetail
{
  public class Program : MyGridProgram
  {

    #endregion
    //To put your code in a PB copy from this comment...
    #region SCRIPT

    public Program()
    {
      Runtime.UpdateFrequency = UpdateFrequency.Update10;
    }

    public void Save()
    {

    }

    public void Main(string argument)
    {
      //var container = (IMyCargoContainer)GridTerminalSystem.GetBlockWithName("ItemDetailContainer");
      var container = (IMyCargoContainer)GridTerminalSystem.GetBlockWithName("Ore Buffer Container");
      var inventory = container.GetInventory();
      var items = inventory.GetItems();
      if (items.Count == 0)
        Echo("Container has no item. Item(s) required.");
      else
        Echo($"Container has {items.Count} items.");
      int i = 0;
      foreach (var item in items)
      {
        
        if (i > 0) Echo("".PadLeft(10, '-'));
        var p = new {
          stId = item.Content.SubtypeId, id = item.ItemId, name = item.Content.SubtypeId + item.Content.TypeId.ToString().Split('_').Last(),
          tId = item.Content.TypeId, s = item.Content.ToString(), cnt = item.Amount, scale = item.Scale
        };
        Echo($"ItemID: {p.id}\n" +
          $"ToString(): {p.s}\n" +
          $"Name: {p.name}\n" +
          $"Scale: {p.scale}\n" +
          $"Amount: {p.cnt}\n" +
          $"Amount Raw: {p.cnt.RawValue}\n" +
          $"Content Info:\n" +
          $"SubtypeId: {p.stId}\n" +
          $"TypeId: {p.tId}\n");
        ++i;
      }

    }
    #endregion
    //to this comment.
    #region post-script
  }
}
#endregion