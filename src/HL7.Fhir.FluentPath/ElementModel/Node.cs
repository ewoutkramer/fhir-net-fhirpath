using System.Collections.Generic;

namespace Hl7.Fhir.ElementModel
{
    public interface INode<T> where T : INode<T>
    {
        IEnumerable<T> GetChildren();
    }


}
