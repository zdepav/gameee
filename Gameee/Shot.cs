using System;
using System.Drawing;
using System.Linq;

namespace Gameee {

    internal class Shot : IEntity {

        private readonly Game game;
        private int x;
        private readonly int y;
        private readonly Pen pen;

        public int X => x;
        public int Y => y;

        public bool Dead { get; private set; }

        public Shot(int x, int y, Game game) {
            this.x = x;
            this.y = y;
            this.game = game;
            pen = new Pen(Color.FromArgb(255, 128 + game.Rand.Next(128), 0));
        }

        ~Shot() => pen.Dispose();

        public void Step(TimeSpan elapsedTime) {
            if (Dead) return;
            x += (int)Math.Round(elapsedTime.TotalMilliseconds / 2.0);
            if (x >= game.Width) Dead = true;
            else {
                foreach (var meteor in game.Entities.Where(e => e is Meteor).Cast<Meteor>()) {
                    if (meteor.Hit(this)) {
                        Dead = true;
                        game.CreateSparkles(x + 4, y, 4, game.Rand.Next(3) + 1);
                    }
                }
            }
        }

        public void Draw(LineRenderer r) {
            if (Dead) return;
            r.GoTo(x, y);
            r.HLine(pen, 7);
        }
    }
}
