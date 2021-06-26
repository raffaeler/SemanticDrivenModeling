using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SemanticLibrary;

namespace ManualMapping
{
    /// <summary>
    /// Classes derived from this interface represent nodes of the original Model
    /// They keep two different pieces of information
    /// The first is the Conceptual translation obtained by analyzing the model
    /// The second is the technology specific information. For example the .NET Type or properties information
    /// </summary>
    public interface IModelNode
    {
    }
}
