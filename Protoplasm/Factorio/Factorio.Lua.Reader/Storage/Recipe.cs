using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using Factorio.Lua.Reader.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protoplasm.Graph;

namespace Factorio.Lua.Reader
{
    [JsonObject("recipe", MemberSerialization = MemberSerialization.OptIn)]
    public partial class Recipe : TypedNamedIconedBase, ILocalized
    {
        public static readonly string ClockIcon = "__core__/graphics/clock-icon.png";

        string ILocalized.Category => "recipe-name";

        public override string LocalizedName => this.LocalisedName() ?? Result?.LocalisedName() ?? Name;

        [JsonProperty("localised_name")]
        public object[] _LocalisedName { get; set; }

        public string Icon => _Icon ?? (Results[0] as IRecipePart)?._Icon;

        public ItemSubGroup SubGroup => References.OfType<RecipeItemSubGroupEdge>().FirstOrDefault()?.ItemSubGroup;

        public RecipePartEdge[] Parts
        {
            get
            {
                return References.OfType<RecipePartEdge>().OrderBy(x => x.Direction)
                    .ToArray();
            }
        }

        public IRecipePart[] AllParts
        {
            get
            {
                return Parts.Select(x => x.Part).Union(Results).ToArray();
            }
        }

        public IRecipePart[] Results
        {
            get
            {
                return References.OfType<RecipePartEdge>().Where(x => x.Direction == Direction.Output)
                    .Select(x => x.Part)
                    .ToArray();
            }
        }
        public ILocalized Result
        {
            get
            {
                var outs = References.OfType<RecipePartEdge>().Where(x => x.Direction == Direction.Output).ToArray();
                if (outs.Length == 1)
                {
                    return outs[0].Part as ILocalized;
                }

                return null;
            }
        }

        [JsonProperty("result")]
        public string _Result { get; set; }

        [JsonProperty("result_count")]
        private uint _ResultCount { get; set; }

        [JsonProperty("category")]
        private string _RecipeCategory { get; set; }

        [JsonProperty("subgroup")]
        public string _Subgroup { get; set; }

        [JsonProperty("energy_required")]
        public double _EnergyRequired { get; set; } = 0.5;

        [JsonProperty("enabled")]
        public bool _Enabled { get; set; }

        [JsonProperty("results")]
        public object[] _Results { get; set; }

        [JsonProperty("ingredients")]
        public object[] _Ingredients { get; set; }

        [JsonProperty("icons")]
        public IconInfo[] Icons { get; set; }


        public override void SetToken(JToken token)
        {
            base.SetToken(token);
            if (_Ingredients == null)
            {
                var normalToken = ((JToken)_Normal);
                var json = normalToken.ToString();
                JsonConvert.PopulateObject(json, this);
            }
        }

        [JsonProperty("normal")]
        public object _Normal { get; set; }

        [JsonProperty("hidden")]
        public bool _Hidden { get; set; }

        public override void ProcessLinks()
        {
            base.ProcessLinks();

            // inputs
            {
                foreach (var token in _Ingredients.Cast<JToken>())
                {
                    //}
                    //foreach (var token in _token["ingredients"])
                    //{
                    Populate(token, true);
                }

            }

            // outputs
            {

                var collectionToken = (_Results?.Cast<JToken>() ?? Enumerable.Empty<JToken>()).ToArray();
                if (collectionToken.Any())
                {
                    foreach (var token in collectionToken)
                    {
                        Populate(token, false);
                    }
                }
                else
                {
                    Populate(_Result, Math.Max(_ResultCount, 1), false);
                }
            }


            // category
            {
                var category = ((JObject)_token).Property("category")?.Value.Value<string>() ?? "crafting";
                var recipeCategory = Storage.RecipeCategories.First(x => x.Name == category);
                Storage.Link<RecipeRecipeCategoryEdge>(this, recipeCategory);
            }

            var itemSubGroup = !string.IsNullOrEmpty(_Subgroup)
                ? Storage.Nodes<ItemSubGroup>(x => x.Name == _Subgroup).FirstOrDefault()
                : null;

            if (itemSubGroup == null)
            {
                var pd = TypeDescriptor.GetProperties(Results[0]).OfType<PropertyDescriptor>()
                    .FirstOrDefault(x => x.Attributes.OfType<JsonPropertyAttribute>().Any(y => y.PropertyName == "subgroup"));
                var sb = (string)pd.GetValue(Results[0]);

                itemSubGroup = !string.IsNullOrEmpty(sb)
                    ? Storage.Nodes<ItemSubGroup>(x => x.Name == sb).FirstOrDefault()
                    : null;
            }

            if (itemSubGroup == null)
            {
                
            }

                Storage.Link<RecipeItemSubGroupEdge>(this, itemSubGroup);
        }

        public RecipeCategory RecipeCategory => References.OfType<RecipeRecipeCategoryEdge>().FirstOrDefault()?.To;
        public string Order => _Order ?? Results.FirstOrDefault()?._Order;

        [JsonProperty("order")]
        public string _Order { get; set; }
        private void Populate(JToken token, bool input)
        {
            object amount = null;
            string name = null;

            if (token.Type == JTokenType.Object)
            {
                dynamic source = token;

                name = source.name;
                amount = source.amount;
            }
            else if (token.Type == JTokenType.Array)
            {
                name = token[0].Value<string>();
                amount = token[1].Value<double>();
            }

            Populate(name, amount, input);
        }

        private void Populate(string name, object amount, bool input)
        {
            var item = Storage.FindNode<IRecipePart>(x => x.Name == name);

            if (item == null)
            {
                var arr = Storage.Nodes.OfType<TypedNamedBase>().Where(x => x.Name == name).ToArray();
            }

            var link = Storage.Link<RecipePartEdge>(this, item);
            link.Direction = input ? Direction.Input : Direction.Output;
            link.Amount = amount;
        }
    }
}