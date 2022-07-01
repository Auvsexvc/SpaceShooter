using System;
using System.Diagnostics;

namespace SpaceShooter.Classes
{
    internal class Enemy : GameObject
    {
        private const int EnemyInitSpeed = 6;

        public Enemy(int speed, int level)
        {
            Random rnd = new Random();
            Speed = rnd.Next(EnemyInitSpeed, speed + 1 + (int)Math.Ceiling((double)level / speed));
            Guid = Guid.NewGuid();
            Tracking = rnd.Next(1, 7) <= 3;
            Shooting = rnd.Next(1, 7) <= 3;
            EvadesLeft = rnd.Next(1, 7) <= 3;
            FireDelay = new Stopwatch();
            FireDelay.Start();
            RateOfFire = rnd.Next(10, 25);
        }

        public bool EvadesLeft { get; }
        public Stopwatch FireDelay { get; }
        public int RateOfFire { get; }
        public bool Shooting { get; }
        public bool TakesEvasiveManeuver { get; set; }
        public bool Tracking { get; }
    }
}