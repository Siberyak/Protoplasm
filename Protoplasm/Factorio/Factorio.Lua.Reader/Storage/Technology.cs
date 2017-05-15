using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// ReSharper disable InconsistentNaming

namespace Factorio.Lua.Reader
{
    [JsonObject("technology", MemberSerialization = MemberSerialization.OptIn)]
    public partial class Technology : TypedNamedIconedBase, ILocalized
    {
        string ILocalized.Category => "technology-name";

        [JsonProperty("order")]
        public string Order { get; set; }


        [JsonProperty("prerequisites")]
        public string[] prerequisites { get; set; }

        [JsonProperty("effects")]
        public JObject[] effects { get; set; }

        [JsonProperty("localised_name")]
        public object[] _LocalisedName { get; set; }

        public override void ProcessLinks()
        {
            base.ProcessLinks();

            foreach (var prerequisite in prerequisites ?? Enumerable.Empty<string>())
            {
                Storage.Link<TechnologyPrerequisiteEdge>(this, Storage.Nodes.OfType<Technology>().First(x => x.Name == prerequisite));
            }

            foreach (var effect in effects ?? Enumerable.Empty<JObject>())
            {
                var recipeProperty = effect.Property("recipe");
                var rec = recipeProperty?.Value.Value<string>();

                if(string.IsNullOrWhiteSpace(rec))
                    continue;

                var typeProperty = effect.Property("type");
                var type = typeProperty?.Value.Value<string>();
                
                if(type != "unlock-recipe")
                    throw new NotImplementedException();

                Storage.Link<TechnologyUnlockRecipeEdge>(this, Storage.Nodes.OfType<Recipe>().First(x => x.Name == rec));
            }

        }
    }
}