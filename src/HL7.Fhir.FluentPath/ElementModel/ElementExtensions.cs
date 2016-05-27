using System;
using System.Linq;
using System.Collections.Generic;
using Hl7.Fhir.ElementModel;

namespace Hl7.Fhir.ElementModel
{

    public static class ElementExtensions
    {
        public static IEnumerable<string> GetChildNames<T>(this T element) where T: INode<T>, INameProvider
        {
            // todo: might need optimization
            return element.GetChildren().Select(e => e.Name).Distinct();
        }

        public static IEnumerable<T> GetChildrenByName<T>(this T element, string name) where T: INode<T>, INameProvider
        {
            return element.GetChildren().Where(e => e.Name == name);
        }
    }

}