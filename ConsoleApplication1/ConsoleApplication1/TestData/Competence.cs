using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1.TestData
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

    public class CompetenceMatch
    {

        public CompetenceMatch(Competence competence, Competence firstOrDefault, int? compareTo)
        {
            
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

    public class Competences : IReadOnlyCollection<Competence>
    {
        private readonly List<Competence> _competences = new List<Competence> ();

        private Competences(IEnumerable<Competence> competences)
        {
            _competences.AddRange(competences);
        }

        internal Competences()
        {
        }

        public Competences AddKeyValue<TKey, TValue>(TKey key, TValue value)
            where TKey : IEquatable<TKey>
            where TValue : IComparable<TValue>, IComparable
        {
            return Add(new Competence<TKey, TValue>(key, value));
        }

        public Competences AddKey<TKey>(TKey key)
    where TKey : IEquatable<TKey>
    
        {
            return AddKeyValue(key, new AnyValueComparable());
        }

        internal class AnyValueComparable : IComparable, IComparable<AnyValueComparable>
        {
            public int CompareTo(object obj)
            {
                return obj != null ? 0 : 1;
            }

            public int CompareTo(AnyValueComparable other)
            {
                return CompareTo((object)other);
            }
        }


        public Competences Add(Competence competence)
        {
            if (competence == null)
                return this;
            _competences.Add(competence);
            return this;
        }


        public IEnumerator<Competence> GetEnumerator()
        {
            return _competences.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _competences.Count;



        public bool Acceptable(IEnumerable<Competence> competences, out IEnumerable<CompetenceMatchingResult> result)
        {
            result = this
                .Select
                (
                    required => new CompetenceMatchingResult
                    (required, 
                        competences
                            .Select(variant => Competence.Acceptable(required, variant))
                            .FirstOrDefault(y => y != null))
                            )
                .ToArray();

            return result.All(x => x != null);
        }

        public Competences AnyOf(IReadOnlyCollection<Competence> competences)
        {
            Add(new AnyOfCompetence(competences as Competences ?? new Competences(competences) ));
            return this;
        }

        public static Competences New()
        {
            return new Competences();
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

    public static class MemberOfCompetenceExtender
    {
        public static Competences MemberOf(this Competences competences, params MembershipItem[] memberOf)
        {
            foreach (var membership in memberOf)
            {
                competences = competences.Add(new MemberOfCompetence(membership));
            }

            return competences;
        }
    }

    public class MemberOfCompetence : Competence
    {
        public MemberOfCompetence(MembershipItem membershipItem)
        {
            MembershipItem = membershipItem;
        }

        public MembershipItem MembershipItem { get; }
        protected override object CompetenceKey => GetType();
        protected override Competence CompareValues(Competence other, bool backward = false)
        {
            if (backward)
                return null;

            var competence = other as MemberOfCompetence;
            return competence?.MembershipItem.MemberOfOrEqual(MembershipItem) == true ? competence : null;
        }

        public override string ToString()
        {
            return $"Member of [{MembershipItem}]";
        }
    }

    public abstract class MembershipItem : Entity
    {
        public string Caption { get; }
        protected MembershipItem(string caption, IEnumerable<MembershipItemsContainer> memberOf)
        {
            Caption = caption;
            MemberOf = memberOf;
        }

        protected IEnumerable<MembershipItem> MemberOf { get; }

        public virtual bool MemberOfOrEqual(MembershipItem item)
        {
            return Equals(this, item) || MemberOf?.Any(x => x.MemberOfOrEqual(item)) == true;
        }

        public override string ToString()
        {
            return Caption ?? $"{base.ToString()}[{GetHashCode()}]";
        }
    }

    public abstract class MembershipItemsContainer : MembershipItem
    {
        public MembershipItemsContainer(string caption, IEnumerable<MembershipItemsContainer> memberOf) : base(caption, memberOf)
        {
        }
    }

    public class Role : MembershipItemsContainer
    {
        public Role(string caption = null) : base(caption, new Role[0])
        {
        }

        public override bool MemberOfOrEqual(MembershipItem item)
        {
            return Equals(this, item);
        }
    }

    public class Department : MembershipItemsContainer
    {
        public Department(string caption, params Department[] memberOf) : base(caption, memberOf)
        {
        }

        public override bool MemberOfOrEqual(MembershipItem item)
        {
            return base.MemberOfOrEqual(item);
        }
    }
}