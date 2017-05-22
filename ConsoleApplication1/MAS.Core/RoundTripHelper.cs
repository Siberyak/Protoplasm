using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MAS.Core.Compatibility.Contracts;
using MAS.Core.Contracts;

namespace MAS.Core
{
    public static class RoundTripHelper
    {
        private delegate IScene VariateDelegate(INegotiator requirements, INegotiator abilities);

        private delegate bool HolderVariateDelegate(IHoldersCompatibilityInfo info, IScene current, out IScene result);

        private static bool TrySatisfy(this IHolder holder, IScene current, out IScene result, HolderVariateDelegate variate, VariantKind kind)
        {
            var enumerable = holder.SatisfactionVariants(current, variate);

            result = kind == VariantKind.First
                ? enumerable.FirstOrDefault()
                : enumerable.LastOrDefault();

            return result != null && current != result;
        }

        private static bool Variate(IHoldersCompatibilityInfo info, IScene current, out IScene result, VariateDelegate variate)
        {
            var requirements = info.RequiremenetsHolder[current];
            var abilities = info.AbilitiesHolder[current];
            
            result = variate(requirements, abilities);

            return result != null;
        }

        public static bool TrySatisfy(this IAbilitiesHolder abilities, IScene current, out IScene result, VariantKind kind)
        {
            return TrySatisfy(abilities, current, out result, VariateRequirements, kind);
        }

        public static bool TrySatisfy(this IRequirementsHolder requirements, IScene current, out IScene result, VariantKind kind)
        {
            return TrySatisfy(requirements, current, out result, VariateAbilities, kind);
        }

        public static bool TrySatisfy(this IEnumerable<IAbilitiesHolder> abilities, IScene current, out IScene result, VariantKind kind)
        {
            return abilities
                .Select(x => new AHolderWrapper(x))
                .TrySatisfy(current, out result, kind);
        }

        public static bool TrySatisfy(this IEnumerable<IRequirementsHolder> requirements, IScene current, out IScene result, VariantKind kind)
        {
            return requirements.Select(x => new RHolderWrapper(x)).TrySatisfy(current, out result, kind);
        }

        private static bool TrySatisfy<T>(this IEnumerable<T> holders, IScene current, out IScene result, VariantKind kind)
            where T : HolderWrapper
        {
            result = current;
            foreach (var holder in holders)
            {
                IScene variant;
                var success = holder.TrySatisfy(current, out variant, kind);
                if (!success)
                    continue;

                if (variant.Satisfaction.CompareTo(result.Satisfaction) > 0)
                {
                    result = variant;
                    if(kind == VariantKind.First)
                        break;
                }
            }

            return result != current;
        }

        private static IEnumerable<IScene> SatisfactionVariants(this IHolder holder, IScene current, HolderVariateDelegate variate, bool improveSatisfaction = true)
        {
            var negotiator = holder[current];
            if (negotiator.IsSatisfied)
            {
                yield break;
            }

            var infos = negotiator.Agent.CompatibilityInfos();

            foreach (var info in infos)
            {
                var branch = current.Branch();
                IScene variant;

                if (!variate(info, branch, out variant))
                    continue;

                if(improveSatisfaction)
                {
                    if (variant.Satisfaction.CompareTo(current.Satisfaction) > 0)
                    {
                        current = variant;
                        yield return variant;
                    }
                }
                else
                    yield return variant;

            }
        }

        public static IEnumerable<IScene> SatisfactionVariants(this IAbilitiesHolder abilities, IScene current, bool improveSatisfaction = true)
        {
            return SatisfactionVariants(abilities, current, VariateRequirements, improveSatisfaction);
        }

        public static IEnumerable<IScene> SatisfactionVariants(this IRequirementsHolder requirements, IScene current, bool improveSatisfaction = true)
        {
            return SatisfactionVariants(requirements, current, VariateAbilities, improveSatisfaction);
        }


        public static bool VariateAbilities(this IHoldersCompatibilityInfo info, IScene current, out IScene result)
        {
            return Variate(info, current, out result, (requirements, abilities) => requirements.Variate(abilities));
        }

        public static bool VariateRequirements(this IHoldersCompatibilityInfo info, IScene current, out IScene result)
        {
            return Variate(info, current, out result, (requirements, abilities) => abilities.Variate(requirements));
        }
    }

    public enum VariantKind
    {
        First, Best
    }

    abstract class HolderWrapper
    {
        //public readonly IHolder Holder;
        public abstract bool TrySatisfy(IScene current, out IScene result, VariantKind kind);
    }

    class AHolderWrapper : HolderWrapper
    {
        private readonly IAbilitiesHolder _holder;

        public AHolderWrapper(IAbilitiesHolder holder)
        {
            _holder = holder;
        }

        public override bool TrySatisfy(IScene current, out IScene result, VariantKind kind)
        {
            return _holder.TrySatisfy(current, out result, kind);
        }
    }
    class RHolderWrapper : HolderWrapper
    {
        private readonly IRequirementsHolder _holder;

        public RHolderWrapper(IRequirementsHolder holder)
        {
            _holder = holder;
        }

        public override bool TrySatisfy(IScene current, out IScene result, VariantKind kind)
        {
            return _holder.TrySatisfy(current, out result, kind);
        }
    }

}