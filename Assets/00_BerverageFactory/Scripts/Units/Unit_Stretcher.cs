using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class Unit_Stretcher : UnitBase
    {
        private readonly List<BeveragePallet> _beveragePallets = new();


        protected override void ExecuteProduction()
        {
            if (_beveragePallets.Count == 0)
                return;

            BeveragePallet beveragePallet = _beveragePallets[0];
            _beveragePallets.RemoveAt(0);
            beveragePallet.SetStretched();
        }


        private void OnTriggerEnter(Collider other)
        {
            BeveragePallet beveragePallet = other.GetComponentInChildren<BeveragePallet>();

            if (beveragePallet != null && !_beveragePallets.Contains(beveragePallet))
                _beveragePallets.Add(beveragePallet);
        }


        private void OnTriggerExit(Collider other)
        {
            BeveragePallet beveragePallet = other.GetComponentInChildren<BeveragePallet>();

            if (beveragePallet != null)
                _beveragePallets.Remove(beveragePallet);
        }


        protected override void OnAborting_PackML()
        {
            StateComplete();
        }

        protected override void OnClearing_PackML()
        {
            StateComplete();
        }

        protected override void OnResetting_PackML()
        {
            StateComplete();
        }

        protected override void OnStarting_PackML()
        {
            StateComplete();
        }

        protected override void OnStopping_PackML()
        {
            StateComplete();
        }
    }
}
