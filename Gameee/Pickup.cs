using System;
using System.Drawing;

namespace Gameee {

    internal class Pickup : IEntity {

        public enum Type { Health = 0, Upgrade = 1, Bomb = 2 }

        private int x, blinkTimer;
        private readonly int y;

        private static readonly LineRenderer.Path
            crossView = LineRenderer.Path.Parse("G8,0;V4;H-4;V4;H-8;V-4;H-4;V-8;H4;V-4;H8;V4;H4;V4;G6,0;V2;H-4;V4;H-4;V-4;H-4;V-4;H4;V-4;H4;V4;H4;V2"),
            uView = LineRenderer.Path.Parse("G0,8;H-8;V-16;H6;V10;H4;V-10;H6;V16;H-8;G5,-5;V10;H-10;V-10"),
            bombView = LineRenderer.Path.Parse("G0,-8;V16;G-3,-8;V2;H6;V-2;G-3,-3;H6;V11;H-6;V-11");

        public Type PickupType { get; }

        public bool Dead { get; private set; }

        public Pickup(Game game) {
            PickupType = (Type)game.Rand.Next(3);
            x = game.Width + 16;
            y = game.Rand.Next(game.Height);
            Dead = false;
            blinkTimer = 0;
        }

        public void Step(TimeSpan elapsedTime) {
            if (Dead) return;
            x -= (int)Math.Round(elapsedTime.TotalMilliseconds / 4.0);
            blinkTimer = (blinkTimer + (int)Math.Round(elapsedTime.TotalMilliseconds)) % 360;
            if (x < -16) Dead = true;
        }

        public void Draw(LineRenderer r) {
            if (Dead) return;
            var c = (Math.Sin(blinkTimer * Math.PI / 180.0) + 1.0) / 2.0;
            switch (PickupType) {
                case Type.Health:
                    var gb = (int)Math.Round(c * 128);
                    Draw(r, crossView, 255, gb, gb);
                    break;
                case Type.Upgrade:
                    Draw(r, uView, (int)Math.Round(c * 64 + 128), (int)Math.Round(c * 96 + 64), (int)Math.Round(c * 128));
                    break;
                case Type.Bomb:
                    Draw(r, bombView, (int)Math.Round(c * 128), (int)Math.Round(c * 96 + 128), 255);
                    break;
            }
        }

        private void Draw(LineRenderer r, LineRenderer.Path view, int R, int G, int B) {
            using (var p = new Pen(Color.FromArgb(R, G, B))) view.Draw(r, x, y, p);
        }

        public bool Picked(Ship ship) {
            if (Dead) return false;
            if (new Rectangle(x - 8, y - 8, 17, 17).IntersectsWith(ship.Bounds)) {
                Dead = true;
                return true;
            }
            return false;
        }
    }
}
