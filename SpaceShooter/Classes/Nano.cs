using System;

namespace SpaceShooter.Classes
{
    internal class Nano : GameObject
    {
        private const int NanoInitSpeed = 6;

        public Nano(int speed, int level)
        {
            Random rnd = new Random();
            Speed = rnd.Next(NanoInitSpeed, speed + 1 + (int)Math.Ceiling((double)level / NanoInitSpeed));
            Guid = Guid.NewGuid();
        }
    }
}