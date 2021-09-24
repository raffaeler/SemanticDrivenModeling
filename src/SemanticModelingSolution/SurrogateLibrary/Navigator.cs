using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public class Navigator
    {
        public Navigator(TypeSystem typeSystem)
        {
            TypeSystem = typeSystem;
        }

        public TypeSystem TypeSystem { get; init; }


    }
}
