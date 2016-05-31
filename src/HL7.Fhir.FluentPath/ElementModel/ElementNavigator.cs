using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Hl7.Fhir.ElementModel
{

    public interface IElementNavigator
    {
        void MoveToNext();
    }

    public class ElementNavigator : IElementReader
    {
        private Element current;

        public string Name
        {
            get
            {
                return current.Name;
            }
        }

        public object Value
        {
            get
            {
                return current.Value;
            }
        }

        public IEnumerable<IElementReader> GetChildren()
        {
            var nav = new ElementNavigator();

            nav.current = current.FirstChild;
            while (nav.current.Next != null)
            {
                nav.current = nav.current.Next;
                yield return nav;
            }
        }

    }

}