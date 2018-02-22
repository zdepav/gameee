using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Gameee {

    internal class Ship : IEntity {

        private readonly Game game;
        private int x, y, xSpeed, ySpeed, shotCooldown, health, flameTimer, upgradedTimer;
        private readonly Color c1, c2, uc1, uc2;
        private bool leftGunIsNext;

        private static readonly LineRenderer.Path
            upgradedView = LineRenderer.Path.Parse("G16,0;V4;H-4;V2;H4;H-4;V2;H4;H-4;V2;H4;H-8;V3;H-10;V3;H-11;V-6;H-3;V-20;H3;V-6;H11;V3;H10;V3;H8;H-4;V2;H4;H-4;V2;H4;H-4;V2;H4;V4"),
            normalView = LineRenderer.Path.Parse("G16,0;V4;H-4;V4;H4;H-4;V2;H-4;V3;H-10;V3;H-11;V-6;H-3;V-20;H3;V-6;H11;V3;H10;V3;H4;V2;H4;H-4;V4;H4;V4");

        public bool Dead => health <= 0;

        public Rectangle Bounds => new Rectangle(x - 16, y - 16, 32, 32);

        public Ship(int x, int y, Game game) {
            this.x = x;
            this.y = y;
            this.game = game;
            c1 = Color.Coral;
            c2 = Color.Gold;
            uc1 = Color.Olive;
            uc2 = Color.GreenYellow;
            xSpeed = ySpeed = shotCooldown = flameTimer = 0;
            leftGunIsNext = true;
            health = 1000;
        }

        public void Step(TimeSpan elapsedTime) {
            if (Dead) return;
            bool a = game.Keyboard[Keys.A], d = game.Keyboard[Keys.D], w = game.Keyboard[Keys.W], s = game.Keyboard[Keys.S];
            if (!a || !d) {
                if (a) xSpeed = Math.Max(xSpeed - 1, -5);
                else if (d) xSpeed = Math.Min(xSpeed + 1, 5);
                else xSpeed = xSpeed < 0 ? xSpeed + 1 : xSpeed > 0 ? xSpeed - 1 : xSpeed;
            }
            if (!w || !s) {
                if (w) ySpeed = Math.Max(ySpeed - 1, -5);
                else if (s) ySpeed = Math.Min(ySpeed + 1, 5);
                else ySpeed = ySpeed < 0 ? ySpeed + 1 : ySpeed > 0 ? ySpeed - 1 : ySpeed;
            }

            x += (int)Math.Round(elapsedTime.TotalMilliseconds * xSpeed / 20.0);
            if (x < 16) x = 16; else if (x > game.Width - 17) x = game.Width - 17;

            y += (int)Math.Round(elapsedTime.TotalMilliseconds * ySpeed / 20.0);
            if (y < 16) y = 16; else if (y > game.Height - 17) y = game.Height - 17;

            if (shotCooldown > 0) shotCooldown = Math.Max(0, shotCooldown - (int)Math.Round(elapsedTime.TotalMilliseconds));
            if (shotCooldown == 0) {
                // if (game.Keyboard[Keys.Space]) {
                if (upgradedTimer > 0) {
                    game.CreateShot(x + 16, y - 10);
                    game.CreateShot(x + 16, y - 8);
                    game.CreateShot(x + 16, y - 6);
                    game.CreateShot(x + 16, y + 6);
                    game.CreateShot(x + 16, y + 8);
                    game.CreateShot(x + 16, y + 10);
                } else {
                    game.CreateShot(x + 16, leftGunIsNext ? y - 8 : y + 8);
                    leftGunIsNext = !leftGunIsNext;
                }
                shotCooldown = 80;
                // }
            }

            foreach (var entity in game.Entities) {
                if (entity is Meteor meteor) {
                    if (meteor.Hit(this))
                        health -= meteor.Size * game.Rand.Next(25, 51);
                } else if (entity is Pickup pickup) {
                    if (pickup.Picked(this)) {
                        switch (pickup.PickupType) {
                            case Pickup.Type.Health: health = Math.Min(health + 250, 1000); break;
                            case Pickup.Type.Upgrade: upgradedTimer = 2500; break;
                            case Pickup.Type.Bomb: game.Explode(); break;
                        }
                    }
                }
            }
            if (Dead) {
                game.CreateFlames(x, y, 24, 20);
                game.End();
                return;
            }

            if (upgradedTimer > 0) upgradedTimer = Math.Max(0, upgradedTimer - (int)Math.Round(elapsedTime.TotalMilliseconds));

            if (health > 950) return;
            flameTimer += (int)Math.Round((10 - health / 100) * elapsedTime.TotalMilliseconds);
            if (flameTimer > 1000) {
                game.CreateFlames(x, y, 16, health < 500 ? 2 : 1);
                flameTimer -= 1000;
            }
        }

        public void Draw(LineRenderer r) {
            if (Dead) return;
            if (upgradedTimer > 0) {
                using (var p = new Pen(new LinearGradientBrush(new Point(x - 17, y), new Point(x + 17, y), uc1, uc2)))
                    upgradedView.Draw(r, x, y, p);
            } else {
                using (var p = new Pen(new LinearGradientBrush(new Point(x - 17, y), new Point(x + 17, y), c1, c2)))
                    normalView.Draw(r, x, y, p);
            }
        }
    }
}

