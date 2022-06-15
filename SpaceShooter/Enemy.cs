using System;

namespace SpaceShooter
{
    internal class Enemy : UnindentifiedFlyingObject
    {
        public Enemy(int speed, bool homing = false)
        {
            Speed = speed;
            Guid = Guid.NewGuid();
            Homing = homing;
        }
    }
}