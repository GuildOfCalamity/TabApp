using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TabApp;

public enum SystemStates : int
{
    None = 0,
    Init = 1,
    Ready = 2,
    Processing = 3,
    Warning = 4,
    Shutdown = 5,
    Faulted = 9,
}
