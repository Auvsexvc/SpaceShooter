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
using System.Windows.Threading;

namespace SpaceShooter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly GameState gameState = new();
        private readonly GameControls gameControls = new();

        private readonly ImageBrush background = new();
        private readonly ImageBrush playerImage = new();
        private readonly ImageBrush nanoSprite = new();
        private readonly ImageBrush boomSprite = new();
        private readonly ImageBrush shieldSprite = new();
        private readonly DispatcherTimer timer = new();

        private readonly List<Rectangle> garbageCollector = new();
        private readonly Random rnd = new();

        private Rect playerHitBox;

        private readonly DoubleAnimation fadeOutAnimation = new()
        {
            Duration = TimeSpan.FromMilliseconds(2000),
            From = 1,
            To = 0.1
        };

        private readonly DoubleAnimation fadeInAnimation = new()
        {
            Duration = TimeSpan.FromMilliseconds(600),
            From = 0,
            To = 1
        };

        public MainWindow()
        {
            InitializeComponent();

            InitializeGameTimer();

            SetUpGame();

            gameState.TriggerEnemySpawn += OnTriggerEnemySpawn;
            gameState.TriggerNanoSpawn += OnTriggerNanoSpawn;
            gameState.GameEnded += OnGameEnded;
            gameState.GameRestarted += OnGameRestarted;
        }

        private void InitializeGameTimer()
        {
            timer.Interval = TimeSpan.FromMilliseconds(20);
            timer.Tick += GameLoop;
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            DrawParallaxStarField();

            gameState.CountDownToEnemySpawn();

            gameState.CountDownToNanoSpawn();

            MovePlayer();

            UpdatePlayerModel();

            MoveBullets();

            MoveEnemies();

            MoveNanos();

            _ = GetBulletCollision();

            GetPlayerCollision();

            UpdateDamage();

            UpdateScore();

            CleanUpGarbageCollector();

            gameState.MakeGameHarder();

            gameState.PlayerDestroyed();
        }

        private void SetUpGame()
        {
            ClearGameCanvas();
            SetUpSprites();
            SetUpStarfield();
            SetUpPlayerShip();
            UpdateDamage();

            CleanUpGarbageCollector();

            GameCanvas.Focus();
            timer.Start();
        }

        private void ClearGameCanvas()
        {
            foreach (Rectangle item in GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Bullet" || (string)rect.Tag == "Enemy" || (string)rect.Tag == "Nano"))
            {
                GameCanvas.Children.Remove(item);
            }
        }

        private void SetUpSprites()
        {
            background.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/StarField.jpg"));
            playerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/player.png"));
            boomSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/boom.png"));
            shieldSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/playerShield.png"));
            nanoSprite.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Assets/NanoBots.png"));
        }

        private void SetUpStarfield()
        {
            background1.Fill = background;
            background2.Fill = background;
            Canvas.SetBottom(background1, 0);
            Canvas.SetBottom(background2, 707);
        }

        private void SetUpPlayerShip()
        {
            Player.Fill = playerImage;
            Canvas.SetLeft(Player, 246);
            Canvas.SetTop(Player, 518);
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

        private void SpawnEnemy()
        {
            Rectangle newEnemy = new()
            {
                Tag = "Enemy",
                Height = 50,
                Width = 56,
                Fill = new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri($"pack://application:,,,/Assets/{rnd.Next(1, 10)}.png"))
                }
            };

            Canvas.SetTop(newEnemy, -10);
            Canvas.SetLeft(newEnemy, rnd.Next(30, 430));
            GameCanvas.Children.Add(newEnemy);
        }

        private void SpawnNano()
        {
            Rectangle newNano = new()
            {
                Tag = "Nano",
                Height = 50,
                Width = 56,
                Fill = nanoSprite
            };
            Canvas.SetTop(newNano, -1000);
            Canvas.SetLeft(newNano, rnd.Next(30, 430));
            GameCanvas.Children.Add(newNano);
        }

        private void SpawnBullet()
        {
            Rectangle newBullet = new()
            {
                Tag = "Bullet",
                Height = 20,
                Width = 5,
                Fill = Brushes.White,
                Stroke = Brushes.Red,
            };
            Canvas.SetLeft(newBullet, Canvas.GetLeft(Player) + (Player.Width / 2));
            Canvas.SetTop(newBullet, Canvas.GetTop(Player) - newBullet.Height);
            GameCanvas.Children.Add(newBullet);
        }

        private void UpdatePlayerModel()
        {
            playerHitBox = new Rect(Canvas.GetLeft(Player), Canvas.GetTop(Player), Player.Width, Player.Height);
            Player.Fill = playerImage;
        }

        private void MovePlayer()
        {
            if (gameControls.MoveLeft && Canvas.GetLeft(Player) > Player.Width / 2)
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) - GameState.PlayerSpeed);
            }
            if (gameControls.MoveRight && Canvas.GetLeft(Player) + Player.Width < GameCanvas.Width - (Player.Width / 2))
            {
                Canvas.SetLeft(Player, Canvas.GetLeft(Player) + GameState.PlayerSpeed);
            }
            if (gameControls.MoveUp && Canvas.GetTop(Player) > Player.Height * 4)
            {
                Canvas.SetTop(Player, Canvas.GetTop(Player) - (GameState.PlayerSpeed / 2));
            }
            if (gameControls.MoveDown && Canvas.GetTop(Player) + Player.Height < GameCanvas.Height - (Player.Height / 2))
            {
                Canvas.SetTop(Player, Canvas.GetTop(Player) + (GameState.PlayerSpeed / 2));
            }
        }

        private void UpdateDamage()
        {
            Damage.Content = $"Damage: {gameState.Damage}";

            if (gameState.Damage > 50)
            {
                Damage.Foreground = Brushes.Yellow;
            }
            if (gameState.Damage < 50)
            {
                Damage.Foreground = Brushes.White;
            }
        }

        private void UpdateScore()
        {
            ScoreText.Content = $"Score: {gameState.Score}";
        }

        private void MoveBullets()
        {
            foreach (Rectangle bullet in GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Bullet"))
            {
                Canvas.SetTop(bullet, Canvas.GetTop(bullet) - GameState.BulletSpeed);
            }
        }

        private void MoveEnemies()
        {
            foreach (Rectangle enemy in GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Enemy"))
            {
                Canvas.SetTop(enemy, Canvas.GetTop(enemy) + gameState.EnemySpeed);
            }
        }

        private void MoveNanos()
        {
            foreach (Rectangle nano in GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Nano"))
            {
                Canvas.SetTop(nano, Canvas.GetTop(nano) + gameState.NanoSpeed);
            }
        }

        private async Task GetBulletCollision()
        {
            foreach (Rectangle bullet in GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Bullet"))
            {
                Rect bulletHitBox = new(Canvas.GetLeft(bullet), Canvas.GetTop(bullet), bullet.Width, bullet.Height);

                if (Canvas.GetTop(bullet) < 60)
                {
                    garbageCollector.Add(bullet);
                }

                foreach (Rectangle nano in GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Nano"))
                {
                    Rect nanoHit = new(Canvas.GetLeft(nano), Canvas.GetTop(nano), nano.Width, nano.Height);

                    if (bulletHitBox.IntersectsWith(nanoHit))
                    {
                        nano.Fill = boomSprite;
                        garbageCollector.Add(bullet);
                        await Task.Delay(100);
                        garbageCollector.Add(nano);
                    }
                }
                foreach (Rectangle enemy in GameCanvas.Children.OfType<Rectangle>().Where(rect => (string)rect.Tag == "Enemy"))
                {
                    Rect enemyHit = new(Canvas.GetLeft(enemy), Canvas.GetTop(enemy), enemy.Width, enemy.Height);

                    if (bulletHitBox.IntersectsWith(enemyHit))
                    {
                        enemy.Fill = boomSprite;
                        garbageCollector.Add(bullet);
                        gameState.Score++;
                        await Task.Delay(100);
                        garbageCollector.Add(enemy);
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

                    if (Canvas.GetTop(item) > 650)
                    {
                        garbageCollector.Add(item);
                        gameState.Damage += 10;
                    }

                    if (playerHitBox.IntersectsWith(enemyHitBox))
                    {
                        Player.Fill = shieldSprite;
                        gameState.Damage++;
                    }
                }

                if ((string)item.Tag == "Nano")
                {
                    Rect nanoHitBox = new(Canvas.GetLeft(item), Canvas.GetTop(item), item.Width, item.Height);

                    if (Canvas.GetTop(item) > 650)
                    {
                        garbageCollector.Add(item);
                    }

                    if (playerHitBox.IntersectsWith(nanoHitBox) && gameState.Damage > 0)
                    {
                        garbageCollector.Add(item);
                        if (gameState.Damage >= 15)
                        {
                            gameState.Damage -= 15;
                        }
                        else
                        {
                            gameState.Damage = 0;
                        }
                    }
                }
            }
        }

        private void CleanUpGarbageCollector()
        {
            foreach (Rectangle item in garbageCollector)
            {
                GameCanvas.Children.Remove(item);
            }
        }

        private async void OnGameRestarted()
        {
            await TransitionToGameScreen();
            SetUpGame();
        }

        private void OnTriggerEnemySpawn()
        {
            SpawnEnemy();
        }

        private void OnTriggerNanoSpawn()
        {
            SpawnNano();
        }

        private async void OnGameEnded()
        {
            Player.Fill = boomSprite;
            timer.Stop();
            Damage.Content = $"Damage: {gameState.Damage}";
            Damage.Foreground = Brushes.Red;
            await Task.Delay(100);
            Player.Fill = null;
            await Task.Delay(300);
            await TransitionToEndScreen();
            FinalScoreText.Text = $"Score: {gameState.Score}";
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    gameControls.MoveLeft = true;
                    break;

                case Key.Right:
                    gameControls.MoveRight = true;
                    break;

                case Key.Up:
                    gameControls.MoveUp = true;
                    break;

                case Key.Down:
                    gameControls.MoveDown = true;
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
                case Key.Left:
                    gameControls.MoveLeft = false;
                    break;

                case Key.Right:
                    gameControls.MoveRight = false;
                    break;

                case Key.Up:
                    gameControls.MoveUp = false;
                    break;

                case Key.Down:
                    gameControls.MoveDown = false;
                    break;
            }
        }

        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {
            gameState.Reset();
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async Task TransitionToGameScreen()
        {
            GameOverMenu.Visibility = Visibility.Hidden;
            await FadeIn(GameCanvas);
        }

        private async Task TransitionToEndScreen()
        {
            await FadeOut(GameCanvas);
            await FadeIn(GameOverMenu);
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
    }
}