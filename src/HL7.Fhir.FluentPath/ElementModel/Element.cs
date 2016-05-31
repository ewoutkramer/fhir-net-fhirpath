using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.ElementModel
{

    public class Element : INode<Element>
    {
        public string Name;
        public string Value;

        public Element FirstChild { get; set; }
        public Element Next { get; set; }
        public Element Parent { get; set; }
    }


}
