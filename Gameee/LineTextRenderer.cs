using System;
using System.Collections.Generic;
using System.Drawing;

namespace Gameee {

    internal class LineTextRenderer {

        private static readonly Dictionary<char, LineRenderer.Path> paths;

        private static readonly Dictionary<string, ITag> tags;

        private interface ITag { void Use(State s, string param); }

        private class ColorTag : ITag {

            public void Use(State s, string param) {
                var rgb = param.Split(',');
                s.pen.Dispose();
                s.pen = new Pen(Color.FromArgb(int.Parse(rgb[0]), int.Parse(rgb[1]), int.Parse(rgb[2])));
            }
        }

        private class BoldTag : ITag {

            private readonly bool bold;

            public BoldTag(bool bold) => this.bold = bold;

            public void Use(State s, string param) => s.bold = bold;
        }

        private class MarkTag : ITag {

            private readonly bool left, top;

            public MarkTag(bool left, bool top) {
                this.left = left;
                this.top = top;
            }

            public void Use(State s, string param) => s.marks.Add(new Point(left ? s.x : s.x + 8, top ? s.y : s.y + 16));
        }

        private class State {

            public readonly string str;
            public Pen pen;
            private readonly int lx;
            public int i, x, y;
            public bool bold;
            public readonly List<Point> marks;

            public char C => str[i];
            public bool EOS => i >= str.Length;

            public State(string str, int x, int y) {
                this.str = str;
                pen = new Pen(Color.White);
                this.x = lx = x;
                this.y = y;
                i = 0;
                bold = false;
                marks = new List<Point>();
            }

            ~State() => pen.Dispose();

            public static State operator ++(State s) {
                ++s.i;
                return s;
            }

            public void NewLine() {
                x = lx;
                y += 19;
            }
        }

        static LineTextRenderer() {
            paths = new Dictionary<char, LineRenderer.Path>();
            foreach (var line in Properties.Resources.chars.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)) {
                paths[char.ToUpper(line[0])] = LineRenderer.Path.Parse(line.Substring(2));
            }
            tags = new Dictionary<string, ITag> {
                { "color", new ColorTag() },
                { "b", new BoldTag(true) },
                { "n", new BoldTag(false) },
                { "ltmark", new MarkTag(left: true, top: true) },
                { "lbmark", new MarkTag(left: true, top: false) },
                { "rtmark", new MarkTag(left: false, top: true) },
                { "rbmark", new MarkTag(left: false, top: false) }
            };
        }

        private readonly LineRenderer renderer;

        public LineTextRenderer(LineRenderer renderer) => this.renderer = renderer;

        public IList<Point> Draw(string str, int x, int y) => Draw(new State(str, x, y));

        private IList<Point> Draw(State s) {
            while (!s.EOS) {
                if (s.C == '\n') {
                    s.NewLine();
                } else if (s.C == '\\') {
                    UseTag(s);
                } else {
                    if (paths.TryGetValue(char.ToUpper(s.C), out LineRenderer.Path path)) {
                        path.Draw(renderer, s.x, s.y, s.pen);
                        if (s.bold) path.Draw(renderer, s.x + 1, s.y, s.pen);
                    }
                    s.x += 11;
                }
                ++s;
            }
            return s.marks;
        }

        private void UseTag(State s) {
            if (s.C != '\\') return;
            ++s;
            var b = s.i;
            while (!s.EOS && char.IsLetterOrDigit(s.C)) ++s;
            var tag = s.str.Substring(b, s.i - b);
            string param = null;
            if (!s.EOS && s.C == '(') {
                ++s;
                b = s.i;
                while (!s.EOS && s.C != ')') ++s;
                param = s.str.Substring(b, s.i - b);
            }
            tags[tag].Use(s, param);
        }
    }
}