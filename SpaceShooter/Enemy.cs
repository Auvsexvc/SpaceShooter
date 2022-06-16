using System;

namespace SpaceShooter
{
    internal class Enemy : UnindentifiedFlyingObject
    {
        public Enemy(int speed, bool homing = false, bool shooting = false, bool evasion = false)
        {
            Speed = speed;
            Guid = Guid.NewGuid();
            Tracking = homing;
            Shooting = shooting;
            EvadesLeft = evasion;
        }
    }
}