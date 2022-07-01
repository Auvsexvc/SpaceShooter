using System;

namespace SpaceShooter.Classes
{
    internal class Nano : GameObject
    {
        public Nano(int speed)
        {
            Speed = speed;
            Guid = Guid.NewGuid();
        }
    }
}