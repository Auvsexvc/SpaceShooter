using System;

namespace SpaceShooter
{
    internal class UnindentifiedFlyingObject
    {
        public Guid Guid { get; set; }
        public int Speed { get; set; }
        public bool Tracking { get; set; }
        public bool Shooting { get; set; }
        public bool Evasion { get; set; }
        public bool TakesEvasiveManeuver { get; set; }
    }
}