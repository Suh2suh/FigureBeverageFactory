using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class Unit_Palletizer : UnitBase
    {
        [SerializeField] private int _packPerPallet = 8;
        [SerializeField] private GameObject _beveragePalletPrefab;

        private readonly List<BeveragePack> _beveragePacks = new();


        protected override void ExecuteProduction()
        {
            if (_beveragePacks.Count < _packPerPallet)
                return;

            BeveragePallet beveragePallet = Instantiate(_beveragePalletPrefab, transform.position, Quaternion.identity)
                                            .GetComponent<BeveragePallet>();
            for (int i = 0; i < _packPerPallet; i++)
            {
                BeveragePack beveragePack = _beveragePacks[0];

                beveragePallet.AddToPallet(beveragePack);
                _beveragePacks.RemoveAt(0);
                Destroy(beveragePack.gameObject);
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            BeveragePack beveragePack = other.GetComponentInChildren<BeveragePack>();

            if (beveragePack != null && !_beveragePacks.Contains(beveragePack))
                _beveragePacks.Add(beveragePack);
        }


        private void OnTriggerExit(Collider other)
        {
            BeveragePack beveragePack = other.GetComponentInChildren<BeveragePack>();

            if (beveragePack != null)
                _beveragePacks.Remove(beveragePack);
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
