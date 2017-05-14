using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Factorio.Lua.Reader
{
    public class DependencyGraph
    {
        public int[,] adjacencyMatrix;
        private readonly List<Mod> mods = new List<Mod>();

        public DependencyGraph(List<Mod> mods)
        {
            this.mods = mods;
        }

        public void DisableUnsatisfiedMods()
        {
            var changeMade = true;
            while (changeMade)
            {
                changeMade = false;
                foreach (var mod in mods)
                {
                    foreach (var dep in mod.parsedDependencies)
                    {
                        if (!DependencySatisfied(dep))
                        {
                            if (mod.Enabled)
                            {
                                changeMade = true;
                                mod.Enabled = false;
                            }
                        }
                    }
                }
            }
        }

        //Assumes no dependency cycles
        public List<Mod> SortMods()
        {
            UpdateAdjacency();

            var L = new List<Mod>();
            var S = new HashSet<Mod>();

            // Get all mods with no incoming dependencies
            for (var i = 0; i < mods.Count; i++)
            {
                var dependency = false;
                for (var j = 0; j < mods.Count(); j++)
                {
                    if (adjacencyMatrix[j, i] == 1)
                    {
                        dependency = true;
                        break;
                    }
                }
                if (!dependency)
                {
                    S.Add(mods[i]);
                }
            }

            while (S.Any())
            {
                var n = S.First();
                S.Remove(n);

                L.Add(n);
                var nIndex = mods.IndexOf(n);

                for (var m = 0; m < mods.Count; m++)
                {
                    if (adjacencyMatrix[nIndex, m] == 1)
                    {
                        adjacencyMatrix[nIndex, m] = 0;

                        var incoming = false;
                        for (var i = 0; i < mods.Count; i++)
                        {
                            if (adjacencyMatrix[i, m] == 1)
                            {
                                incoming = true;
                                break;
                            }
                        }
                        if (!incoming)
                        {
                            S.Add(mods[m]);
                        }
                    }
                }
            }

            //Should be no edges (dependencies) left by here

            L.Reverse();
            return L;
        }

        public void UpdateAdjacency()
        {
            adjacencyMatrix = new int[mods.Count(), mods.Count()];

            for (var i = 0; i < mods.Count; i++)
            {
                for (var j = 0; j < mods.Count; j++)
                {
                    if (mods[i].DependsOn(mods[j], false))
                    {
                        adjacencyMatrix[i, j] = 1;
                    }
                    else
                    {
                        adjacencyMatrix[i, j] = 0;
                    }
                }
            }
        }

        public bool DependencySatisfied(ModDependency dep)
        {
            if (dep.Optional)
            {
                return true;
            }

            foreach (var mod in mods.Where(m => m.Enabled))
            {
                if (mod.SatisfiesDependency(dep))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public enum DependencyType
    {
        EqualTo,
        GreaterThan,
        GreaterThanOrEqual
    }

    public class ModDependency
    {
        public string ModName = "";
        public bool Optional;
        public Version Version;
        public DependencyType VersionType = DependencyType.EqualTo;
    }

    [JsonObject]
    public class Mod
    {
        [JsonProperty]
        public string author = "";

        [JsonProperty]
        public List<string> dependencies = new List<string>();

        [JsonProperty]
        public string description = "";

        [JsonProperty]
        public string dir = "";

        [JsonProperty]
        public bool Enabled { get; set; } = true;

        [JsonProperty]
        public string Name = "";

        [JsonIgnore]
        public List<ModDependency> parsedDependencies = new List<ModDependency>();

        [JsonIgnore]
        public Version parsedVersion;

        [JsonProperty]
        public string title = "";

        [JsonProperty]
        public string version = "";

        public bool SatisfiesDependency(ModDependency dep)
        {
            if (Name != dep.ModName)
            {
                return false;
            }
            if (dep.Version != null)
            {
                if (dep.VersionType == DependencyType.EqualTo
                    && parsedVersion != dep.Version)
                {
                    return false;
                }
                if (dep.VersionType == DependencyType.GreaterThan
                    && parsedVersion <= dep.Version)
                {
                    return false;
                }
                if (dep.VersionType == DependencyType.GreaterThanOrEqual
                    && parsedVersion < dep.Version)
                {
                    return false;
                }
            }
            return true;
        }

        public bool DependsOn(Mod mod, bool ignoreOptional)
        {
            IEnumerable<ModDependency> depList;
            if (ignoreOptional)
            {
                depList = parsedDependencies.Where(d => !d.Optional);
            }
            else
            {
                depList = parsedDependencies;
            }
            foreach (var dep in depList)
            {
                if (mod.SatisfiesDependency(dep))
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}