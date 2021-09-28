using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public record NavigationPath<TInfo, TExtra>(NavigationSegment<TInfo> Root, TExtra PathInfo)
    {
    }
}
