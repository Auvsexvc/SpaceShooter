using System;

namespace SpaceShooter
{
    internal class Enemy
    {
        public int Speed { get; set; }
        public Guid Guid { get; set; }

        public Enemy(int speed)
        {
            Speed = speed;
            Guid = Guid.NewGuid();
        }
    }
}