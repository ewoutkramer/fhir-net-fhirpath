﻿/* 
 * Copyright (c) 2017, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hl7.Fhir.Utility
{
    public interface IAnnotatable
    {
        void AddAnnotation(object annotation);

        void RemoveAnnotations(Type type);
    }

    public static class AnnotatableExtensions
    {
        public static void RemoveAnnotations<A>(this IAnnotatable annotatable)
        {
            annotatable.RemoveAnnotations(typeof(A));
        }

        public static void SetAnnotation<A>(this IAnnotatable annotatable, A annotation)
        {
            annotatable.RemoveAnnotations<A>();
            if (annotation != null)
                annotatable.AddAnnotation(annotation);
        }


    }


}