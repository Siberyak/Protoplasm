using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApplication1
{
    public abstract class EntityAgent<T> : BaseAgent
        where T : BaseEntity
    {
        protected EntityAgent(T entity)
        {
            Entity = entity;
        }

        protected T Entity { get; }

        public override IReadOnlyCollection<BaseRequirement> Requirements => Entity.Requirements;

        public override ConformableInfo[] ConformableFor(BaseAgent requirementsAgent)
        {
            var result = requirementsAgent.Requirements.Conformable(Entity.Abilities);
            return result;
        }

        protected void LifeCircle()
        {
            if (Satisfied())
            {
                return;
            }

            foreach (var variant in GetStisfactionVariants())
            {
                if(!Estimate(variant))
                    continue;
            }
        }

        private bool Satisfied()
        {
            return true;
        }

        protected IEnumerable<SatisfactionVariant> GetStisfactionVariants()
        {
            return Enumerable.Empty<SatisfactionVariant>();
        }

        public bool Estimate(SatisfactionVariant variant)
        {
            return true;
        }
    }

    public class SatisfactionVariant
    {
    }

    public static class SatisfactionHelper
    {
        public static ConformType ToConformType(this ConformableInfo[] infos)
        {
            infos = infos ?? new ConformableInfo[0];

            if (infos.Length == 0 || infos.Any(x => x.Variants.Count == 0 || x.Variants.ContainsKey(ConformType.Imposible)))
                return ConformType.Imposible;

            if (infos.Any(x => x.Variants.ContainsKey(ConformType.Posible)))
                return ConformType.Posible;

            if (infos.All(x => x.Variants.ContainsKey(ConformType.Conform)))
                return ConformType.Conform;

            throw new NotImplementedException();
        }


        //public static IEnumerable<SatisfactionVariant> Find(IEnumerable<BaseRequirement> requirements, IEnumerable<BaseAbility> abilities)
        public static void Find(this BaseAgent current, IEnumerable<BaseAgent> agents)
        {
            foreach (var agent in agents)
            {
                var result = current.ConformableFor(agent);
                result.All(x => x.Variants.Any());
            }
        }

        public static ConformableInfo[] Conformable(this IEnumerable<BaseRequirement> requirements, IEnumerable<BaseAbility> posibleAbilities)
        {
            var abilities = posibleAbilities.ToArray();
            var result = requirements
                .Select(
                    r => new ConformableInfo
                    {
                        Requirement = r,
                        Variants = abilities
                            .Select
                            (
                                a => new
                                {
                                    Ability = a,
                                    Result = r.Conformable(a)
                                }
                            )
                            .Where(x => x.Result != ConformType.Imposible)
                            .GroupBy(x => x.Result)
                            .ToDictionary(x => x.Key, x => x.Select(y => y.Ability).ToArray())
                    }
                )
                .ToArray();

            return result;
        }
    }

    public class ConformableInfo
    {
        public static ConformableInfo[] Empty = new ConformableInfo[0];
        public BaseRequirement Requirement;
        public Dictionary<ConformType, BaseAbility[]> Variants;
        public override string ToString()
        {
            object variants = Variants.Count == 0 ? (object)ConformType.Imposible : (string.Join(", ", Variants.Select(x => $"{x.Key}:{x.Value.Length}")));
            return $"{Requirement} <- {variants}";
        }
    }
}