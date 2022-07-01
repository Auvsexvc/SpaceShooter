using SpaceShooter.Classes;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SpaceShooter.Models
{
    internal class NanoModel : GameModel
    {
        public NanoModel(GameObject uObj)
        {
            uIElement = new Rectangle()
            {
                Tag = $"Nano",
                Uid = uObj.Guid.ToString(),
                Height = 50,
                Width = 56,
                Fill = new ImageBrush(new BitmapImage(new Uri($"Assets/{uObj.GetType().Name.ToLower()}.png", UriKind.Relative))),
            };
        }
    }
}