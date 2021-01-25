using System;
using Meadow.Peripherals.Motors;

namespace Meadow.Foundation.Motors
{
    public partial class Tb67h420ftg
    {
        public class Motor : IDCMotor
        {
            public float Speed {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }
            public bool IsNeutral {
                get => throw new NotImplementedException();
                set => throw new NotImplementedException();
            }

            internal Motor(Tb67h420ftg driver)
            {

            }
        }
    }
}
