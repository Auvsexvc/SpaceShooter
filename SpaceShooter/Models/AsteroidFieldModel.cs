using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpaceShooter.Models
{
    internal class AsteroidFieldModel : GameModel
    {
        public AsteroidFieldModel(double width, double height)
        {
            sprite.ImageSource = new BitmapImage(new Uri("Assets/asteroids.png", UriKind.Relative));
            uIElement = new Rectangle()
            {
                Tag = "Asteroids",
                Height = height,
                Width = width,
                Fill = sprite,
                Stretch = Stretch.Uniform
            };
        }
    }
}