using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SpaceShooter.Models
{
    internal abstract class GameModel
    {
        protected readonly ImageBrush sprite;
        protected UIElement uIElement;

        protected GameModel()
        {
            sprite = new ImageBrush();
            uIElement = new UIElement();
        }

        public Shape GetShape()
        {
            return (Shape)uIElement;
        }

        public UIElement GetUIElement()
        {
            return uIElement;
        }
    }
}