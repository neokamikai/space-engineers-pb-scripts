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
namespace IngameScript.BlockCounter
{
  public class Program : MyGridProgram
  {

    #endregion
    //To put your code in a PB copy from this comment...
    #region SCRIPT

    public Program()
    {
      Runtime.UpdateFrequency = UpdateFrequency.Update100;
    }

    public void Save()
    {

    }

    public void Main(string argument)
    {
      var blocks = new List<IMyTerminalBlock>();
      GridTerminalSystem.GetBlocks(blocks);
      //var grouped = blocks.GroupBy(bk => bk.GetType().Name.Substring(2));
      var grouped = blocks.GroupBy(bk => bk.CubeGrid.CustomName);
      foreach (var item in grouped)
      {
        Echo($"{item.Count().ToString().PadLeft(4, ' ')} {item.Key}");
      }
    }
    #endregion
    //to this comment.
    #region post-script
  }
}
#endregion