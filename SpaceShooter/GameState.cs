using System;
using System.Collections.Generic;

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
        private const int EnemyBulletInitSpeed = 15;
        private readonly Random rnd = new();

        private int enemyLimit;
        private int nanoLimit;
        private int enemySpeed;
        private int nanoSpeed;
        private int enemyCounter;
        private int nanoCounter;
        private int level;
        private bool levelRised;

        public List<UnindentifiedFlyingObject> Ufos { get; } = new();
        public int EnemyCounter { get => enemyCounter; }
        public int EnemySpeed { get => enemySpeed; }
        public int NanoSpeed { get => nanoSpeed; }
        public int Score { get; set; }
        public int Damage { get; set; }
        public int Level { get => level; }
        public static int PlayerSpeed { get => PlayerInitSpeed; }
        public static int BulletSpeed { get => BulletInitSpeed; }
        public static int EnemyBulletSpeed { get => EnemyBulletInitSpeed; }

        public int EnemyLimit { get => enemyLimit; }

        public event Action<Enemy>? TriggerSpawnEnemyModel;

        public event Action<Nano>? TriggerSpawnNanoModel;

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

        public bool IsPlayerDestroyed()
        {
            return Damage > 99;
        }

        public void GameOver()
        {
            Damage = 100;
            GameEnded?.Invoke();
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

        public void SpawnNano()
        {
            Nano newNano = new(rnd.Next(NanoInitSpeed, nanoSpeed + 1 + (int)Math.Ceiling((double)level / NanoInitSpeed)));
            Ufos!.Add(newNano);
            TriggerSpawnNanoModel?.Invoke(newNano);
        }

        public void SpawnEnemy()
        {
            Enemy newEnemy = new(rnd.Next(EnemyInitSpeed, enemySpeed + 1 + (int)Math.Ceiling((double)level / enemySpeed)), rnd.Next(1, 7) <= 3, rnd.Next(1, 7) <= 3, rnd.Next(1, 7) <= 3);
            Ufos!.Add(newEnemy);
            TriggerSpawnEnemyModel?.Invoke(newEnemy);
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
            Ufos.Clear();
            GameRestarted?.Invoke();
        }
    }
}