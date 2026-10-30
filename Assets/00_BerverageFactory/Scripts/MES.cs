using System.Collections.Generic;
using UnityEngine;

namespace FBFactory
{
    public class MES : SingletonBase<MES>
    {
        [SerializeField] private LineController _lineController;
        [SerializeField] private List<OrderData> _orders;
        [SerializeField, ReadOnly] private List<OrderProductionStatus> _orderStatus;
        private bool _isInit = false;


        protected override void Awake()
        {
            base.Awake();

            Initialize();
        }

        private void Initialize()
        {
            if (_isInit)
                return;

            for (int id = 0; id < _orders.Count; id++) 
            {
                var order = _orders[id];
                order.ID = id;
                _orderStatus.Add(new()
                {
                    Order = order,
                    Yield = 0,
                    Scrap = 0,
                });
            }

            _lineController.InitOrders(_orders);

            _isInit = true;
        }


        public void OnProduced(int orderID)
        {
            _orderStatus[orderID].Yield++;
        }

        public void OnRejected(int orderID)
        {
            _orderStatus[orderID].Scrap++;
        }
    }


    [System.Serializable]
    class OrderProductionStatus
    {
        public OrderData Order;
        public int Yield;
        public int Scrap;
    }

    [System.Serializable]
    public class OrderData
    {
        public int ID;
        [field: SerializeField] public BOMScriptableObject BOM { get; private set; }
        [field: SerializeField] public int TargetQuantity { get; private set; }
    }
}
