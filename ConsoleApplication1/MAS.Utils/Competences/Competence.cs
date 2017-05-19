using System;
using System.Collections.Generic;
using System.Linq;

namespace MAS.Utils
{
    public abstract class Competence
    {
        protected abstract object CompetenceKey { get; }
        //protected abstract IComparable CompetenceValue { get; }

        public static Competences Set<TKey, TValue>(TKey key, TValue value)
        where TKey : IEquatable<TKey>
        where TValue : IComparable<TValue>, IComparable
        {
            return Competences.New().AddKeyValue(key, value);
        }

        protected abstract Competence CompareValues(Competence other, bool backward = false);
        

        public static Competence Acceptable(Competence required, Competence variant)
        {

            if (required == null && variant == null)
                return null;

            if (required == null || variant == null)
                return null;

            if (!Equals(required.CompetenceKey, variant.CompetenceKey))
                return null;

            Competence result = required.CompareValues(variant) ?? variant.CompareValues(required, true);
            return result;
        }
    }

    class Competence<TKey, TValue> : Competence
        where TKey : IEquatable<TKey>
        where TValue : IComparable<TValue>, IComparable
    {
        public readonly TKey Key;
        public readonly TValue Value;

        protected override object CompetenceKey => Key;

        //protected override IComparable CompetenceValue => Value;

        public Competence(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }

        protected override Competence CompareValues(Competence other, bool backward = false)
        {
            return CompareValues<TValue>(other, backward) 
                ?? CompareValues<Competences.AnyValueComparable>(other, backward);
        }

        protected Competence CompareValues<T>(Competence other, bool backward)
            where T : IComparable, IComparable<T>
        {
            var competence = other as Competence<TKey, T>;
            if (competence == null)
                return null;

            var compareTo = competence.Value.CompareTo(Value);

            return backward
                ? compareTo <= 0 ? (Competence)this : null
                : compareTo >= 0 ? competence : null;
        }

        public override string ToString()
        {
            return Value is Competences.AnyValueComparable 
                ? $"By key [{Key}]"
                : $"By key-value [{Key}]-[{Value}]";
        }
    }

    public class CompetenceMatchingResult
    {
        public Competence Required { get; }
        public Competence Result { get; }

        internal CompetenceMatchingResult(Competence required, Competence result)
        {
            Required = required;
            Result = result;
        }
    }

    class AnyOfCompetence : Competence
    {
        public override string ToString()
        {
            return $"Any of [{_competences.Count}] competences";
        }

        static AnyKeyEquatable _key = new AnyKeyEquatable();
        private readonly Competences _competences;
        public AnyOfCompetence(Competences competences)
        {
            _competences = competences;
        }

        private class AnyKeyEquatable
        {
            public override bool Equals(object obj)
            {
                return obj != null;
            }
        }

        protected override object CompetenceKey => _key;
        protected override Competence CompareValues(Competence other, bool backward = false)
        {
            var result = _competences.Select(x => Competence.Acceptable(x, other)).FirstOrDefault(x => x != null);
            return result;
        }
    }


}