﻿using System;

namespace SpaceShooter
{
    internal class Enemy : UnindentifiedFlyingObject
    {
        public Enemy(int speed)
        {
            Speed = speed;
            Guid = Guid.NewGuid();
        }
    }
}