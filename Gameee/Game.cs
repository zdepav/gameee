using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using Gameee.Properties;

namespace Gameee {

    internal class Game {

        private readonly Color background;
        private Ship ship;
        private readonly (int x, int y)[] stars;
        private readonly Pen starPen;
        private readonly List<IEntity> entities, newEntities;
        private int meteorTimer, pickupTimer, minMeteorInterval, blinkTimer, score, scoreTimer;
        private readonly Queue<double> intervals;
        private bool paused, shouldExplode, showingInfo, showingFps;
        private readonly SoundPlayer soundPlayer;
        private (Point tl, Point br) musicLinkLocation;

        private static readonly LineRenderer.Path
            logoOuterView = LineRenderer.Path.Parse(
                "G3,3;H20;V8;H-2;V-6;H-16;V40;H16;V-19;H-6;V-2;H8;V23;H-20;V-44;" +   // G
                "G30,3;H20;V44;H-2;V-21;H-16;V21;H-2;V-44;G32,5;H16;V19;H-16;V-19;" + // A
                "G57,3;V44;H2;V-36;H16;V36;H2;V-44;H-2;V6;H-16;V-6;H-2;" +            // M
                "G84,3;H20;V2;H-18;V19;H9;V2;H-9;V19;H18;V2;H-20;V-44;" +             // E
                "G111,3;H20;V2;H-18;V19;H9;V2;H-9;V19;H18;V2;H-20;V-44;" +            // E
                "G138,3;H20;V2;H-18;V19;H9;V2;H-9;V19;H18;V2;H-20;V-44"),             // E
            logoInnerView = LineRenderer.Path.Parse(
                "G22,10;V-6;H-18;V42;H18;V-21;H-6;" + // G
                "G31,46;V-42;H18;V42;G31,25;H18;" +   // A
                "G58,4;V42;G58,10;H18;G76,4;V42;" +   // M
                "G103,4;H-18;V42;H18;G85,25;H9;" +    // E
                "G130,4;H-18;V42;H18;G112,25;H9;" +   // E
                "G157,4;H-18;V42;H18;G139,25;H9;");   // E

        private readonly Pen logoOuterPen, logoInnerPen;

        public Keyboard Keyboard { get; }
        public Mouse Mouse { get; }
        public Random Rand { get; }
        public int Width { get; }
        public int Height { get; }
        public IEnumerable<IEntity> Entities => entities.Select(e => e);

        public bool Running { get; private set; }

        public Game(int width, int height) {
            Width = width;
            Height = height;
            background = Color.FromArgb(32, 32, 32);
            Keyboard = new Keyboard();
            Mouse = new Mouse();
            Rand = new Random();
            stars = new (int x, int y)[100];
            for (var i = 0; i < stars.Length; ++i) stars[i] = (Rand.Next(Width + 48) - 47, Rand.Next(Height));
            starPen = new Pen(Color.FromArgb(48, 48, 48));
            logoOuterPen = new Pen(Color.FromArgb(38, 127, 0));
            logoInnerPen = new Pen(Color.FromArgb(131, 175, 112));
            entities = new List<IEntity>();
            newEntities = new List<IEntity>();
            intervals = new Queue<double>();
            score = blinkTimer = 0;
            minMeteorInterval = 1000;
            meteorTimer = Rand.Next(400);
            paused = false;
            soundPlayer = new SoundPlayer { Stream = Resources.ThresholdOfInsanity };
            soundPlayer.Load();
            if (Settings.Default.PlayMusic) soundPlayer.PlayLooping();
            shouldExplode = false;
            Running = false;
            showingInfo = false;
            showingFps = false;
            musicLinkLocation = (Point.Empty, Point.Empty);
        }

        private void Reset() {
            minMeteorInterval = 1000;
            meteorTimer = Rand.Next(400);
            pickupTimer = 8000 + Rand.Next(4000);
            score = 0;
            entities.Clear();
            newEntities.Clear();
            ship = new Ship(100, Height / 2, this);
            Running = true;
        }

        public void Draw(Graphics g) {
            g.Clear(background);
            var renderer = new LineRenderer(g);
            var textRenderer = new LineTextRenderer(renderer);
            if (showingInfo) {
                logoOuterView.Draw(renderer, Width / 2 - 81, 5, logoOuterPen);
                logoInnerView.Draw(renderer, Width / 2 - 81, 5, logoInnerPen);
                var points = textRenderer.Draw(Resources.controls, 4, 60);
                musicLinkLocation = (points[0], points[1]);
            } else {
                foreach (var (x, y) in stars) {
                    renderer.GoTo(x, y);
                    renderer.HLine(starPen, 48);
                }
                foreach (var e in entities) e.Draw(renderer);
                ship?.Draw(renderer);
                var scoreText = $"Score:     {score.ToString().PadLeft(8)}";
                if (Settings.Default.Highscore > 0)
                    scoreText += $"\nHighscore: {Settings.Default.Highscore,8}";
                textRenderer.Draw(scoreText, 4, 4);
                if (!Running && blinkTimer < 500) textRenderer.Draw("Press R to start", 4, Height - 24);
                if (paused) textRenderer.Draw("Paused", Width / 2 - 51, Height / 2 - 5);
                
                if (!paused && showingFps) {
                    if (intervals.Count > 0 ) {
                        textRenderer.Draw($"{1000.0 / intervals.Average():F0} fps", 4, Height - 43);
                    }
                    textRenderer.Draw($"{renderer.DrawnLineCount} lines", 4, Height - 62);
                }
            }
        }

