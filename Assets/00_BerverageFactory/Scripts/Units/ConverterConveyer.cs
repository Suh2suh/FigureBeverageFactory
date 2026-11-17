using UnityEngine;

namespace FBFactory
{
    public class ConverterConveyer : Unit_ConveyerBelt
    {
        public void SetMoveDirection(Vector3 localDirection)
        {
            SetLocalMoveDirection(localDirection);
        }
    }
}
