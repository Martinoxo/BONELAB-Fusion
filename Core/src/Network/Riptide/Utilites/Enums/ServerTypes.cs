using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Riptide.Utilities
{
    /// <summary>
    /// Server types for when a server is created or joined.
    /// </summary>
    public enum ServerTypes
    {
        None = 0,
        P2P = 1,
        Dedicated = 2,
        Public = 3,
    }
}
