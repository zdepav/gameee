using System;
using System.Drawing;

namespace Gameee {

    internal class Sparkle : IEntity {

        private readonly int x, y;
        private readonly Game game;
        private int size;
        private readonly Pen pen;

        public bool Dead => size == 0;

        public Sparkle(int x, int y, Game game) {
            this.x = x;
            this.y = y;
            this.game = game;
            size = game.Rand.Next(4, 7);
            pen = new Pen(Color.FromArgb(255, 255, 128 + game.Rand.Next(128)));
        }

        public void Step(TimeSpan elapsedTime) { if (!Dead) --size; }

        public void Draw(LineRenderer r) {
            if (Dead) return;
            r.GoTo(x, y - size);
            r.VLine(pen, size * 2 + 1);
            r.GoTo(x - size, y);
            r.HLine(pen, size * 2 + 1);
            if (size > 3) {
                r.GoTo(x + 3, y - 1);
                r.VLine(pen, 2);
                r.GoTo(x - 3, y - 1);
                r.VLine(pen, 2);
                r.GoTo(x - 1, y + 3);
                r.HLine(pen, 2);
                r.GoTo(x - 1, y - 3);
                r.HLine(pen, 2);
            }
        }
    }
}
