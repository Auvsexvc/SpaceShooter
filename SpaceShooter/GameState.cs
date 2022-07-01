using System;
using System.Collections.Generic;

namespace SpaceShooter
{
    internal class GameState
    {
        private const int AsteroidLimit = 900;
        private const int BulletInitSpeed = 20;
        private const int EnemyBulletInitSpeed = 15;
        private const int EnemyInitLimit = 60;
        private const int EnemyInitSpeed = 6;
        private const int NanoInitLimit = 750;
        private const int NanoInitSpeed = 6;
        private const int PlayerInitSpeed = 14;
        private readonly Random rnd = new();

        private int asteroidCounter;
        private int enemyCounter;
        private int enemyLimit;
        private int enemySpeed;
        private int level;
        private bool levelRised;
        private int nanoCounter;
        private int nanoLimit;
        private int nanoSpeed;

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

        public event Action? GameEnded;

        public event Action? GameRestarted;

        public event Action? TriggerSpawnAsteroidModel;

        public event Action<UnindentifiedFlyingObject>? TriggerSpawnModel;

        public static int BulletSpeed { get => BulletInitSpeed; }
        public static int EnemyBulletSpeed { get => EnemyBulletInitSpeed; }
        public static int PlayerSpeed { get => PlayerInitSpeed; }
        public int Damage { get; set; }
        public int EnemyCounter { get => enemyCounter; }
        public int EnemyLimit { get => enemyLimit; }
        public int EnemySpeed { get => enemySpeed; }
        public bool IsGameOver { get; set; }
        public int Level { get => level; }
        public int NanoSpeed { get => nanoSpeed; }
        public int Score { get; set; }
        public List<UnindentifiedFlyingObject> Ufos { get; } = new();

        public void CountDownToAsteroidSpawn()
        {
            asteroidCounter--;

            if (asteroidCounter < 0)
            {
                SpawnAsteroid();
                asteroidCounter = AsteroidLimit;
            }
        }

        public void CountDownToEnemySpawn()
        {
            enemyCounter--;

            if (enemyCounter < 0)
            {
                SpawnEnemy();
                enemyCounter = enemyLimit;
            }
        }

        public void CountDownToNanoSpawn()
        {
            nanoCounter--;

            if (nanoCounter < 0)
            {
                SpawnNano();
                nanoCounter = nanoLimit;
            }
        }

        public void GameOver()
        {
            Damage = 100;
            IsGameOver = true;
            GameEnded?.Invoke();
        }

        public bool IsPlayerDestroyed()
        {
            return Damage > 99;
        }

        public void MakeGameHarder()
        {
            if (Score % 5 != 0 && levelRised)
            {
                levelRised = false;
            }
            else if (Score % 5 == 0 && !levelRised)
            {
                if (Score != 0 && Score % 10 == 0)
                {
                    enemySpeed = EnemyInitSpeed + (int)Math.Ceiling((double)Level / EnemyInitSpeed);
                }
                else
                {
                    enemyLimit = EnemyInitLimit - ((int)Math.Ceiling(Level / (EnemyInitLimit / 10.0)) * 10) - (int)Math.Ceiling((EnemyInitLimit - Level) / 10.0);
                    if (enemyLimit < 11)
                    {
                        enemyLimit = 10;
                    }
                }
                level++;
                levelRised = true;
            }
        }

        public void RemoveUfoByUid(string uid) =>
            Ufos.Remove(Ufos.Find(e => e.Guid.ToString() == uid)!);

        public void Reset()
        {
            Score = 0;
            Damage = 0;
            level = 0;
            enemySpeed = EnemyInitSpeed;
            enemyLimit = EnemyInitLimit;
            nanoSpeed = NanoInitSpeed;
            nanoLimit = NanoInitLimit;
            IsGameOver = false;
            Ufos.Clear();
            GameRestarted?.Invoke();
        }

        public void SpawnAsteroid()
        {
            TriggerSpawnAsteroidModel?.Invoke();
        }

        public void SpawnEnemy()
        {
            Enemy newEnemy = new(rnd.Next(EnemyInitSpeed, enemySpeed + 1 + (int)Math.Ceiling((double)level / enemySpeed)), rnd.Next(1, 7) <= 3, rnd.Next(1, 7) <= 3, rnd.Next(1, 7) <= 3);
            Ufos!.Add(newEnemy);
            TriggerSpawnModel?.Invoke(newEnemy);
        }

        public void SpawnNano()
        {
            Nano newNano = new(rnd.Next(NanoInitSpeed, nanoSpeed + 1 + (int)Math.Ceiling((double)level / NanoInitSpeed)));
            Ufos!.Add(newNano);
            TriggerSpawnModel?.Invoke(newNano);
        }
    }
}