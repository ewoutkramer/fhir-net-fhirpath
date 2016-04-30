﻿/* 
 * Copyright (c) 2015, Furore (info@furore.com) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/ewoutkramer/fhir-net-api/master/LICENSE
 */

using Hl7.Fhir.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hl7.Fhir.FluentPath
{
    public delegate IEnumerable<IFluentPathValue> Evaluator(IEnumerable<IFluentPathValue> focus, IEvaluationContext ctx);
   // public delegate object ScalarEvaluator(IEnumerable<IFhirPathValue> focus, IEvaluationContext ctx);

    // There is a special case around the entry point, where the type of the entry point can be represented, but is optional.
    // To illustrate this point, take the path
    //      telecom.where(use = 'work').value
    // This can be evaluated as an expression on a Patient resource, or other kind of resources. 
    // However, for natural human use, expressions are often prefixed with the name of the context in which they are used:
    //	    Patient.telecom.where(use = 'work').value
    // These 2 expressions have the same outcome, but when evaluating the second, the evaluation will only produce results when used on a Patient resource.

    public static class Eval
    {
        public static IEnumerable<IFluentPathValue> Select(this Evaluator evaluator, IFluentPathValue instance, IEvaluationContext context)
        {
            var original = FhirValueList.Create(instance);
            context.OriginalContext = original;
            return evaluator(original, context);
        }

        public static IEnumerable<IFluentPathValue> Select(this Evaluator evaluator, IFluentPathValue instance)
        {
            var original = FhirValueList.Create(instance);
            return evaluator.Select(instance, new BaseEvaluationContext());
        }

        public static object Scalar(this Evaluator evaluator, IFluentPathValue instance, IEvaluationContext context)
        {
            return evaluator.Select(instance, context).SingleValue();
        }

        public static object Scalar(this Evaluator evaluator, IFluentPathValue instance)
        {
            return evaluator.Select(instance, new BaseEvaluationContext()).SingleValue();
        }

        public static bool Predicate(this Evaluator evaluator, IFluentPathValue instance, IEvaluationContext context)
        {
            return evaluator.Select(instance, context).BooleanEval().AsBoolean();
        }

        public static bool IsTrue(this Evaluator evaluator, IFluentPathValue instance)
        {
            return evaluator.Select(instance, new BaseEvaluationContext()).BooleanEval().AsBoolean();
        }
        public static Evaluator Then(this Evaluator first, Evaluator then)
        {
            return (f,c) =>  then(first(f,c),c);
        }

        //public static Evaluator Chain(IEnumerable<Evaluator> evaluators)
        //{
        //    return c =>
        //        evaluators.Aggregate(c, (result, next) => next(result));
        //}


        public static Evaluator Return(object value)
        {
            return (_, __) =>
            {
                if (value is IFluentPathValue)
                    return new[] { (IFluentPathValue)value };
                else
                    return new[] { new ConstantValue(value) };
            };
        }

        public static Evaluator ResolveValue(string name)
        {
            return (_, ctx) => ctx.ResolveValue(name);
        }


        public static Evaluator Axis(Axis axis)
        {
            return (focus, ctx) =>
            {
                switch (axis)
                {
                    case FluentPath.Axis.Focus:
                        return focus;
                    default:
                        throw new InvalidOperationException("Internal error: unknown axis '{0}'".FormatWith(axis));
                }
            };
        }

        public static Evaluator Axis(string name)
        {
            if (name == "this")
                return Axis(FluentPath.Axis.Focus);
            else
                throw new InvalidOperationException("Internal error: unknown axis '{0}'".FormatWith(name));
        }

        public static Evaluator Length()
        {
            return (f, _) => f.MaxLength();
        }

        public static Evaluator Extension(Evaluator url)
        {
            return (f, c) =>
            {
                var u = url(f, c).AsString();
                return f.Extension(u);
            };
        }

        public static Evaluator Substring(Evaluator start, Evaluator length)
        {
            return (f, c) =>
            {
                long s = start(f, c).AsInteger();
                long? l = length != null ? length(f, c).AsInteger() : (long?)null;

                return f.Substring(s, l);
            };
        }

        public static Evaluator Infix(this Evaluator left, Operator op, Evaluator right)
        {
            return (f,c) =>
            {
                var leftNodes = left(f,c);
                var rightNodes = right(f,c);

                IEnumerable<IFluentPathValue> result = null;

                switch (op)
                {
                    case Operator.Equal:
                        result = leftNodes.IsEqualTo(rightNodes); break;
                    case Operator.Equivalent:
                        result = leftNodes.IsEquivalentTo(rightNodes); break;
                    case Operator.GreaterThan:
                        result = leftNodes.GreaterThan(rightNodes); break;
                    case Operator.GreaterOrEqual:
                        result = leftNodes.GreaterOrEqual(rightNodes); break;
                    case Operator.LessThan:
                        result = leftNodes.LessThan(rightNodes); break;
                    case Operator.LessOrEqual:
                        result = leftNodes.LessOrEqual(rightNodes); break;
                    case Operator.Add:
                        result = leftNodes.Add(rightNodes); break;
                    case Operator.Sub:
                        result = leftNodes.Sub(rightNodes); break;
                    case Operator.Mul:
                        result = leftNodes.Mul(rightNodes); break;
                    case Operator.Div:
                        result = leftNodes.Div(rightNodes); break;
                    case Operator.And:
                        result = leftNodes.And(rightNodes); break;
                    case Operator.Or:
                        result = leftNodes.Or(rightNodes); break;
                    case Operator.Xor:
                        result = leftNodes.Xor(rightNodes); break;
                    case Operator.Implies:
                        result = leftNodes.Implies(rightNodes); break;
                    case Operator.Union:
                        result = leftNodes.Union(rightNodes); break;
                    case Operator.Concat:
                        result = leftNodes.Add(rightNodes); break;  // should only work for strings ;-)                        
                    case Operator.In:
                        result = leftNodes.SubsetOf(rightNodes); break;
                    default:
                        throw Error.NotImplemented("Infix operator '{0}' is not yet implemented".FormatWith(op));
                }

                return result;
            };
        }

        public static Evaluator Where(Evaluator condition)
        {
            return (f,c)=> f.Where(elements => condition(elements,c));
        }

        public static Evaluator All(Evaluator condition)
        {
            return (f,c) => f.All(elements => condition(elements,c));
        }

        public static Evaluator Any(Evaluator condition)
        {
            return (f, c) =>
            {
                if (condition != null)
                    return f.Any(elements => condition(elements, c));
                else
                    return FhirValueList.Create(f.Any());
            };
        }

        public static Evaluator Select(Evaluator mapper)
        {
            return (f, c) => f.Select(elements => mapper(elements, c));
        }

        public static Evaluator Empty()
        {
            return (f,_)=> f.IsEmpty();
        }

        public static Evaluator Not()
        {
            return (f, _) => f.Not();
        }

        public static Evaluator Item(Evaluator index)
        {
            return (f,c) =>
            {
                var ix = index(f,c).Single().AsInteger();
                return f.Item((int)ix);
            };
        }

        public static Evaluator First()
        {
            return (f,_) => f.Take(1);
        }


        public static Evaluator Last()
        {
            return (f, _) => f.Reverse().Take(1);
        }

        public static Evaluator Tail()
        {
            return (f,_) => f.Skip(1);
        }

        public static Evaluator Skip(Evaluator num)
        {
            return (f,c) =>
            {
                var ix = num(f,c).Single().AsInteger();
                return f.Skip((int)ix);
            };
        }

        public static Evaluator Take(Evaluator num)
        {
            return (f,c) =>
            {
                var ix = num(f,c).Single().AsInteger();
                return f.Take((int)ix);
            };
        }

        public static Evaluator Count()
        {
            return (f, _) => f.CountItems();
        }

        public static Evaluator AsInteger()
        {
            return (f,_) => f.IntegerEval();
        }

        public static Evaluator StartsWith(Evaluator prefix)
        {
            return (f, c) =>
            {
                var p = prefix(f, c).Single().AsString();
                return f.StartingWith(p);
            };
        }

        public static Evaluator Log(Evaluator argument)
        {
            return (f, c) =>
            {
                var arg = argument(f, c).Single().AsString();
                c.Log(arg, f);
                return f;
            };
        }

        /// <summary>
        /// This value is here primarily for unit testing to make the value predictable
        /// </summary>
        public static DateTimeOffset? FixedEvaluateNowValue;
        public static Evaluator Now()
        {
            return (f, c) =>
            {
                if (FixedEvaluateNowValue.HasValue)
                {
                    return new[] { new ConstantValue(PartialDateTime.FromDateTime(FixedEvaluateNowValue.Value)) };
                }
                return new[] { new ConstantValue(PartialDateTime.Now()) };
            };
        }

        public static Evaluator Today()
        {
            return (f, c) =>
            {
                if (FixedEvaluateNowValue.HasValue)
                {
                    return new[] { new ConstantValue(PartialDateTime.FromDateTime(FixedEvaluateNowValue.Value.Date)) };
                }
                return new[] { new ConstantValue(PartialDateTime.Today()) };
            };
        }

        public static Evaluator Distinct()
        {
            return (f, c) => f.Distinct();
        }

        public static Evaluator Contains(Evaluator substring)
        {
            return (f, c) =>
            {
                var subs = substring(f, c).AsString();
                return f.JustValues().Where(v => v.AsStringRepresentation().Contains(subs));
            };
        }

        public static Evaluator Matches(Evaluator regexp)
        {
            return (f, c) =>
            {
                var r = regexp(f, c).AsString();
                return f.JustValues().Where(v => Regex.IsMatch(v.AsStringRepresentation(), r));
            };
        }

        public static Evaluator Children(Evaluator nameParam)
        {
            return (f,c) =>
            {
                var name = nameParam(f,c).Single().AsString();
                return f.Children(name);
            };
        }

        public static Evaluator Children(string name)
        {
            return Children(Eval.Return(name));
        }


        public static Evaluator Function(string name, IEnumerable<Evaluator> paramList)
        {
            // Later: provide a hook for custom functions
            throw Error.NotSupported("An unknown function '{0}' is invoked".FormatWith(name));
        }

        //public static Evaluator<decimal> Add(this Evaluator<decimal> left, Evaluator<decimal> right)
        //{
        //    return c => Result.ForValue(left(c).Value + right(c).Value);
        //}

        //public static Evaluator<decimal> Sub(this Evaluator<decimal> left, Evaluator<decimal> right)
        //{
        //    return c => Result.ForValue(left(c).Value - right(c).Value);
        //}

        //public static Evaluator<decimal> Mul(this Evaluator<decimal> left, Evaluator<decimal> right)
        //{
        //    return c => Result.ForValue(left(c).Value * right(c).Value);
        //}

        //public static Evaluator<decimal> Div(this Evaluator<decimal> left, Evaluator<decimal> right)
        //{
        //    return c => Result.ForValue(left(c).Value / right(c).Value);
        //}

        //public static Evaluator<string> Add(this Evaluator<string> left, Evaluator<string> right)
        //{
        //    return c => Result.ForValue(left(c).Value + right(c).Value);
        //}
    }


    public enum Axis
    {
        //Children,
        //Descendants,
        //Context,
        //Resource,
        //Parent,
        Focus
    }

  }
