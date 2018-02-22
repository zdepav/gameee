using System.Drawing;
using System.Windows.Forms;

namespace Gameee {

    internal class Mouse {

        public bool LeftButton { get; private set; }

        public bool MiddleButton { get; private set; }

        public bool RightButton { get; private set; }

        public Point Location { get; private set; }

        public int X => Location.X;

        public int Y => Location.Y;

        public Mouse() {
            LeftButton = false;
            MiddleButton = false;
            RightButton = false;
            Location = Point.Empty;
        }

        public void PressMouseButton(MouseButtons button) {
            switch (button) {
                case MouseButtons.Left: LeftButton = true; break;
                case MouseButtons.Middle: MiddleButton = true; break;
                case MouseButtons.Right: RightButton = true; break;
            }
        }

        public void ReleaseMouseButton(MouseButtons button) {
            switch (button) {
                case MouseButtons.Left: LeftButton = false; break;
                case MouseButtons.Middle: MiddleButton = false; break;
                case MouseButtons.Right: RightButton = false; break;
            }
        }

        public void MoveMouse(Point location) => Location = location;
    }
}
