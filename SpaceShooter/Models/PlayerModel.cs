using SpaceShooter.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SpaceShooter.Models
{
    internal class PlayerModel : GameModel
    {
        public PlayerModel(GameObject uObj)
        {
            uIElement = new Rectangle()
            {
                Name = "Player",
                Tag = "Player",
                Uid = uObj.Guid.ToString(),
                Height = 50,
                Width = 60,
                Fill = new ImageBrush(new BitmapImage(new Uri("Assets/player.png", UriKind.Relative))),
            };
        }
    }
}
