using System.Collections.Generic;

namespace Hl7.Fhir.ElementModel
{
    public interface INodeReader<T> where T : INodeReader<T>
    {
        IEnumerable<T> GetChildren();
    }


}
