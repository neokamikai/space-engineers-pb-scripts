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
namespace IngameScript.ThrusterDirections
{
  public class Program : MyGridProgram
  {

    #endregion
    //To put your code in a PB copy from this comment...
    #region SCRIPT

    public Program()
    {

    }

    public void Save()
    {

    }
    [Flags]
    public enum ThrusterDirection
    {
      None = 0, Front = 1, Back = 2, Left = 4, Right = 8, Up = 16, Down = 32
    }
    
    public static ThrusterDirection TranslateThrusterDirection(Vector3I dirVector)
    {
      ThrusterDirection d = ThrusterDirection.None;
      d |= dirVector.Z < 0 ? ThrusterDirection.Back : dirVector.Z == 0 ? ThrusterDirection.None : ThrusterDirection.Front;
      d |= dirVector.Y < 0 ? ThrusterDirection.Up : dirVector.Y == 0 ? ThrusterDirection.None : ThrusterDirection.Down;
      d |= dirVector.X < 0 ? ThrusterDirection.Right : dirVector.X == 0 ? ThrusterDirection.None : ThrusterDirection.Left;
      return d;
    }

    public void Main(string argument)
    {
      var thrusters = new List<IMyThrust>();
      GridTerminalSystem.GetBlocksOfType(thrusters);
      foreach (var t in thrusters)
      {
        if (t.CubeGrid.EntityId != Me.CubeGrid.EntityId) continue;
        var dir = TranslateThrusterDirection(t.GridThrustDirection);
        Echo($"{t.CustomName} X:{t.GridThrustDirection.X} Y:{t.GridThrustDirection.Y} Z:{t.GridThrustDirection.Z} EnumDirection: {dir}");
      }

    }

    #endregion
    //to this comment.
    #region post-script
  }
}
#endregion