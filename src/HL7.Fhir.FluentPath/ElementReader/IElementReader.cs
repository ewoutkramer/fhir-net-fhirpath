/* 
 * Copyright (c) 2015, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */

using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.ElementModel;

namespace Hl7.Fhir.ElementModel
{

    public interface IElementReader : INodeReader<IElementReader>, INameProvider, IValueProvider { }

}