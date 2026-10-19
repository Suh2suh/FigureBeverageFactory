using System;
using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class BeveragePallet : MonoBehaviour
    {
        [SerializeField] private List<BeveragePackData> _packs = new();
        [SerializeField, ReadOnly] private bool _isStretched;

        public List<BeveragePackData> Packs => _packs;
        public bool IsStretched => _isStretched;


        public void AddToPallet(BeveragePack pack)
        {
            _packs.Add(new BeveragePackData(pack));
        }


        public void SetStretched()
        {
            _isStretched = true;
        }


        [Serializable]
        public struct BeveragePackData
        {
            [SerializeField] private List<BeveragePack.BeverageData> _beverages;

            public BeveragePackData(BeveragePack pack)
            {
                _beverages = new List<BeveragePack.BeverageData>(pack.Beverages);
            }
        }
    }
}
