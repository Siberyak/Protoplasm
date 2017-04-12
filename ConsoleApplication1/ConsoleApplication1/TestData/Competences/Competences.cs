using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1.TestData
{
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

        public Competences AddKeyValue<TKey, TValue>(TKey key, TValue value, out Competence competence)
            where TKey : IEquatable<TKey>
            where TValue : IComparable<TValue>, IComparable
        {
            return Add(competence = new Competence<TKey, TValue>(key, value));
        }

        public Competences AddKeyValue<TKey, TValue>(TKey key, TValue value)
            where TKey : IEquatable<TKey>
            where TValue : IComparable<TValue>, IComparable
        {
            Competence competence;
            return AddKeyValue(key, value, out competence);
        }

        public Competences AddKey<TKey>(TKey key, out Competence competence)
            where TKey : IEquatable<TKey>
    
        {
            return AddKeyValue(key, new AnyValueComparable(), out competence);
        }
        public Competences AddKey<TKey>(TKey key)
            where TKey : IEquatable<TKey>

        {
            Competence competence;
            return AddKey(key, out competence);
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

            return result.All(x => x.Result != null);
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
}