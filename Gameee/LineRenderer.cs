using System;
using System.Collections.Generic;
using System.Drawing;

namespace Gameee {

    internal class LineRenderer {

        private readonly Graphics g;
        private int x, y;

        public ulong DrawnLineCount { get; private set; }

        public LineRenderer(Graphics g) {
            this.g = g;
            x = y = 0;
            DrawnLineCount = 0;
        }

        public void GoTo(int _x, int _y) => (x, y) = (_x, _y);

        public void HLine(Pen p, int length) {
            ++DrawnLineCount;
            g.DrawLine(p, x, y, x += length, y);
        }

        public void VLine(Pen p, int length) {
            ++DrawnLineCount;
            g.DrawLine(p, x, y, x, y += length);
        }
        
        public class Path {

            private abstract class Step { }

            private class GoTo : Step {
                public readonly int X, Y;
                public GoTo(int x, int y) {
                    X = x;
                    Y = y;
                }
            }

            private class H : Step {
                public readonly int L;
                public H(int l) => L = l;
            }

            private class V : Step {
                public readonly int L;
                public V(int l) => L = l;
            }

            private readonly Step[] steps;

            private Path(Step[] steps) => this.steps = steps;

            public static Path Parse(string path) {
                var pathSteps = new List<Step>();
                foreach (var step in path.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) {
                    if (step[0] == 'G') {
                        var xy = step.Substring(1).Split(',');
                        pathSteps.Add(new GoTo(int.Parse(xy[0]), int.Parse(xy[1])));
                    } else if (step[0] == 'V') {
                        pathSteps.Add(new V(int.Parse(step.Substring(1))));
                    } else if (step[0] == 'H') {
                        pathSteps.Add(new H(int.Parse(step.Substring(1))));
                    }
                }
                return new Path(pathSteps.ToArray());
            }

            public void Draw(LineRenderer lr, int x, int y, Pen p) {
                foreach (var step in steps) {
                    if (step is GoTo g) lr.GoTo(x + g.X, y + g.Y);
                    else if (step is H h) lr.HLine(p, h.L);
                    else if (step is V v) lr.VLine(p, v.L);
                }
            }
        }
    }
}