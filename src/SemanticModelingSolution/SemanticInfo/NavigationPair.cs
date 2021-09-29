using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SurrogateLibrary;

namespace SemanticLibrary
{
    /// <summary>
    /// A class storing two paths to the properties of the source and target model.
    /// It also store the evaluation (score) of the mapping
    /// </summary>
    /// <param name="Source">The source path. It points to the root item</param>
    /// <param name="Target">The target path. It points to the root item</param>
    /// <param name="Evaluation">An object containing the info about this couple of paths</param>
    public record NavigationPair(NavigationSegment<Metadata> Source, NavigationSegment<Metadata> Target,
        Evaluation Evaluation)
    {
    }
}
