using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.ElementModel
{
    public interface INode<T> where T: INode<T>
    {
        T Parent { get; set; }
        T FirstChild { get; set; }
        T Next { get; set; }
    }
}
