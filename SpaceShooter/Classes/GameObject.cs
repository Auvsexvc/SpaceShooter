using System;

namespace SpaceShooter.Classes
{
    internal abstract class GameObject
    {
        public Guid Guid { get; set; }

        public int Speed { get; set; }
    }
}