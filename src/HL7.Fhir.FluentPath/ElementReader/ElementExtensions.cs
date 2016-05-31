using System.Linq;
using System.Collections.Generic;

namespace Hl7.Fhir.ElementModel
{

    public static class ElementExtensions
    {
        public static IEnumerable<string> GetChildNames<T>(this T element) where T: INodeReader<T>, INameProvider
        {
            // todo: might need optimization
            return element.GetChildren().Select(e => e.Name).Distinct();
        }

        public static IEnumerable<T> GetChildrenByName<T>(this T element, string name) where T: INodeReader<T>, INameProvider
        {
            return element.GetChildren().Where(e => e.Name == name);
        }
    }

}