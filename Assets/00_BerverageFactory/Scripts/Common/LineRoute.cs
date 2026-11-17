using System;
using System.Collections.Generic;
using UnityEngine;

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
        public ProcessingUnit PrevUnit;
        public List<Unit_ConveyerBelt> Conveyers = new();
        public List<ConverterDirectionSetting> ConverterDirections = new();
        public ProcessingUnit NextUnit;
        public ConnectionType Type;
    }

    [Serializable]
    public class ConverterDirectionSetting
    {
        public ConverterConveyer Converter;
        public Vector3 LocalDirection = Vector3.right;
    }

    [Serializable]
    public class ProductionPath
    {
        public string Name;
        public List<LineConnection> Connections = new();
    }
}
