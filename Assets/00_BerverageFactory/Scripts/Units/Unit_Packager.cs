using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class Unit_Packager : UnitBase
    {
        [SerializeField] private int _beveragePerPack = 6;
        [SerializeField] private GameObject _beveragePackPrefab;
        [SerializeField] private Transform _pointPackedBottle;

        private readonly List<Beverage> _beverages = new();


        protected override void ExecuteProduction()
        {
            if (_beverages.Count < _beveragePerPack)
                return;

            var beveragePack = Instantiate(_beveragePackPrefab, _pointPackedBottle.position, Quaternion.identity)
                               .GetComponent<BeveragePack>();
            for (int i = 0; i < _beveragePerPack; i++)
            {
                Beverage beverage = _beverages[0];

                beveragePack.AddToPack(beverage);
                _beverages.RemoveAt(0);
                Destroy(beverage.gameObject);
            }

            beveragePack.transform.position = _pointPackedBottle.position;
        }


        private void OnTriggerEnter(Collider other)
        {
            Beverage beverage = other.GetComponentInChildren<Beverage>();

            if (beverage != null && !_beverages.Contains(beverage))
                _beverages.Add(beverage);
        }


        private void OnTriggerExit(Collider other)
        {
            Beverage beverage = other.GetComponentInChildren<Beverage>();

            if (beverage != null)
                _beverages.Remove(beverage);
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
