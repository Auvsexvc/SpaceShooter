using System;

namespace SpaceShooter.Classes
{
    internal class Enemy : GameObject
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