using System;
using System.Collections.Generic;

namespace FBFactory
{
    public enum ConnectionType
    {
        Production,
        Reject
    }

    [Serializable]
    public class LineConnection
    {
        public UnitBase PrevUnit;
        public GroupConveyerBelt Conveyor;
        public UnitBase NextUnit;
        public ConnectionType Type;
    }

    [Serializable]
    public class ProductionPath
    {
        public string Name;
        public List<LineConnection> Connections = new();
    }
}
