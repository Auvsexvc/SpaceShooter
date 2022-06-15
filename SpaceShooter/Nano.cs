using System;

namespace SpaceShooter
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