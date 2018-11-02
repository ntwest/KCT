using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalConstructionTime
{
    interface IAssemblyFacility
    {
        int BuildRate { get; set; }
    }

    class VABAssemblyFacility
    {
        public int BuildRate { get; private set; }        
    }
}
