using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace Rw.Core.Interfaces
{
    public interface IRwTool
    {
        Command CreateCommand();
    }
}
