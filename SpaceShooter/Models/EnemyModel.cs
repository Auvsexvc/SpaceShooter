using SpaceShooter.Classes;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpaceShooter.Models
{
    internal class EnemyModel : GameModel
    {
        public EnemyModel(GameObject uObj)
        {
            uIElement = new Rectangle()
            {
                Tag = $"{uObj.GetType().Name}",
                Uid = uObj.Guid.ToString(),
                Height = 50,
                Width = 56,
                Fill = new ImageBrush(new BitmapImage(new Uri($"Assets/{rnd.Next(1, 10)}.png", UriKind.Relative))),
            };
        }
    }
}