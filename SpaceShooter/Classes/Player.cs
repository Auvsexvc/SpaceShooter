namespace SpaceShooter.Classes
{
    internal class Player : GameObject
    {
        public Player()
        {
            Speed = 14;
            Damage = 0;
        }

        public int Damage { get; set; }
        public bool MoveDown { get; set; }
        public bool MoveLeft { get; set; }
        public bool MoveRight { get; set; }
        public bool MoveUp { get; set; }
    }
}