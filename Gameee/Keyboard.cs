using System.Collections.Generic;
using System.Windows.Forms;

namespace Gameee {

    internal class Keyboard {

        private readonly Dictionary<Keys, bool> keys;

        public bool this[Keys k] => keys.TryGetValue(k, out bool b) && b;

        public Keyboard() => keys = new Dictionary<Keys, bool>();

        public void PressKey(Keys k) => keys[k] = true;

        public void ReleaseKey(Keys k) => keys[k] = false;
    }
}
