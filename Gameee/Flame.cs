using System;
using System.Drawing;

namespace Gameee {

    internal class Flame : IEntity {
        
        private int size;
        private readonly Pen pen;
        private readonly (int _x, int _y, bool h)[] parts;

        public bool Dead => size == 0;

        public Flame(int x, int y, Game game) {
            size = game.Rand.Next(6, 9);
            var g = game.Rand.Next(128);
            pen = new Pen(Color.FromArgb(255, g, game.Rand.Next(g / 2)));
            parts = new (int _x, int _y, bool h)[game.Rand.Next(4, 7)];
            for (var i = 0; i < parts.Length; ++i) {
                var d = game.Rand.Next(5);
                if (d == 0) parts[i] = (x, y, game.Rand.Next(2) == 0);
                else {
                    var α = game.Rand.Next(360) * Math.PI / 180.0;
                    parts[i] = (x + (int)Math.Round(d * Math.Cos(α)), y + (int)Math.Round(d * Math.Sin(α)), game.Rand.Next(2) == 0);
                }
            }
        }

        public void Step(TimeSpan elapsedTime) { if (!Dead) --size; }

        public void Draw(LineRenderer r) {
            var size_d4 = size / 4;
            var size_d2 = size / 2;
            foreach ((int _x, int _y, bool h) in parts) {
                if (h) {
                    r.GoTo(_x - size_d4, _y);
                    r.HLine(pen, size_d2);
                } else {
                    r.GoTo(_x, _y - size_d4);
                    r.VLine(pen, size_d2);
                }
            }
        }
    }
}
