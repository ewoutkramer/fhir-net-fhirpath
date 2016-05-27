
using Hl7.Fhir.FluentPath.InstanceTree;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.ElementModel;

namespace Hl7.Fhir.FluentPath
{

    public class FhirEvaluationContext : BaseEvaluationContext
    {
        IElement OriginalResource { get; }

        public FhirEvaluationContext()
        {
        }

        public FhirEvaluationContext(FhirClient client) : this(client, null)
        {
        }

        public FhirEvaluationContext(IElement originalResource) : this(null, originalResource)
        {
        }

        public FhirEvaluationContext(FhirClient client, IElement originalResource)
        {
            FhirClient = client;
            OriginalResource = originalResource;
        }


        FhirClient FhirClient { get; set; }

        public override IEnumerable<ITypedValueProvider> InvokeExternalFunction(string name, IEnumerable<ITypedValueProvider> focus, IEnumerable<IEnumerable<ITypedValueProvider>> parameters)
        {
            if(name == "resolve")
            {
                throw new NotImplementedException();        
            }
            else
                throw new NotSupportedException($"Function '{name}' is unknown");
        }


        public override IEnumerable<ITypedValueProvider> ResolveValue(string name)
        {
            var baseValue = base.ResolveValue(name);
            if (baseValue != null) return baseValue;

            if (name == "resource")
                return FhirValueList.Create(OriginalResource);

            return null;
        }

        public virtual IElement ResolveResource(string url)
        {
            if (FhirClient == null)
                throw Error.InvalidOperation($"The EvaluationContext does not have a FhirClient to use to resolve url '{url}'");

            try
            {
                var resource = FhirClient.Get(url);
                if (resource == null) return null;

                var xml = FhirSerializer.SerializeResourceToXml(resource);
                return TreeConstructor.FromXml(xml);
            }
            catch (Exception e)
            {
                throw e;
                //return null;
            }
        }
    }

}
