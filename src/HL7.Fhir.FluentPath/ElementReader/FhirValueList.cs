using System.Linq;
using System.Collections.Generic;

namespace Hl7.Fhir.ElementModel
{

    public static class FhirValueList
    {
        public static IEnumerable<IValueProvider> Create(params object[] values)
        {
            if (values != null)
            {
                return values.Select(value => value == null ? null : value is IValueProvider ? (IValueProvider)value : new ConstantValue(value));
            }
            else
            {
                return FhirValueList.Empty();
            }
        }

        public static IEnumerable<IValueProvider> Empty()
        {
            return Enumerable.Empty<IValueProvider>();
        }
    }

}