using Rw.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rw.Core.Generators
{
    public sealed class GuidGenerator : IGuidGenerator
    {
        public Guid NewGuid() => Guid.NewGuid();
    }
}
