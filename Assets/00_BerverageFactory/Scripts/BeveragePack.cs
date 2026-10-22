using System;
using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class BeveragePack : FactoryMaterial
    {
        [SerializeField] private List<BeverageData> _beverages = new();

        public List<BeverageData> Beverages => _beverages;


        public void AddToPack(Beverage beverage)
        {
            _beverages.Add(new BeverageData(beverage));
        }


        [Serializable]
        public struct BeverageData
        {
            [SerializeField] private int _maxML;
            [SerializeField] private int _filledML;
            [SerializeField] private FlavorType _flavor;
            [SerializeField] private bool _hasCap;
            [SerializeField] private Material _label;
            [SerializeField] private string _code;

            public BeverageData(Beverage beverage)
            {
                _maxML = beverage.MaxML;
                _filledML = beverage.FilledML;
                _flavor = beverage.Flavor;
                _hasCap = beverage.HasCap;
                _label = beverage.Label;
                _code = beverage.Code;
            }
        }
    }
}
