using SpaceShooter.Classes;
using System;
using System.Collections.Generic;

namespace SpaceShooter
{
    internal class GameState
    {
        private const int AsteroidLimit = 900;
        private const int EnemyInitLimit = 60;
        private const int EnemyInitSpeed = 6;
        private const int LaserInitSpeed = 20;
        private const int NanoInitLimit = 750;
        private const int NanoInitSpeed = 6;
        private readonly List<GameObject> _gameObjects;
        private readonly Player _player;
        private int asteroidCounter;
        private int enemyCounter;
        private int enemyLimit;
        private int enemySpeed;
        private bool levelRised;
        private int nanoCounter;
        private int nanoLimit;
        private int nanoSpeed;

        public GameState()
        {
            Score = 0;
            Level = 0;
            enemyCounter = EnemyInitLimit;
            enemySpeed = EnemyInitSpeed;
            enemyLimit = EnemyInitLimit;
            nanoSpeed = NanoInitSpeed;
            nanoLimit = NanoInitLimit;
            nanoCounter = NanoInitLimit;
            LaserSpeed = LaserInitSpeed;
            _gameObjects = new();
            _player = new Player();
        }

        public event Action? GameEnded;

        public event Action? GameRestarted;

        public event Action? TriggerSpawnAsteroidModel;

        public event Action<GameObject>? TriggerSpawnModel;

        public bool IsGameOver { get; set; }
        public int LaserSpeed { get; }
        public int Level { get; private set; }

        public int Score { get; set; }

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
            _player.Damage = 100;
            IsGameOver = true;
            GameEnded?.Invoke();
        }

        public List<GameObject> GetGameObjects()
        {
            return _gameObjects;
        }

        public Player GetPlayer()
        {
            return _player;
        }

        public bool IsPlayerDestroyed()
        {
            return _player.Damage > 99;
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
                Level++;
                levelRised = true;
            }
        }

        public void RemoveGameObjectsByUid(string uid) =>
            _gameObjects.Remove(_gameObjects.Find(e => e.Guid.ToString() == uid)!);

        public void Reset()
        {
            Score = 0;
            _player.Damage = 0;
            Level = 0;
            enemySpeed = EnemyInitSpeed;
            enemyLimit = EnemyInitLimit;
            nanoSpeed = NanoInitSpeed;
            nanoLimit = NanoInitLimit;
            IsGameOver = false;
            _gameObjects.Clear();
            GameRestarted?.Invoke();
        }

        public void SpawnAsteroid()
        {
            TriggerSpawnAsteroidModel?.Invoke();
        }

        public void SpawnEnemy()
        {
            Enemy newEnemy = new(enemySpeed, Level);
            _gameObjects!.Add(newEnemy);
            TriggerSpawnModel?.Invoke(newEnemy);
        }

        public void SpawnNano()
        {
            Nano newNano = new(nanoSpeed, Level);
            _gameObjects!.Add(newNano);
            TriggerSpawnModel?.Invoke(newNano);
        }
    }
}