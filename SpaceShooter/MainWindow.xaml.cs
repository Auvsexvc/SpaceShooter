using SpaceShooter.Classes;
using SpaceShooter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpaceShooter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GameState _gameState;
        private readonly List<Rectangle> _garbageCollector;
        private readonly PlayerModel _playerModel;
        private readonly Random rnd = new();

        public MainWindow()
        {
            InitializeComponent();
            _gameState = new();
            _playerModel = new PlayerModel(_gameState.GetPlayer());
            _gameState.TriggerSpawnModel += OnGameObjectSpawn;
            _gameState.TriggerSpawnAsteroidModel += OnAsteroidSpawn;
            _gameState.GameEnded += OnGameEnded;
            _gameState.GameRestarted += OnGameRestarted;

            _garbageCollector = new();
            _ = SetUpGame();
        }

        private void ButtonPlayAgainClick(object sender, RoutedEventArgs e)
        {
            _gameState.Reset();
        }

        private void ButtonQuitClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CleanUpGarbageCollector()
        {
            foreach (Rectangle item in _garbageCollector)
            {
                _gameState.RemoveGameObjectByUid(item.Uid);
                GameCanvas.Children.Remove(item);
            }
        }

        private void ClearGameCanvas()
        {
            foreach (Rectangle item in GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Bullet" || (string)rect.Tag == "Enemy" || (string)rect.Tag == "Nano" || (string)rect.Tag == "Player").ToList())
            {
                GameCanvas.Children.Remove(item);
            }
        }

        private void DrawParallaxStarField()
        {
            Canvas.SetBottom(background1, Canvas.GetBottom(background1) - 5);
            Canvas.SetBottom(background2, Canvas.GetBottom(background2) - 5);
            if (Canvas.GetBottom(background1) < -707)
            {
                Canvas.SetBottom(background1, Canvas.GetBottom(background2) + background2.Height);
            }

            if (Canvas.GetBottom(background2) < -707)
            {
                Canvas.SetBottom(background2, Canvas.GetBottom(background1) + background1.Height);
            }
        }

        private void EnemyEvades(Rectangle uRect, Rectangle bullet)
        {
            Enemy? uObj = _gameState.GetGameObjects().Find(u => u.Guid.ToString() == uRect.Uid) as Enemy;
            uObj!.TakesEvasiveManeuver = false;

            if (Canvas.GetTop(uRect) > Canvas.GetTop(bullet) - (GameCanvas.Width - (uRect.Width / 2)) && Canvas.GetLeft(uRect) - (uRect.Width / 2) <= Canvas.GetLeft(bullet) && Canvas.GetLeft(uRect) + uRect.Width + (uRect.Width / 2) >= Canvas.GetLeft(bullet) + bullet.Width)
            {
                if (uObj!.EvadesLeft)
                {
                    if (Canvas.GetLeft(uRect) - uObj!.Speed > uRect.Width / 2)
                    {
                        uObj.TakesEvasiveManeuver = true;
                        Canvas.SetLeft(uRect, Canvas.GetLeft(uRect) - (uObj!.Speed - 2));
                    }
                }
                else
                {
                    if (Canvas.GetLeft(uRect) - uObj!.Speed < GameCanvas.Width - (uRect.Width + (uRect.Width / 2)))
                    {
                        uObj.TakesEvasiveManeuver = true;
                        Canvas.SetLeft(uRect, Canvas.GetLeft(uRect) + (uObj!.Speed - 2));
                    }
                }

                if (uObj.Shooting)
                {
                    EnemyShoots(uObj);
                }
            }
        }

        private void EnemyIsAShooter(Enemy uObj, Rectangle uRect)
        {
            if (Canvas.GetTop(uRect) > Canvas.GetTop(_playerModel.GetUIElement()) - GameCanvas.Height && Canvas.GetTop(uRect) < Canvas.GetTop(_playerModel.GetUIElement()) && uObj.Shooting && !uObj.TakesEvasiveManeuver)
            {
                if (Canvas.GetLeft(uRect) < Canvas.GetLeft(_playerModel.GetUIElement()))
                {
                    Canvas.SetLeft(uRect, Canvas.GetLeft(uRect) + uObj!.Speed);
                }
                else if (Canvas.GetLeft(uRect) > Canvas.GetLeft(_playerModel.GetUIElement()) + (_playerModel.GetShape().Width / 4))
                {
                    Canvas.SetLeft(uRect, Canvas.GetLeft(uRect) - uObj!.Speed);
                }
                else
                {
                    EnemyShoots(uObj);
                }
            }
        }

        private void EnemyIsTrackingPlayer(Enemy enemyObject, Rectangle uRect)
        {
            if (Canvas.GetTop(uRect) > Canvas.GetTop(_playerModel.GetUIElement()) - 150 && Canvas.GetTop(uRect) < Canvas.GetTop(_playerModel.GetUIElement()) && enemyObject.Tracking && !enemyObject.Shooting && !enemyObject.TakesEvasiveManeuver)
            {
                if (Canvas.GetLeft(uRect) < Canvas.GetLeft(_playerModel.GetUIElement()))
                {
                    Canvas.SetLeft(uRect, Canvas.GetLeft(uRect) + enemyObject!.Speed);
                }
                else if (Canvas.GetLeft(uRect) > Canvas.GetLeft(_playerModel.GetUIElement()) + (_playerModel.GetShape().Width / 4))
                {
                    Canvas.SetLeft(uRect, Canvas.GetLeft(uRect) - enemyObject!.Speed);
                }
                else
                {
                    if (enemyObject.Shooting)
                    {
                        EnemyShoots(enemyObject);
                    }
                }
            }
        }

        private void EnemyShoots(Enemy enemyObject)
        {
            if (enemyObject.FireDelay.ElapsedMilliseconds % enemyObject.RateOfFire == 0)
            {
                SpawnBullet(enemyObject);
            }
        }

        private async Task FadeIn(UIElement e)
        {
            DoubleAnimation fadeInAnimation = new()
            {
                Duration = TimeSpan.FromMilliseconds(600),
                From = 0,
                To = 1
            };

            e.BeginAnimation(OpacityProperty, fadeInAnimation);
            await Task.Delay(fadeInAnimation.Duration.TimeSpan);
            if (e == GameOverMenu)
            {
                e.Visibility = Visibility.Visible;
            }
            else
            {
                e.Opacity = 1;
            }
        }

        private async Task FadeOut(UIElement e)
        {
            DoubleAnimation fadeOutAnimation = new()
            {
                Duration = TimeSpan.FromMilliseconds(2000),
                From = 1,
                To = 0.1
            };

            e.BeginAnimation(OpacityProperty, fadeOutAnimation);
            await Task.Delay(fadeOutAnimation.Duration.TimeSpan);
            if (e == GameOverMenu)
            {
                e.Visibility = Visibility.Hidden;
            }
            else
            {
                e.Opacity = 0.5;
            }
        }

        private async void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            await GameLoop();
        }

        private async Task GameLoop()
        {
            while (!_gameState.IsGameOver)
            {
                await Task.Delay(16);

                DrawParallaxStarField();

                _gameState.CountDownToEnemySpawn();

                _gameState.CountDownToNanoSpawn();
                _gameState.CountDownToAsteroidSpawn();

                MoveAsteroids();

                MovePlayer();

                RevertPlayerModel();

                _ = GetBulletCollision();

                MoveBullets();

                MoveObjects();

                GetPlayerCollision();

                UpdateDamage();

                UpdateScore();

                CleanUpGarbageCollector();

                _gameState.MakeGameHarder();

                if (_gameState.IsPlayerDestroyed())
                {
                    _gameState.GameOver();
                }
            }
        }

        private async Task GetBulletCollision()
        {
            foreach (Rectangle bullet in GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Bullet"))
            {
                ImageBrush _boomSprite = new()
                {
                    ImageSource = new BitmapImage(new Uri("Assets/boom.png", UriKind.Relative))
                };
                Rect bulletHitBox = new(Canvas.GetLeft(bullet), Canvas.GetTop(bullet), bullet.Width, bullet.Height);

                if (Canvas.GetTop(bullet) < 60)
                {
                    _garbageCollector.Add(bullet);
                }

                foreach (Rectangle nano in GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Nano"))
                {
                    Rect nanoHit = new(Canvas.GetLeft(nano), Canvas.GetTop(nano), nano.Width, nano.Height);

                    if (bulletHitBox.IntersectsWith(nanoHit))
                    {
                        nano.Fill = _boomSprite;
                        _garbageCollector.Add(bullet);
                        await Task.Delay(100);
                        _garbageCollector.Add(nano);
                        _gameState.SpawnEnemy();
                    }
                }
                foreach (Rectangle enemy in GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Enemy"))
                {
                    Rect enemyHit = new(Canvas.GetLeft(enemy), Canvas.GetTop(enemy), enemy.Width, enemy.Height);

                    EnemyEvades(enemy, bullet);

                    if (bulletHitBox.IntersectsWith(enemyHit))
                    {
                        enemy.Fill = _boomSprite;
                        _garbageCollector.Add(bullet);
                        _gameState.Score++;
                        await Task.Delay(100);
                        _garbageCollector.Add(enemy);
                    }
                }
            }
        }

        private void GetPlayerCollision()
        {
            foreach (Rectangle item in GameCanvas.Children.OfType<Rectangle>())
            {
                ImageBrush shieldSprite = new()
                {
                    ImageSource = new BitmapImage(new Uri("Assets/playerShield.png", UriKind.Relative))
                };
                Rect playerHitBox = new Rect(Canvas.GetLeft(_playerModel.GetUIElement()), Canvas.GetTop(_playerModel.GetUIElement()), _playerModel.GetShape().Width, _playerModel.GetShape().Height);
                if ((string)item.Tag == "Enemy")
                {
                    Rect enemyHitBox = new(Canvas.GetLeft(item), Canvas.GetTop(item), item.Width, item.Height);

                    if (playerHitBox.IntersectsWith(enemyHitBox))
                    {
                        _playerModel.GetShape().Fill = shieldSprite;
                        _gameState.GetPlayer().Damage++;
                    }
                    else if (Canvas.GetTop(item) > 650)
                    {
                        _garbageCollector.Add(item);
                        _gameState.GetPlayer().Damage += 10;
                    }
                }

                if ((string)item.Tag == "EnemyBullet")
                {
                    Rect enemyHitBox = new(Canvas.GetLeft(item), Canvas.GetTop(item), item.Width, item.Height);

                    if (playerHitBox.IntersectsWith(enemyHitBox))
                    {
                        _playerModel.GetShape().Fill = shieldSprite;
                        _gameState.GetPlayer().Damage += 10;
                        _garbageCollector.Add(item);
                    }
                    else if (Canvas.GetTop(item) > 650)
                    {
                        _garbageCollector.Add(item);
                    }
                }

                if ((string)item.Tag == "Nano")
                {
                    Rect nanoHitBox = new(Canvas.GetLeft(item), Canvas.GetTop(item), item.Width, item.Height);

                    if (playerHitBox.IntersectsWith(nanoHitBox) && _gameState.GetPlayer().Damage > 0)
                    {
                        _garbageCollector.Add(item);
                        if (_gameState.GetPlayer().Damage >= 15)
                        {
                            _gameState.GetPlayer().Damage -= 15;
                        }
                        else
                        {
                            _gameState.GetPlayer().Damage = 0;
                        }
                    }
                    else if (Canvas.GetTop(item) > 650)
                    {
                        _garbageCollector.Add(item);
                    }
                }
            }
        }

        private void MoveAsteroids()
        {
            foreach (Rectangle nano in GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Asteroids"))
            {
                Canvas.SetTop(nano, Canvas.GetTop(nano) + 5);
            }
        }

        private void MoveBullets()
        {
            foreach (Rectangle bullet in GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Bullet" || (string)rect.Tag == "EnemyBullet"))
            {
                if ((string)bullet.Tag == "EnemyBullet")
                {
                    Canvas.SetTop(bullet, Canvas.GetTop(bullet) + _gameState.LaserSpeed);
                }
                else
                {
                    Canvas.SetTop(bullet, Canvas.GetTop(bullet) - _gameState.LaserSpeed);
                }
            }
        }

        private void MoveObjects()
        {
            foreach (GameObject gameObject in _gameState.GetGameObjects())
            {
                Rectangle enemyRect = GameCanvas.Children.OfType<Rectangle>().FirstOrDefault(rect => gameObject.Guid.ToString() == rect.Uid)!;
                if (enemyRect is not null)
                {
                    Canvas.SetTop(enemyRect, Canvas.GetTop(enemyRect) + gameObject!.Speed);
                    if (gameObject is Enemy enemyObject)
                    {
                        EnemyIsTrackingPlayer(enemyObject, enemyRect);
                        EnemyIsAShooter(enemyObject, enemyRect);
                    }
                }
            }
        }

        private void MovePlayer()
        {
            if (_gameState.GetPlayer().MoveLeft && Canvas.GetLeft(_playerModel.GetUIElement()) > _playerModel.GetShape().Width / 2)
            {
                Canvas.SetLeft(_playerModel.GetUIElement(), Canvas.GetLeft(_playerModel.GetUIElement()) - _gameState.GetPlayer().Speed);
            }
            if (_gameState.GetPlayer().MoveRight && Canvas.GetLeft(_playerModel.GetUIElement()) + _playerModel.GetShape().Width < GameCanvas.Width - (_playerModel.GetShape().Width / 2))
            {
                Canvas.SetLeft(_playerModel.GetUIElement(), Canvas.GetLeft(_playerModel.GetUIElement()) + _gameState.GetPlayer().Speed);
            }
            if (_gameState.GetPlayer().MoveUp && Canvas.GetTop(_playerModel.GetUIElement()) > _playerModel.GetShape().Height * 4)
            {
                Canvas.SetTop(_playerModel.GetUIElement(), Canvas.GetTop(_playerModel.GetUIElement()) - (_gameState.GetPlayer().Speed / 2));
            }
            if (_gameState.GetPlayer().MoveDown && Canvas.GetTop(_playerModel.GetUIElement()) + _playerModel.GetShape().Height < GameCanvas.Height - (_playerModel.GetShape().Height / 2))
            {
                Canvas.SetTop(_playerModel.GetUIElement(), Canvas.GetTop(_playerModel.GetUIElement()) + (_gameState.GetPlayer().Speed / 2));
            }
        }

        private void OnAsteroidSpawn()
        {
            SpawnAsteroidModel();
        }

        private async void OnGameEnded()
        {
            ImageBrush _boomSprite = new()
            {
                ImageSource = new BitmapImage(new Uri("Assets/boom.png", UriKind.Relative))
            };
            _playerModel.GetShape().Fill = _boomSprite;
            Damage.Content = $"Damage: {_gameState.GetPlayer().Damage}";
            Damage.Foreground = Brushes.Red;
            await Task.Delay(200);
            _playerModel.GetShape().Fill = new ImageBrush(new BitmapImage(new Uri("Assets/skull.png", UriKind.Relative)));
            await Task.Delay(300);
            await TransitionToEndScreen();
            FinalScoreText.Text = $"Score: {_gameState.Score}";
        }

        private void OnGameObjectSpawn(GameObject uObj)
        {
            SpawnModel(uObj);
        }

        private async void OnGameRestarted()
        {
            await TransitionToGameScreen();
            await SetUpGame();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                    _gameState.GetPlayer().MoveLeft = true;
                    break;

                case Key.D:
                    _gameState.GetPlayer().MoveRight = true;
                    break;

                case Key.W:
                    _gameState.GetPlayer().MoveUp = true;
                    break;

                case Key.S:
                    _gameState.GetPlayer().MoveDown = true;
                    break;

                case Key.Space:
                    SpawnBullet();
                    break;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                    _gameState.GetPlayer().MoveLeft = false;
                    break;

                case Key.D:
                    _gameState.GetPlayer().MoveRight = false;
                    break;

                case Key.W:
                    _gameState.GetPlayer().MoveUp = false;
                    break;

                case Key.S:
                    _gameState.GetPlayer().MoveDown = false;
                    break;
            }
        }

        private void RevertPlayerModel()
        {
            _playerModel.GetShape().Fill = new ImageBrush(new BitmapImage(new Uri("Assets/player.png", UriKind.Relative)));
        }

        private async Task SetUpGame()
        {
            ClearGameCanvas();
            SetUpStarfield();
            SetUpPlayerShip();
            UpdateDamage();

            CleanUpGarbageCollector();

            GameCanvas.Focus();
            await GameLoop();
        }

        private void SetUpPlayerShip()
        {
            RevertPlayerModel();
            GameCanvas.Children.Add(_playerModel.GetUIElement());
            Canvas.SetLeft(_playerModel.GetUIElement(), (GameCanvas.Width / 2) - (_playerModel.GetShape().Width / 2));
            Canvas.SetTop(_playerModel.GetUIElement(), GameCanvas.Height / 1.5);
        }

        private void SetUpStarfield()
        {
            ImageBrush background = new ImageBrush(new BitmapImage(new Uri("Assets/StarField.jpg", UriKind.Relative)));
            background1.Fill = background;
            background2.Fill = background;
            Canvas.SetBottom(background1, 0);
            Canvas.SetBottom(background2, 707);
        }

        private void SpawnAsteroidModel()
        {
            AsteroidFieldModel newAsteroid = new(GameCanvas.Width, GameCanvas.Height);
            Canvas.SetTop(newAsteroid.GetUIElement(), rnd.Next(-1000, -580));
            Canvas.SetLeft(newAsteroid.GetUIElement(), rnd.Next(0, (int)GameCanvas.Width));
            Canvas.SetZIndex(newAsteroid.GetUIElement(), Canvas.GetZIndex(GameCanvas) + 1);
            GameCanvas.Children.Add(newAsteroid.GetUIElement());
        }

        private void SpawnBullet()
        {
            BulletModel newBullet = new();
            Canvas.SetLeft(newBullet.GetUIElement(), Canvas.GetLeft(_playerModel.GetUIElement()) + (_playerModel.GetShape().Width / 2));
            Canvas.SetTop(newBullet.GetUIElement(), Canvas.GetTop(_playerModel.GetUIElement()) - newBullet.GetShape().Height);
            GameCanvas.Children.Add(newBullet.GetUIElement());
        }

        private void SpawnBullet(GameObject uObj)
        {
            Rectangle shooter = GameCanvas.Children.OfType<Rectangle>().First(r => r.Uid == uObj.Guid.ToString());
            BulletModel newBullet = new(uObj);
            Canvas.SetLeft(newBullet.GetUIElement(), Canvas.GetLeft(shooter) + (shooter.Width / 2));
            Canvas.SetTop(newBullet.GetUIElement(), Canvas.GetTop(shooter) + shooter.Height);
            GameCanvas.Children.Add(newBullet.GetUIElement());
        }

        private void SpawnModel(GameObject uObj)
        {
            GameModel uRect;
            if (uObj.GetType() == typeof(Enemy))
            {
                uRect = new EnemyModel(uObj);
            }
            else
            {
                uRect = new NanoModel(uObj);
            }

            Canvas.SetTop(uRect.GetUIElement(), -10);
            Canvas.SetLeft(uRect.GetUIElement(), rnd.Next((int)uRect.GetShape().Width / 2, (int)(GameCanvas.Width - (uRect.GetShape().Width + (uRect.GetShape().Width / 2)))));
            GameCanvas.Children.Add(uRect.GetUIElement());
        }

        private async Task TransitionToEndScreen()
        {
            await FadeOut(GameCanvas);
            await FadeIn(GameOverMenu);
        }

        private async Task TransitionToGameScreen()
        {
            GameOverMenu.Visibility = Visibility.Hidden;
            await FadeIn(GameCanvas);
        }

        private void UpdateDamage()
        {
            Damage.Content = $"Damage: {_gameState.GetPlayer().Damage}";

            if (_gameState.GetPlayer().Damage > 50)
            {
                Damage.Foreground = Brushes.Yellow;
            }
            else
            {
                Damage.Foreground = Brushes.White;
            }
        }

        private void UpdateScore()
        {
            ScoreText.Content = $"Score: {_gameState.Score}";
        }
    }
}