        public void Step(TimeSpan elapsedTime) {
            if (!paused && !showingInfo) {
                shouldExplode = false;
                if (Running) ship.Step(elapsedTime);
                int x, y;
                var millis = (int)Math.Round(elapsedTime.TotalMilliseconds);
                for (var i = 0; i < stars.Length; ++i) {
                    (x, y) = stars[i];
                    x -= (int)Math.Round(elapsedTime.TotalMilliseconds / 2.0);
                    stars[i] = x < -47 ? (x + Width, Rand.Next(Height)) : (x, y);
                }
                foreach (var e in entities) e.Step(elapsedTime);
                if (Running && shouldExplode) {
                    foreach (var m in entities.OfType<Meteor>())
                        m.Explode();
                    for (var i = 0; i < 200; ++i)
                        CreateFlames(Rand.Next(Width), Rand.Next(Height), 0, 1);
                }
                Cleanup(entities);
                entities.AddRange(newEntities);
                newEntities.Clear();
                meteorTimer = Math.Max(0, meteorTimer - millis);
                if (meteorTimer == 0) {
                    entities.Add(new Meteor(this));
                    meteorTimer = minMeteorInterval + Rand.Next(minMeteorInterval / 2);
                    if (Running && minMeteorInterval > 150)
                        minMeteorInterval -= 4;
                }
                blinkTimer = (blinkTimer + millis) % 1000;
                if (Running) {
                    scoreTimer += millis;
                    score += scoreTimer / 50;
                    scoreTimer %= 50;
                    pickupTimer = Math.Max(0, pickupTimer - millis);
                    if (pickupTimer == 0) {
                        entities.Add(new Pickup(this));
                        pickupTimer = 8000 + Rand.Next(4000);
                    }
                }
                intervals.Enqueue(elapsedTime.TotalMilliseconds);
                if (intervals.Count > 50) intervals.Dequeue();
            }
        }

        private void Cleanup<T>(List<T> list) where T : IEntity {
            var nextFree = 0;
            for (var i = 0; i < list.Count; ++i) {
                if (!list[i].Dead) {
                    if (nextFree != i) list[nextFree] = list[i];
                    ++nextFree;
                }
            }
            if (nextFree < list.Count) list.RemoveRange(nextFree, list.Count - nextFree);
        }

        public void CreateShot(int x, int y) => newEntities.Add(new Shot(x, y, this));

        public void CreateSparkles(int x, int y, int radius, int count) {
            for (var i = 0; i < count; ++i) {
                var d = Rand.Next(radius + 1);
                if (d == 0) newEntities.Add(new Sparkle(x, y, this));
                else {
                    var α = Rand.Next(360) * Math.PI / 180.0;
                    newEntities.Add(new Sparkle(x + (int)Math.Round(d * Math.Cos(α)), y + (int)Math.Round(d * Math.Sin(α)), this));
                }
            }
        }

        public void CreateFlames(int x, int y, int radius, int count) {
            for (var i = 0; i < count; ++i) {
                var d = Rand.Next(radius + 1);
                if (d == 0) newEntities.Add(new Flame(x, y, this));
                else {
                    var α = Rand.Next(360) * Math.PI / 180.0;
                    newEntities.Add(new Flame(x + (int)Math.Round(d * Math.Cos(α)), y + (int)Math.Round(d * Math.Sin(α)), this));
                }
            }
        }

        public void AddScore(int ammount) => score += ammount;

        public void TogglePause() => paused = Running && !paused;

        public void Explode() => shouldExplode = true;

        public void End() {
            Running = false;
            if (score > Settings.Default.Highscore) {
                Settings.Default.Highscore = score;
                Settings.Default.Save();
            }
        }

        public void PressKey(Keys key) {
            Keyboard.PressKey(key);
            switch (key) {
                case Keys.R:
                    if (!Running && !showingInfo) Reset();
                    break;
                case Keys.P:
                    if (!showingInfo) TogglePause();
                    break;
                case Keys.Escape:
                    Exit();
                    break;
                case Keys.M:
                    if (Settings.Default.PlayMusic) {
                        soundPlayer.Stop();
                        Settings.Default.PlayMusic = false;
                    } else {
                        soundPlayer.PlayLooping();
                        Settings.Default.PlayMusic = true;
                    }
                    Settings.Default.Save();
                    break;
                case Keys.F:
                    showingFps = !showingFps;
                    break;
                case Keys.F1:
                    showingInfo = !showingInfo;
                    break;
            }
        }

        private void Exit() {
            Application.Exit();
        }

        public void ReleaseKey(Keys key) => Keyboard.ReleaseKey(key);

        public void PressMouseButton(MouseButtons button) {
            Mouse.PressMouseButton(button);
            if (button == MouseButtons.Left &&
                Mouse.X > musicLinkLocation.tl.X &&
                Mouse.Y > musicLinkLocation.tl.Y &&
                Mouse.X < musicLinkLocation.br.X &&
                Mouse.Y < musicLinkLocation.br.Y) {
                Process.Start("http://teknoaxe.com/Link_Code_3.php?q=1023");
            }
        }

        public void ReleaseMouseButton(MouseButtons button) => Mouse.ReleaseMouseButton(button);

        public void MoveMouse(Point location) => Mouse.MoveMouse(location);

    }

}
