using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SurrogateLibrary
{
    public record NavigationArc
    {
        public NavigationArc(NavigationArc previous, SurrogateProperty next) =>
            (Previous, Next) = (previous, next);

        public NavigationArc Previous { get; init; }
        public SurrogateProperty Next { get; init; }
        public string Path { get; set; }

    }

    public record NavigationArc2
    {
        public NavigationArc2(SurrogateProperty previous, SurrogateProperty next) =>
            (Previous, Next) = (previous, next);

        public SurrogateProperty Previous { get; init; }
        public SurrogateProperty Next { get; init; }
        public string Path { get; set; }

    }


}
