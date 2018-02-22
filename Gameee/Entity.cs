using System;

namespace Gameee {

    internal interface IEntity {

        void Step(TimeSpan elapsedTime);

        void Draw(LineRenderer r);

        bool Dead { get; }
    }
}
