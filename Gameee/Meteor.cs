using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Gameee {

    internal class Meteor : IEntity {

        private readonly Game game;
        private int x, life;
        private readonly int y, size, radius;
        private readonly Color c1, c2;

        private static readonly LineRenderer.Path
            smallView = LineRenderer.Path.Parse("G16,0;V8;H-8;V8;H-16;V-8;H-8;V-16;H8;V-8;H16;V24;H-16;V-16;H24;V8"),
            mediumView = LineRenderer.Path.Parse("G24,0;V8;H-8;V8;H-8;V8;H-16;V-8;H-8;V-8;H-8;V-16;H8;V-8;H8;V-8;H16;V8;H8;V24;H-8;V8;H-16;V-8;H-8;V-16;H8;V-8;H16;V8;H16;V8"),
            largeView = LineRenderer.Path.Parse("G32,0;V16;H-8;V8;H-8;V8;H-32;V-8;H-8;V-8;H-8;V-32;H8;V-8;H8;V-8;H32;V8;H8;V40;H-8;V8;H-32;V-8;H-8;V-32;H8;V-8;H32;V8;H16;V16");

        public bool Dead => life <= 0;

        public int Size => size;

        public Meteor(Game game) {
            this.game = game;
            size = game.Rand.Next(4);
            if (size == 0) size = 1;
            radius = (size + 1) * 8;
            x = game.Width + radius;
            y = game.Rand.Next(game.Height);
            life = size * 50;
            var b = 192 - game.Rand.Next(64);
            var rg = b / 2 + game.Rand.Next(b / 2);
            c1 = Color.FromArgb(rg, rg, b);
            c2 = Color.FromArgb(rg, rg, rg);
        }

        public bool Hit(IEntity e) {
            if (Dead) return false;
            switch (size) {
                case 1:
                    if (RectWasHit(x - 16, y - 8, x + 16, y + 8, e) ||
                        RectWasHit(x - 8, y - 16, x + 8, y + 16, e)) {
                        Hurt(e);
                        return true;
                    }
                    break;
                case 2:
                    if (RectWasHit(x - 24, y - 8, x + 24, y + 8, e) ||
                        RectWasHit(x - 16, y - 16, x + 16, y + 16, e) ||
                        RectWasHit(x - 8, y - 24, x + 8, y + 24, e)) {
                        Hurt(e);
                        return true;
                    }
                    break;
                case 3:
                    if (RectWasHit(x - 32, y - 16, x + 32, y + 16, e) ||
                        RectWasHit(x - 24, y - 24, x + 24, y + 24, e) ||
                        RectWasHit(x - 16, y - 32, x + 16, y + 32, e)) {
                        Hurt(e);
                        return true;
                    }
                    break;
            }
            return false;
        }

        private void Hurt(IEntity e) {
            if (e is Shot) life -= game.Rand.Next(10, 16);
            else if (e is Ship) life = 0;
            if (Dead) {
                game.CreateSparkles(x + 4, y, 8 + size * 8, size == 3 ? 52 : size * 12);
                if (e is Shot) game.AddScore(100 * size * size);
            }
        }

        public void Explode() {
            if (Dead) return;
            life = 0;
            game.CreateSparkles(x + 4, y, 8 + size * 8, size == 3 ? 52 : size * 12);
            game.AddScore(50 * size * size);
        }

        private bool RectWasHit(int x1, int y1, int x2, int y2, IEntity e) {
            if (e is Shot shot) return shot.Y >= y1 && shot.Y <= y2 && shot.X >= x1 - 6 && shot.X <= x2;
            if (e is Ship ship) return new Rectangle(x1, y1, x2 - x1, y2 - y1).IntersectsWith(ship.Bounds);
            return false;
        }

        public void Step(TimeSpan elapsedTime) {
            if (Dead) return;
            x -= (int)Math.Round(elapsedTime.TotalMilliseconds / (size == 1 ? 5.0 : size == 2 ? 10.0 : 20.0));
            if (x < -radius) life = -1;
        }

        public void Draw(LineRenderer r) {
            if (Dead) return;
            using (var p = new Pen(new LinearGradientBrush(new Point(x - radius, y - radius), new Point(x + radius, y + radius), c1, c2))) {
                switch (size) {
                    case 1: smallView.Draw(r, x, y, p); break;
                    case 2: mediumView.Draw(r, x, y, p); break;
                    case 3: largeView.Draw(r, x, y, p); break;
                }
            }
        }
    }
}
