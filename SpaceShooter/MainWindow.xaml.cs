using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const int maxDelay = 17;
        private const int minDelay = 17;

        private const int SpawnMaxCanvasWidth = 430;
        private const int SpawnMinCanvasWidth = 30;
        private readonly ImageBrush _asteroidsSprite = new();
        private readonly ImageBrush _background = new();
        private readonly ImageBrush _boomSprite = new();
        private readonly GameControls _gameControls = new();
        private readonly GameState _gameState = new();
        private readonly List<Rectangle> _garbageCollector = new();
        private readonly ImageBrush _nanoSprite = new();
        private readonly ImageBrush _playerImage = new();
        private readonly ImageBrush _shieldSprite = new();
        private readonly ImageBrush _skullSprite = new();

        private readonly DoubleAnimation fadeInAnimation = new()
        {
            Duration = TimeSpan.FromMilliseconds(600),
            From = 0,
            To = 1
        };

        private readonly DoubleAnimation fadeOutAnimation = new()
        {
            Duration = TimeSpan.FromMilliseconds(2000),
            From = 1,
            To = 0.1
        };

        private readonly Random rnd = new();
        private readonly Stopwatch stw = new();
        private Rect playerHitBox;

        public MainWindow()
        {
            InitializeComponent();
            SetUpGame();

            _gameState.TriggerSpawnModel += OnUfoSpawn;
            _gameState.TriggerSpawnAsteroidModel += OnAsteroidSpawn;
            _gameState.GameEnded += OnGameEnded;
            _gameState.GameRestarted += OnGameRestarted;
        }

        private void CleanUpGarbageCollector()
        {
            foreach (Rectangle item in _garbageCollector)
            {
                _gameState.RemoveUfoByUid(item.Uid);
                GameCanvas.Children.Remove(item);
            }
        }

        private void ClearGameCanvas()
        {
            foreach (Rectangle item in GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Bullet" || (string)rect.Tag == "Enemy" || (string)rect.Tag == "Nano").ToList())
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
            var uObj = _gameState.Ufos.Find(u => u.Guid.ToString() == uRect.Uid);
            uObj!.TakesEvasiveManeuver = false;

            if (Canvas.GetTop(uRect) > Canvas.GetTop(bullet) - 450 && Canvas.GetLeft(uRect) - 30 <= Canvas.GetLeft(bullet) && Canvas.GetLeft(uRect) + uRect.Width + 30 >= Canvas.GetLeft(bullet) + bullet.Width)
            {
                if (uObj!.EvadesLeft)
                {
                    if (Canvas.GetLeft(uRect) - uObj!.Speed > SpawnMinCanvasWidth)
                    {
                        uObj.TakesEvasiveManeuver = true;
                        Canvas.SetLeft(uRect, Canvas.GetLeft(uRect) - (uObj!.Speed - 2));
                    }
                }
                else
                {
                    if (Canvas.GetLeft(uRect) - uObj!.Speed < SpawnMaxCanvasWidth)
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

        private void EnemyShoots(UnindentifiedFlyingObject uObj)
        {
            stw.Start();
            if (stw.ElapsedMilliseconds % 20 == 0)
                SpawnBullet(uObj);
        }

        private async Task FadeIn(UIElement e)
        {
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
                int delay = Math.Max(minDelay, maxDelay);
                await Task.Delay(delay);

                DrawParallaxStarField();

                _gameState.CountDownToEnemySpawn();

                _gameState.CountDownToNanoSpawn();
                _gameState.CountDownToAsteroidSpawn();

                MoveAsteroids();

                MovePlayer();

                UpdatePlayerModel();

                _ = GetBulletCollision();

                MoveBullets();

                MoveUfos();

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
                if ((string)item.Tag == "Enemy")
                {
                    Rect enemyHitBox = new(Canvas.GetLeft(item), Canvas.GetTop(item), item.Width, item.Height);

                    if (playerHitBox.IntersectsWith(enemyHitBox))
                    {
                        Player.Fill = _shieldSprite;
                        _gameState.Damage++;
                    }
                    else if (Canvas.GetTop(item) > 650)
                    {
                        _garbageCollector.Add(item);
                        _gameState.Damage += 10;
                    }
                }

                if ((string)item.Tag == "EnemyBullet")
                {
                    Rect enemyHitBox = new(Canvas.GetLeft(item), Canvas.GetTop(item), item.Width, item.Height);

                    if (playerHitBox.IntersectsWith(enemyHitBox))
                    {
                        Player.Fill = _shieldSprite;
                        _gameState.Damage += 10;
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

                    if (playerHitBox.IntersectsWith(nanoHitBox) && _gameState.Damage > 0)
                    {
                        _garbageCollector.Add(item);
                        if (_gameState.Damage >= 15)
                        {
                            _gameState.Damage -= 15;
                        }
                        else
                        {
                            _gameState.Damage = 0;
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
                    Canvas.SetTop(bullet, Canvas.GetTop(bullet) + GameState.EnemyBulletSpeed);
                }
                else
                {
                    Canvas.SetTop(bullet, Canvas.GetTop(bullet) - GameState.BulletSpeed);
                }
            }
        }

        private void MovePlayer()
        {
            if (_gameControls.MoveLeft && Canvas.GetLeft(Player) > Player.Width / 2)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) - GameState.PlayerSpeed);
            }
            if (_gameControls.MoveRight && Canvas.GetLeft(Player) + Player.Width < GameCanvas.Width - (Player.Width / 2))
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) + GameState.PlayerSpeed);
            }
            if (_gameControls.MoveUp && Canvas.GetTop(Player) > Player.Height * 4)
            {
                Canvas.SetTop(Player, Canvas.GetTop(Player) - (GameState.PlayerSpeed / 2));
            }
            if (_gameControls.MoveDown && Canvas.GetTop(Player) + Player.Height < GameCanvas.Height - (Player.Height / 2))
            {
                Canvas.SetTop(Player, Canvas.GetTop(Player) + (GameState.PlayerSpeed / 2));
            }
        }

        private void MoveUfos()
        {
            foreach (UnindentifiedFlyingObject enemyObj in _gameState.Ufos)
            {
                Rectangle enemyRect = GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == $"{enemyObj.GetType().Name}").FirstOrDefault(rect => enemyObj.Guid.ToString() == rect.Uid)!;
                if (enemyRect is not null)
                {
                    Canvas.SetTop(enemyRect, Canvas.GetTop(enemyRect) + enemyObj!.Speed);
                    if (enemyObj.GetType() == typeof(Enemy))
                    {
                        UfoIsTrackingPlayer(enemyObj, enemyRect);
                        UfoIsAShooter(enemyObj, enemyRect);
                    }
                }
            }
        }

        private void OnAsteroidSpawn()
        {
            SpawnAsteroidModel();
        }

        private async void OnGameEnded()
        {
            Player.Fill = _boomSprite;
            Damage.Content = $"Damage: {_gameState.Damage}";
            Damage.Foreground = Brushes.Red;
            await Task.Delay(200);
            Player.Fill = _skullSprite;
            await Task.Delay(300);
            await TransitionToEndScreen();
            FinalScoreText.Text = $"Score: {_gameState.Score}";
        }

        private async void OnGameRestarted()
        {
            await TransitionToGameScreen();
            SetUpGame();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.A:
                    _gameControls.MoveLeft = true;
                    break;

                case Key.D:
                    _gameControls.MoveRight = true;
                    break;

                case Key.W:
                    _gameControls.MoveUp = true;
                    break;

                case Key.S:
                    _gameControls.MoveDown = true;
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
                    _gameControls.MoveLeft = false;
                    break;

                case Key.D:
                    _gameControls.MoveRight = false;
                    break;

                case Key.W:
                    _gameControls.MoveUp = false;
                    break;

                case Key.S:
                    _gameControls.MoveDown = false;
                    break;
            }
        }

        private void OnUfoSpawn(UnindentifiedFlyingObject uObj)
        {
            SpawnModel(uObj);
        }

        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            _gameState.Reset();
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void SetUpGame()
        {
            ClearGameCanvas();
            SetUpSprites();
            SetUpStarfield();
            SetUpPlayerShip();
            UpdateDamage();

            CleanUpGarbageCollector();

            GameCanvas.Focus();
            await GameLoop();
        }

        private void SetUpPlayerShip()
        {
            Player.Fill = _playerImage;
            Canvas.SetLeft(Player, 246);
            Canvas.SetTop(Player, 518);
        }

        private void SetUpSprites()
        {
            _background.ImageSource = new BitmapImage(new Uri("Assets/StarField.jpg", UriKind.Relative));
            _playerImage.ImageSource = new BitmapImage(new Uri("Assets/player.png", UriKind.Relative));
            _boomSprite.ImageSource = new BitmapImage(new Uri("Assets/boom.png", UriKind.Relative));
            _shieldSprite.ImageSource = new BitmapImage(new Uri("Assets/playerShield.png", UriKind.Relative));
            _skullSprite.ImageSource = new BitmapImage(new Uri("Assets/skull.png", UriKind.Relative));
            _nanoSprite.ImageSource = new BitmapImage(new Uri("Assets/nano.png", UriKind.Relative));
            _asteroidsSprite.ImageSource = new BitmapImage(new Uri("Assets/asteroids.png", UriKind.Relative));
        }

        private void SetUpStarfield()
        {
            background1.Fill = _background;
            background2.Fill = _background;
            Canvas.SetBottom(background1, 0);
            Canvas.SetBottom(background2, 707);
        }

        private void SpawnAsteroidModel()
        {
            Rectangle newAsteroid = new()
            {
                Tag = "Asteroids",
                Height = GameCanvas.Height,
                Width = GameCanvas.Width,
                Fill = _asteroidsSprite,
                Stretch = Stretch.Uniform
            };
            Canvas.SetTop(newAsteroid, rnd.Next(-1000, -580));
            Canvas.SetLeft(newAsteroid, rnd.Next(0, (int)GameCanvas.Width));
            Canvas.SetZIndex(newAsteroid, Canvas.GetZIndex(GameCanvas) + 1);
            GameCanvas.Children.Add(newAsteroid);
        }

        private void SpawnBullet()
        {
            Rectangle newBullet = new()
            {
                Tag = "Bullet",
                Height = 20,
                Width = 5,
                Fill = Brushes.White,
                Stroke = Brushes.Green,
            };
            Canvas.SetLeft(newBullet, Canvas.GetLeft(Player) + (Player.Width / 2));
            Canvas.SetTop(newBullet, Canvas.GetTop(Player) - newBullet.Height);
            GameCanvas.Children.Add(newBullet);
        }

        private void SpawnBullet(UnindentifiedFlyingObject uObj)
        {
            Rectangle shooter = GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Enemy").First(r => r.Uid == uObj.Guid.ToString());
            Rectangle newBullet = new()
            {
                Tag = $"{uObj.GetType().Name}Bullet",
                Height = 20,
                Width = 5,
                Fill = Brushes.LightGoldenrodYellow,
                Stroke = Brushes.Red,
            };
            Canvas.SetLeft(newBullet, Canvas.GetLeft(shooter) + (shooter.Width / 2));
            Canvas.SetTop(newBullet, Canvas.GetTop(shooter) + shooter.Height);
            GameCanvas.Children.Add(newBullet);
        }

        private void SpawnModel(UnindentifiedFlyingObject uObj)
        {
            ImageBrush fill;
            if (uObj.GetType() == typeof(Enemy))
            {
                fill = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri($"Assets/{rnd.Next(1, 10)}.png", UriKind.Relative))
                };
            }
            else
            {
                fill = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri($"Assets/{uObj.GetType().Name.ToLower()}.png", UriKind.Relative))
                };
            }
            Rectangle uRect = new()
            {
                Tag = $"{uObj.GetType().Name}",
                Uid = uObj.Guid.ToString(),
                Height = 50,
                Width = 56,
                Fill = fill
            };
            Canvas.SetTop(uRect, -10);
            Canvas.SetLeft(uRect, rnd.Next(30, 430));
            GameCanvas.Children.Add(uRect);
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

        private void UfoIsAShooter(UnindentifiedFlyingObject uObj, Rectangle uRect)
        {
            if (Canvas.GetTop(uRect) > Canvas.GetTop(Player) - GameCanvas.Height && Canvas.GetTop(uRect) < Canvas.GetTop(Player) && uObj.Shooting && !uObj.TakesEvasiveManeuver)
            {
                if (Canvas.GetLeft(uRect) < Canvas.GetLeft(Player))
                {
                    Canvas.SetLeft(uRect, Canvas.GetLeft(uRect) + uObj!.Speed);
                }
                else if (Canvas.GetLeft(uRect) > Canvas.GetLeft(Player) + (Player.Width / 4))
                {
                    Canvas.SetLeft(uRect, Canvas.GetLeft(uRect) - uObj!.Speed);
                }
                else
                {
                    EnemyShoots(uObj);
                }
            }
        }

        private void UfoIsTrackingPlayer(UnindentifiedFlyingObject uObj, Rectangle uRect)
        {
            if (Canvas.GetTop(uRect) > Canvas.GetTop(Player) - 150 && Canvas.GetTop(uRect) < Canvas.GetTop(Player) && uObj.Tracking && !uObj.Shooting && !uObj.TakesEvasiveManeuver)
            {
                if (Canvas.GetLeft(uRect) < Canvas.GetLeft(Player))
                {
                    Canvas.SetLeft(uRect, Canvas.GetLeft(uRect) + uObj!.Speed);
                }
                else if (Canvas.GetLeft(uRect) > Canvas.GetLeft(Player) + (Player.Width / 4))
                {
                    Canvas.SetLeft(uRect, Canvas.GetLeft(uRect) - uObj!.Speed);
                }
                else
                {
                    if (uObj.Shooting)
                    {
                        EnemyShoots(uObj);
                    }
                }
            }
        }

        private void UpdateDamage()
        {
            Damage.Content = $"Damage: {_gameState.Damage}";

            if (_gameState.Damage > 50)
            {
                Damage.Foreground = Brushes.Yellow;
            }
            else
            {
                Damage.Foreground = Brushes.White;
            }
        }

        private void UpdatePlayerModel()
        {
            playerHitBox = new Rect(Canvas.GetLeft(Player), Canvas.GetTop(Player), Player.Width, Player.Height);
            Player.Fill = _playerImage;
        }

        private void UpdateScore()
        {
            ScoreText.Content = $"Score: {_gameState.Score}";
        }
    }
}