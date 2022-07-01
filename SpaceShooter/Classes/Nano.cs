using System;

namespace SpaceShooter.Classes
{
    internal class Nano : UnindentifiedFlyingObject
    {
        public Nano(int speed)
        {
            Speed = speed;
            Guid = Guid.NewGuid();
        }
    }
}