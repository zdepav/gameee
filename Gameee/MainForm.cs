using System;
using System.Windows.Forms;

namespace Gameee {

    public partial class MainForm : Form {

        private readonly Game game;
        private DateTime time;

        public MainForm() {
            InitializeComponent();
            game = new Game(800, 400);
            time = DateTime.Now;
        }

        private void MainForm_Paint(object sender, PaintEventArgs e) => game.Draw(e.Graphics);

        private void timer1_Tick(object sender, EventArgs e) {
            var ntime = DateTime.Now;
            game.Step(ntime - time);
            time = ntime;
            Refresh();
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e) => game.PressKey(e.KeyCode);

        private void MainForm_KeyUp(object sender, KeyEventArgs e) => game.ReleaseKey(e.KeyCode);

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e) => game.End();

        private void MainForm_MouseDown(object sender, MouseEventArgs e) => game.PressMouseButton(e.Button);

        private void MainForm_MouseUp(object sender, MouseEventArgs e) => game.ReleaseMouseButton(e.Button);

        private void MainForm_MouseMove(object sender, MouseEventArgs e) => game.MoveMouse(e.Location);
    }
}
