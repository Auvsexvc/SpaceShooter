using SpaceShooter.Classes;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SpaceShooter.Models
{
    internal class BulletModel : GameModel
    {
        public BulletModel()
        {
            uIElement = new Rectangle()
            {
                Tag = "Bullet",
                Height = 20,
                Width = 5,
                Fill = Brushes.White,
                Stroke = Brushes.Green,
            };
        }

        public BulletModel(UnindentifiedFlyingObject uObj)
        {
            uIElement = new Rectangle()
            {
                Tag = $"{uObj.GetType().Name}Bullet",
                Height = 20,
                Width = 5,
                Fill = Brushes.LightGoldenrodYellow,
                Stroke = Brushes.Red,
            };
        }
    }
}