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
namespace IngameScript.KeepDoorsClosed
{
  public class Program : MyGridProgram
  {

    #endregion
    //To put your code in a PB copy from this comment...
    #region SCRIPT
    Dictionary<long, Dictionary<string, object>> doorControlSystem = new Dictionary<long, Dictionary<string, object>>();
    public Program()
    {
      Runtime.UpdateFrequency = UpdateFrequency.Update10;
    }
    const double AUTO_CLOSE_SECONDS = 2.5D;
    public void Save()
    {

    }

    public void Main(string argument)
    {
      var doors = new List<IMyAirtightSlideDoor>();
      GridTerminalSystem.GetBlocksOfType(doors);
      foreach (var door in doors)
      {
        if (door.CustomData.Contains("keep-open")) continue;
        var id = door.EntityId;
        var isOpen = door.Status == DoorStatus.Open;
        if (!doorControlSystem.ContainsKey(id))
        {
          doorControlSystem.Add(id, new Dictionary<string, object> { { "openSince", isOpen ? (DateTime?)DateTime.Now : null } });
        }
        DateTime? openSince = (DateTime?) doorControlSystem[id]["openSince"];
        if (openSince == null && isOpen) doorControlSystem[id]["openSince"] = DateTime.Now;
        else if(isOpen && openSince != null && DateTime.Now > openSince.Value.AddSeconds(AUTO_CLOSE_SECONDS) )
        {
          //Echo($"Should Close Door {id}");
          doorControlSystem[id] = null;
          door.CloseDoor();
        }
      }
    }
    #endregion
    //to this comment.
    #region post-script
  }
}
#endregion