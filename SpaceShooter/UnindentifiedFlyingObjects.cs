using System;

namespace SpaceShooter
{
    internal abstract class UnindentifiedFlyingObject
    {
        public bool EvadesLeft { get; set; }
        public Guid Guid { get; set; }
        public bool Shooting { get; set; }
        public int Speed { get; set; }
        public bool TakesEvasiveManeuver { get; set; }
        public bool Tracking { get; set; }
    }
}