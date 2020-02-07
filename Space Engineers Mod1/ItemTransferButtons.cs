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
namespace IngameScript.ItemTransferButtons
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

    public void Main(string argument)
    {
      //TODO:
      //1) Get Destination from 'argument' with pattern: /(?'Destination'[^\|]+)|(?'Action'.+)
      //2) From the 'Destination' Block read and parse 'CustomData' with pattern: \"(?'action'[^:]+)\"\s*:\{(?'data'[^\{\}]+)\}
      //3) Get the desired 'action' and 'data'
    }
    #endregion
    //to this comment.
    #region post-script
  }
}
#endregion