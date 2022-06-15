using System;

namespace SpaceShooter
{
    internal class GameState
    {
        private const int EnemyInitLimit = 60;
        private const int NanoInitLimit = 150;
        private const int EnemyInitSpeed = 6;
        private const int NanoInitSpeed = 6;
        private const int PlayerInitSpeed = 14;
        private const int BulletInitSpeed = 20;
        private int enemyLimit;
        private int nanoLimit;
        private int enemySpeed;
        private int nanoSpeed;
        private int enemyCounter;
        private int nanoCounter;
        private int level;
        private bool levelRised;

        public int EnemyCounter { get => enemyCounter; }
        public int EnemySpeed { get => enemySpeed; }
        public int NanoSpeed { get => nanoSpeed; }
        public int Score { get; set; }
        public int Damage { get; set; }
        public int Level { get => level; }
        public static int PlayerSpeed { get => PlayerInitSpeed; }
        public int BulletSpeed { get => BulletInitSpeed; }
        public int EnemyLimit { get => enemyLimit; }

        public event Action? EnemiesCounted;

        public event Action? NanosCounted;

        public event Action? GameEnded;

        public event Action? GameRestarted;

        public GameState()
        {
            Score = 0;
            Damage = 0;
            level = 0;
            enemyCounter = EnemyInitLimit;
            enemySpeed = EnemyInitSpeed;
            enemyLimit = EnemyInitLimit;
            nanoSpeed = NanoInitSpeed;
            nanoLimit = NanoInitLimit;
            nanoCounter = NanoInitLimit;
        }

        public void PlayerDestroyed()
        {
            if (Damage > 99)
            {
                Damage = 100;
                GameEnded?.Invoke();
            }
        }

        public void CountDownToEnemySpawn()
        {
            enemyCounter--;

            if (enemyCounter < 0)
            {
                EnemiesCounted?.Invoke();
                enemyCounter = enemyLimit;
            }
        }

        public void CountDownToNanoSpawn()
        {
            nanoCounter--;

            if (nanoCounter < 0)
            {
                NanosCounted?.Invoke();
                nanoCounter = nanoLimit;
            }
        }

        public void MakeGameHarder()
        {
            if (Score % 5 != 0 && levelRised)
            {
                levelRised = false;
            }
            if (Score % 5 == 0 && !levelRised)
            {
                if (Score != 0 && Score % 10 == 0)
                {
                    enemySpeed = EnemyInitSpeed + (int)Math.Ceiling((double)Level / (double)EnemyInitSpeed);
                }
                else
                {
                    enemyLimit = EnemyInitLimit - ((int)Math.Ceiling((double)Level / ((double)EnemyInitLimit / 10)) * 10) - (int)Math.Ceiling((EnemyInitLimit - Level) / 10.0);
                    if (enemyLimit < 2)
                    {
                        enemyLimit = 1;
                    }
                }
                level++;
                levelRised = true;
            }
        }

        public void Reset()
        {
            Score = 0;
            Damage = 0;
            level = 0;
            enemySpeed = EnemyInitSpeed;
            enemyLimit = EnemyInitLimit;
            nanoSpeed = NanoInitSpeed;
            nanoLimit = NanoInitLimit;
            GameRestarted?.Invoke();
        }
    }
}