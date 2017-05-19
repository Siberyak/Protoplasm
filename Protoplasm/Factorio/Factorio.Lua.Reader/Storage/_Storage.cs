using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using DevExpress.XtraPrinting.Native;
using Factorio.Lua.Reader;
using Factorio.Lua.Reader.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Protoplasm.Utils;
using Protoplasm.Graph;

namespace Factorio.Lua.Reader
{
    public class Storage : MutableDataGraph
    {
        //public Storage() : base(FF.Instance, FF.Instance)
        //{
        //}

        public static string ModsPath = "_data\\_Factorio_mods";
        abstract class LoadHelper
        {
            public abstract IEnumerable Load(Storage storage, JObject data, string property);
        }
        class LoadHelper<T> : LoadHelper
            where T : Base
        {
            public override IEnumerable Load(Storage storage, JObject data, string property)
            {
                return Load<T>(storage, data, property);
            }
        }


        private static Dictionary<Type, JsonObjectAttribute> _typesAttributes;

        public static List<Base> LoadAll(Storage storage, JObject data)
        {
            var all = new List<Base>();
            foreach (var type in _typesAttributes.Keys)
            {
                var helperType = typeof(LoadHelper<>).MakeGenericType(type);
                var helper = (LoadHelper)Activator.CreateInstance(helperType);
                var property = _typesAttributes[type].Id;
                var result = helper.Load(storage, data, property).Cast<Base>().ToArray();
                all.AddRange(result);
            }
            return all;
        }

        public static List<T> Load<T>(Storage storage, JObject data, string property = null)
            where T : Base
        {
            var list = new List<T>();
            property = property ?? _typesAttributes[typeof(T)].Id;

            if (data.Property(property) == null)
                return list;

            var properties = ((JObject)data[property]).Properties();


            foreach (var token in properties)
            {
                var value = (JObject)token.Value;
                T item;

                if (true)
                {
                    item = Activator.CreateInstance<T>();
                    item.SetGraph(storage);
                    storage.AddNode((INode)item);
                    var json = value.ToString();
                    JsonConvert.PopulateObject(json, item);
                }
                else
                {
                    item = value.ToObject<T>();
                }

                item.SetToken(token.Children().First());
                list.Add(item);


            }

            return list;
        }

        public static Storage Current
        {
            get
            {
                lock (_locker)
                {
                    return _current ?? (_current = Load());
                }
            }
        }

        static readonly object _locker = new object();

        private static Storage Load()
        {

            var flag = false;
            if (flag)
                FactorioProgram.Load();


            _typesAttributes = typeof(Base).Assembly.GetTypes()
                .Select(x => new { Type = x, Attribute = x.Attribute<JsonObjectAttribute>() })
                .Where(x => typeof(Base).IsAssignableFrom(x.Type) && x.Attribute != null && !String.IsNullOrEmpty(x.Attribute.Id))
                .ToDictionary(x => x.Type, x => x.Attribute);

            var storage = new Storage();
            storage.StringsByCategory = LoadJson(LocaleJson);
            storage.Mods = LoadJson<Dictionary<string, Mod>>(ModsJson);

            var data = LoadJson(DataJson);

            var props = new Dictionary<string, List<PropInfo>>();

            CollectProperties(data, props);

            //AnalyzeData(data, props);

            GenerateClasses(data, props);

            var keys = data.Keys();

            _typesAttributes = typeof(Base).Assembly.GetTypes()
                .Select(x => new { Type = x, Attribute = x.Attribute<JsonObjectAttribute>() })
                .Where(x => typeof(Base).IsAssignableFrom(x.Type) && x.Attribute != null && !String.IsNullOrEmpty(x.Attribute.Id))
                .ToDictionary(x => x.Type, x => x.Attribute);

            var all = LoadAll(storage, data);

            foreach (var @base in all)
            {
                @base.ProcessLinks();
            }

            var parts = storage.Edges.OfType<RecipePartEdge>().ToArray();

            var recepted = storage.Items.Where(x => x.BackReferences.OfType<RecipePartEdge>().Any(y => y.Direction == Direction.Output)).ToArray();
            var notRecepted = storage.Items.Where(x => x.BackReferences.OfType<RecipePartEdge>().All(y => y.Direction != Direction.Output)).ToArray();

            var itemGroups = storage.ItemGroups.ToArray();

            var errors = storage.Edges.Select(x => x.From).Where(x => !(x is Recipe)).ToArray();

            //            var assemblingMachines = storage.Nodes<AssemblingMachine>().ToArray();
            
            var tmp = storage.Nodes.OfType<TypedNamedBase>().Where(x => x.Name == "sulfur").ToArray();
            var tmp2 = storage.Nodes.OfType<TypedNamedBase>().Where(x => x.Name.Contains("density")).ToArray();

            var recipeCategories = storage.Nodes.OfType<RecipeCategory>().ToArray();
            var assemblingMachines = storage.Nodes.OfType<AssemblingMachine>().ToArray();


            Optimize(storage);

            return storage;
        }

        static void Optimize(Storage storage)
        {
            var outs = storage.Edges<RecipePartEdge>(x => x.Direction == Direction.Output).ToArray();
            var byCategory = outs.GroupBy(x => x.Recipe.RecipeCategory).ToDictionary(x => x.Key, x => x.ToArray());

            //var craft = storage.Nodes<RecipeCategory>(x => x.Name == "craft").First();

        }

        public T FindNode<T>(Func<T, bool> predicate)
        {
            return Nodes.OfType<T>().FirstOrDefault(predicate);
        }
        public TEdge Link<TEdge>(INode from, INode to)
    where TEdge : IEdge
        {
            var edge = (TEdge)Activator.CreateInstance(typeof(TEdge), this, from, to);
            AddEdge(edge);
            return edge;
        }


        public class PropInfo
        {
            public override string ToString()
            {
                return $"{Name}, type={Type}";
            }

            public string Name;
            public uint Count;
            public readonly List<JTokenType> Types = new List<JTokenType>();

            public string Type
            {
                get
                {
                    if (Types.Count != 1)
                        return "object";

                    switch (Types[0])
                    {
                        case JTokenType.Array:
                            return $"object[]";
                        case JTokenType.Integer:
                            return "int";
                        case JTokenType.Float:
                            return "float";
                        case JTokenType.String:
                            return "string";
                        case JTokenType.Boolean:
                            return "bool";
                        case JTokenType.Date:
                            return "System.DateTime";
                        case JTokenType.Guid:
                            return "System.Guid";
                        case JTokenType.TimeSpan:
                            return "System.TimeSpan";
                        default:
                            return "object";
                    }
                }
            }
        }

        private static void CollectProperties(IDictionary<string, JToken> dict, Dictionary<string, List<PropInfo>> props)
        {
            foreach (var dictPair in dict)
            {
                var objprops = new List<PropInfo>();
                props.Add(dictPair.Key, objprops);

                var total = new PropInfo() { Name = "-total-" };
                objprops.Add(total);


                foreach (var info in ((JObject)dictPair.Value).Properties())
                {
                    JObject obj = (JObject)info.Value;
                    total.Count++;

                    foreach (var property in obj.Properties())
                    {
                        var name = property.Name;

                        var propInfo = objprops.FirstOrDefault(x => x.Name == name);

                        if (propInfo == null)
                            objprops.Add(propInfo = new PropInfo { Name = name });

                        propInfo.Count++;

                        var type = property.Value.Type;
                        if (!propInfo.Types.Contains(type))
                            propInfo.Types.Add(type);
                    }
                }
            }
        }

        private static void CollectProps(JObject techs)
        {
            Dictionary<string, List<JTokenType>> result = new Dictionary<string, List<JTokenType>>();
            var items = techs.Properties().Select(x => (JObject)x.Value).Select(i => CollectProps(i, result)).ToArray();
        }

        private static object CollectProps(JObject tech, Dictionary<string, List<JTokenType>> result)
        {
            var props = ((JArray)tech["effects"]);
            return props;
        }

        private static void AnalyzeData(JObject data, Dictionary<string, Dictionary<string, uint>> props)
        {
            var types = data.Properties().SelectMany(x => ((JObject)x.Value).Properties()).Select(x => (JObject)x.Value).GroupBy(x => x["type"].Value<string>()).ToDictionary(x => x.Key, x => x.GroupBy(y => y["subgroup"] ?? "").ToDictionary(y => y.Key, y => y.ToArray()));

            var tmp = types.Where(x => x.Value.Count > 1).ToDictionary(x => x.Key, x => x.Value);


            var properties = data.Properties();
            foreach (JProperty property in properties)
            {
                var propName = property.Name;

                if (propName != "fluid")
                    continue;

                var propValue = (JObject)property.Value;

                var instances = propValue.Properties().Select(x => x.Value).ToArray();
                foreach (dynamic instance in instances)
                {
                    var obj = (JObject)instance;
                    var subgroup = obj["subgroup"] ?? "";
                    var type = obj["type"];
                }
            }
        }
        //*[@id="mw-content-text"]/ul[2]
        private static string xpath = "//*[@id=\"mw-content-text\"]/ul[2]";
        private static string xml = @"
<data>
<ul><li> <a href=""/Prototype/AmmoCategory"" title=""Prototype/AmmoCategory"">Prototype/AmmoCategory</a> <b>ammo-category</b></li>
<li> <a href=""/Prototype/AutoplaceControl"" title=""Prototype/AutoplaceControl"">Prototype/AutoplaceControl</a> <b>autoplace-control</b></li>
<li> <a href=""/Prototype/DamageType"" title=""Prototype/DamageType"">Prototype/DamageType</a> <b>damage-type</b></li>
<li> <a href=""/Prototype/Entity"" title=""Prototype/Entity"">Prototype/Entity</a> &lt;abstract&gt;
<ul><li> <a href=""/index.php?title=Prototype/Arrow&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Arrow (page does not exist)"">Prototype/Arrow</a> <b>arrow</b></li>
<li> <a href=""/index.php?title=Prototype/Corpse&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Corpse (page does not exist)"">Prototype/Corpse</a> <b>corpse</b></li>
<li> <a href=""/Prototype/Decorative"" title=""Prototype/Decorative"">Prototype/Decorative</a> <b>decorative</b></li>
<li> <a href=""/index.php?title=Prototype/Explosion&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Explosion (page does not exist)"">Prototype/Explosion</a> <b>explosion</b>
<ul><li> <a href=""/index.php?title=Prototype/FlameThrowerExplosion&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/FlameThrowerExplosion (page does not exist)"">Prototype/FlameThrowerExplosion</a> <b>flame-thrower-explosion</b></li></ul></li>
<li> <a href=""/Prototype/EntityWithHealth"" title=""Prototype/EntityWithHealth"">Prototype/EntityWithHealth</a> &lt;abstract&gt;
<ul><li> <a href=""/Prototype/Accumulator"" title=""Prototype/Accumulator"">Prototype/Accumulator</a> <b>accumulator</b></li>
<li> <a href=""/index.php?title=Prototype/Beacon&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Beacon (page does not exist)"">Prototype/Beacon</a> <b>beacon</b></li>
<li> <a href=""/Prototype/Car"" title=""Prototype/Car"">Prototype/Car</a> <b>car</b></li>
<li> <a href=""/Prototype/Character"" title=""Prototype/Character"">Prototype/Character</a> <b>player</b></li>
<li> <a href=""/Prototype/Container"" title=""Prototype/Container"">Prototype/Container</a> <b>container</b>
<ul><li> <a href=""/Prototype/SmartContainer"" title=""Prototype/SmartContainer"">Prototype/SmartContainer</a> <b>smart-container</b>
<ul><li> <a href=""/Prototype/LogisticContainer"" title=""Prototype/LogisticContainer"">Prototype/LogisticContainer</a> <b>logistic-container</b></li></ul></li></ul></li>
<li> <a href=""/Prototype/ElectricPole"" title=""Prototype/ElectricPole"">Prototype/ElectricPole</a> <b>electric-pole</b></li>
<li> <a href=""/index.php?title=Prototype/Fish&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Fish (page does not exist)"">Prototype/Fish</a> <b>fish</b></li>
<li> <a href=""/Prototype/Furnace"" title=""Prototype/Furnace"">Prototype/Furnace</a> <b>furnace</b></li>
<li> <a href=""/Prototype/Inserter"" title=""Prototype/Inserter"">Prototype/Inserter</a> <b>inserter</b></li>
<li> <a href=""/Prototype/Lab"" title=""Prototype/Lab"">Prototype/Lab</a> <b>lab</b></li>
<li> <a href=""/index.php?title=Prototype/Lamp&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Lamp (page does not exist)"">Prototype/Lamp</a> <b>lamp</b></li>
<li> <a href=""/index.php?title=Prototype/Market&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Market (page does not exist)"">Prototype/Market</a> <b>market</b></li>
<li> <a href=""/Prototype/MiningDrill"" title=""Prototype/MiningDrill"">Prototype/MiningDrill</a> <b>mining-drill</b></li>
<li> <a href=""/Prototype/PipeConnectable"" title=""Prototype/PipeConnectable"">Prototype/PipeConnectable</a> &lt;abstract&gt;
<ul><li> <a href=""/Prototype/AssemblingMachine"" title=""Prototype/AssemblingMachine"">Prototype/AssemblingMachine</a> <b>assembling-machine</b></li>
<li> <a href=""/index.php?title=Prototype/Boiler&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Boiler (page does not exist)"">Prototype/Boiler</a> <b>boiler</b></li>
<li> <a href=""/Prototype/Generator"" title=""Prototype/Generator"">Prototype/Generator</a> <b>generator</b></li>
<li> <a href=""/index.php?title=Prototype/Pump&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Pump (page does not exist)"">Prototype/Pump</a> <b>pump</b></li>
<li> <a href=""/index.php?title=Prototype/Pipe&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Pipe (page does not exist)"">Prototype/Pipe</a> <b>pipe</b></li>
<li> <a href=""/index.php?title=Prototype/PipeToGround&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/PipeToGround (page does not exist)"">Prototype/PipeToGround</a> <b>pipe-to-ground</b></li></ul></li>
<li> <a href=""/index.php?title=Prototype/PlayerPort&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/PlayerPort (page does not exist)"">Prototype/PlayerPort</a> <b>player-port</b></li>
<li> <a href=""/Prototype/Radar"" title=""Prototype/Radar"">Prototype/Radar</a> <b>radar</b></li>
<li> <a href=""/index.php?title=Prototype/Rail&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Rail (page does not exist)"">Prototype/Rail</a> <b>rail</b></li>
<li> <a href=""/index.php?title=Prototype/RailSignal&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/RailSignal (page does not exist)"">Prototype/RailSignal</a> <b>rail-signal</b></li>
<li> <a href=""/Prototype/Roboport"" title=""Prototype/Roboport"">Prototype/Roboport</a> <b>roboport</b></li>
<li> <a href=""/index.php?title=Prototype/Robot&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Robot (page does not exist)"">Prototype/Robot</a> &lt;abstract&gt;
<ul><li> <a href=""/index.php?title=Prototype/CombatRobot&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/CombatRobot (page does not exist)"">Prototype/CombatRobot</a> <b>combat-robot</b></li>
<li> <a href=""/index.php?title=Prototype/ConstructionRobot&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/ConstructionRobot (page does not exist)"">Prototype/ConstructionRobot</a> <b>construction-robot</b></li>
<li> <a href=""/index.php?title=Prototype/LogisticRobot&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/LogisticRobot (page does not exist)"">Prototype/LogisticRobot</a> <b>logistic-robot</b></li></ul></li>
<li> <a href=""/index.php?title=Prototype/RocketDefense&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/RocketDefense (page does not exist)"">Prototype/RocketDefense</a> <b>rocket-defense</b></li>
<li> <a href=""/Prototype/SolarPanel"" title=""Prototype/SolarPanel"">Prototype/SolarPanel</a> <b>solar-panel</b></li>
<li> <a href=""/index.php?title=Prototype/Splitter&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Splitter (page does not exist)"">Prototype/Splitter</a> <b>splitter</b></li>
<li> <a href=""/index.php?title=Prototype/TrainStop&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/TrainStop (page does not exist)"">Prototype/TrainStop</a> <b>train-stop</b></li>
<li> <a href=""/index.php?title=Prototype/TrainUnit&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/TrainUnit (page does not exist)"">Prototype/TrainUnit</a> &lt;abstract&gt;
<ul><li> <a href=""/index.php?title=Prototype/CargoWagon&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/CargoWagon (page does not exist)"">Prototype/CargoWagon</a> <b>cargo-wagon</b></li>
<li> <a href=""/Prototype/Locomotive"" title=""Prototype/Locomotive"">Prototype/Locomotive</a> <b>locomotive</b></li></ul></li>
<li> <a href=""/index.php?title=Prototype/TransportBelt&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/TransportBelt (page does not exist)"">Prototype/TransportBelt</a> <b>transport-belt</b></li>
<li> <a href=""/index.php?title=Prototype/TransportBeltToGround&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/TransportBeltToGround (page does not exist)"">Prototype/TransportBeltToGround</a> <b>transport-belt-to-ground</b></li>
<li> <a href=""/index.php?title=Prototype/Tree&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Tree (page does not exist)"">Prototype/Tree</a> <b>tree</b></li>
<li> <a href=""/index.php?title=Prototype/Turret&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Turret (page does not exist)"">Prototype/Turret</a> <b>turret</b>
<ul><li> <a href=""/index.php?title=Prototype/AmmoTurret&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/AmmoTurret (page does not exist)"">Prototype/AmmoTurret</a> <b>ammo-turret</b></li>
<li> <a href=""/index.php?title=Prototype/ElectricTurret&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/ElectricTurret (page does not exist)"">Prototype/ElectricTurret</a> <b>electric-turret</b></li></ul></li>
<li> <a href=""/Prototype/Unit"" title=""Prototype/Unit"">Prototype/Unit</a> <b>unit</b></li>
<li> <a href=""/index.php?title=Prototype/UnitSpawner&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/UnitSpawner (page does not exist)"">Prototype/UnitSpawner</a> <b>unit-spawner</b></li>
<li> <a href=""/index.php?title=Prototype/Wall&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Wall (page does not exist)"">Prototype/Wall</a> <b>wall</b></li></ul></li>
<li> <a href=""/index.php?title=Prototype/FlyingText&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/FlyingText (page does not exist)"">Prototype/FlyingText</a> <b>flying-text</b></li>
<li> <a href=""/index.php?title=Prototype/Ghost&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Ghost (page does not exist)"">Prototype/Ghost</a> <b>ghost</b></li>
<li> <a href=""/index.php?title=Prototype/ItemEntity&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/ItemEntity (page does not exist)"">Prototype/ItemEntity</a> <b>item-entity</b></li>
<li> <a href=""/index.php?title=Prototype/LandMine&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/LandMine (page does not exist)"">Prototype/LandMine</a> <b>land-mine</b></li>
<li> <a href=""/index.php?title=Prototype/Particle&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Particle (page does not exist)"">Prototype/Particle</a> <b>particle</b></li>
<li> <a href=""/index.php?title=Prototype/Projectile&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Projectile (page does not exist)"">Prototype/Projectile</a> <b>projectile</b></li>
<li> <a href=""/index.php?title=Prototype/RailRemnants&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/RailRemnants (page does not exist)"">Prototype/RailRemnants</a> <b>rail-remnants</b></li>
<li> <a href=""/index.php?title=Prototype/Resource&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Resource (page does not exist)"">Prototype/Resource</a> <b>resource</b></li>
<li> <a href=""/index.php?title=Prototype/Smoke&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Smoke (page does not exist)"">Prototype/Smoke</a> <b>smoke</b></li>
<li> <a href=""/index.php?title=Prototype/Sticker&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Sticker (page does not exist)"">Prototype/Sticker</a> <b>sticker</b></li></ul></li>
<li> <a href=""/Prototype/Item"" class=""mw-redirect"" title=""Prototype/Item"">Prototype/Item</a> <b>item</b>
<ul><li> <a href=""/index.php?title=Prototype/Ammo&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Ammo (page does not exist)"">Prototype/Ammo</a> <b>ammo</b></li>
<li> <a href=""/index.php?title=Prototype/Armor&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Armor (page does not exist)"">Prototype/Armor</a> <b>armor</b></li>
<li> <a href=""/index.php?title=Prototype/Capsule&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Capsule (page does not exist)"">Prototype/Capsule</a> <b>capsule</b></li>
<li> <a href=""/index.php?title=Prototype/Equipment&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Equipment (page does not exist)"">Prototype/Equipment</a> &lt;abstract&gt;
<ul><li> <a href=""/index.php?title=Prototype/NightVisionEquipment&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/NightVisionEquipment (page does not exist)"">Prototype/NightVisionEquipment</a> <b>night-vision-equipment</b></li>
<li> <a href=""/index.php?title=Prototype/EnergyShieldEquipment&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/EnergyShieldEquipment (page does not exist)"">Prototype/EnergyShieldEquipment</a> <b>energy-shield-equipment</b></li>
<li> <a href=""/index.php?title=Prototype/BatteryEquipment&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/BatteryEquipment (page does not exist)"">Prototype/BatteryEquipment</a> <b>battery-equipment</b></li>
<li> <a href=""/index.php?title=Prototype/SolarPanelEquipment&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/SolarPanelEquipment (page does not exist)"">Prototype/SolarPanelEquipment</a> <b>solar-panel-equipment</b></li>
<li> <a href=""/index.php?title=Prototype/GeneratorEquipment&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/GeneratorEquipment (page does not exist)"">Prototype/GeneratorEquipment</a> <b>generator-equipment</b></li>
<li> <a href=""/index.php?title=Prototype/ActiveDefenseEquipment&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/ActiveDefenseEquipment (page does not exist)"">Prototype/ActiveDefenseEquipment</a> <b>active-defense-equipment</b></li>
<li> <a href=""/index.php?title=Prototype/MovementBonusEquipment&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/MovementBonusEquipment (page does not exist)"">Prototype/MovementBonusEquipment</a> <b>movement-bonus-equipment</b></li></ul></li>
<li> <a href=""/index.php?title=Prototype/Gun&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Gun (page does not exist)"">Prototype/Gun</a> <b>gun</b></li>
<li> <a href=""/index.php?title=Prototype/MiningTool&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/MiningTool (page does not exist)"">Prototype/MiningTool</a> <b>mining-tool</b></li>
<li> <a href=""/index.php?title=Prototype/Module&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Module (page does not exist)"">Prototype/Module</a> <b>module</b></li></ul></li>
<li> <a href=""/Prototype/ItemGroup"" title=""Prototype/ItemGroup"">Prototype/ItemGroup</a> <b>item-group</b></li>
<li> <a href=""/index.php?title=Prototype/MapSettings&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/MapSettings (page does not exist)"">Prototype/MapSettings</a> <b>map-settings</b></li>
<li> <a href=""/Prototype/NoiseLayer"" title=""Prototype/NoiseLayer"">Prototype/NoiseLayer</a> <b>noise-layer</b></li>
<li> <a href=""/index.php?title=Prototype/RailCategory&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/RailCategory (page does not exist)"">Prototype/RailCategory</a> <b>rail-category</b></li>
<li> <a href=""/Prototype/Recipe"" title=""Prototype/Recipe"">Prototype/Recipe</a> <b>recipe</b></li>
<li> <a href=""/Prototype/RecipeCategory"" title=""Prototype/RecipeCategory"">Prototype/RecipeCategory</a> <b>recipe-category</b></li>
<li> <a href=""/Prototype/Technology"" class=""mw-redirect"" title=""Prototype/Technology"">Prototype/Technology</a> <b>technology</b></li>
<li> <a href=""/index.php?title=Prototype/Tile&amp;action=edit&amp;redlink=1"" class=""new"" title=""Prototype/Tile (page does not exist)"">Prototype/Tile</a> <b>tile</b></li></ul>

</data>";


        private static void GenerateClasses(JObject data, Dictionary<string, List<PropInfo>> props)
        {
            var xs = new XmlSerializer(typeof(data));
            var rrr = (data)xs.Deserialize(new StringReader(xml));


            List<PrototypeInfo> hierarchy = rrr.Childs.SelectMany(x => x.All).ToList();
            var h = hierarchy.GroupBy(x => x.Text).ToDictionary(x => x.Key, x => x.Count());

            var arr = data.Properties().SelectMany(x => ((JObject)x.Value).Properties().Select(y => (JObject)y.Value)).GroupBy(x => x["type"].Value<string>()).ToDictionary(x => x.Key, x => x.ToArray());

            var except = arr.Keys.Except(h.Keys).ToArray();
            hierarchy.AddRange(except.Select(name => new PrototypeInfo() { Text = name, TextInfo = new TextInfo { Text = $"Prototype/{NormalizeName(name)}" } }));

            foreach (var child in rrr.Childs)
            {
                child.SetParent(null);
            }

            var tmp = hierarchy.GroupBy(x => x.Text).Where(x => x.Count() > 1).ToArray();
            var hh = hierarchy.ToDictionary(x => x.Text);

            foreach (var prototypeInfo in hierarchy.ToArray())
            {
                var typeInfo = _typesAttributes.FirstOrDefault(x => x.Value.Id == prototypeInfo.Text);
                if (typeInfo.Key != null)
                {
                    var properties = typeInfo.Key.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Instance);
                    var existsProps = properties
                        .Select(x => new { PI = x, Attribute = x.Attribute<JsonPropertyAttribute>() })
                        .Where(x => x.Attribute != null)
                        .ToDictionary(x => String.IsNullOrEmpty(x.Attribute.PropertyName) ? x.PI.Name : x.Attribute.PropertyName, x => (uint)1);

                    prototypeInfo.ExistsProperties = existsProps;
                }

                if (props.ContainsKey(prototypeInfo.Text))
                {
                    prototypeInfo.Properties = props[prototypeInfo.Text];
                }
            }


            foreach (var prototypeInfo in hierarchy.ToArray())
            {
                foreach (var property in prototypeInfo.Properties.Select(x => x.Name).ToArray())
                {
                    if (prototypeInfo.ParentsContainsProperty(property) || prototypeInfo.ExistsProperties.ContainsKey(property))
                        prototypeInfo.Properties.RemoveAll(x => x.Name == property);
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine("// ReSharper disable InconsistentNaming");
            sb.AppendLine("// ReSharper disable PartialTypeWithSinglePart");
            sb.AppendLine();
            sb.AppendLine("//*************************************************************************************");
            sb.AppendLine("//");
            sb.AppendLine("//     G E N E R A T E D   C L A S S E S");
            sb.AppendLine("//");
            sb.AppendLine("//*************************************************************************************");
            sb.AppendLine();
            sb.AppendLine("using Newtonsoft.Json;");
            sb.AppendLine();

            sb.AppendLine($"namespace {typeof(Base).Namespace}");
            sb.AppendLine("{");
            sb.AppendLine();


            foreach (var prototypeInfo in hierarchy)
            {
                var key = prototypeInfo.Text;
                var propsByKey = prototypeInfo.Properties;

                propsByKey.RemoveAll(x => x.Name == "-total-");


                var hhh = hh[key];

                var exists = _typesAttributes.Where(x => x.Value.Id == key).Select(x => x.Key).FirstOrDefault();
                var notExists = exists == null;


                var baseType = notExists ? hhh.BaseType : "";


                if (baseType == typeof(TypedNamedBase).Name)
                    propsByKey.RemoveAll(x => x.Name == "name");

                if (baseType == typeof(TypedNamedBase).Name || baseType == typeof(TypedBase).Name)
                    propsByKey.RemoveAll(x => x.Name == "type");

                PropInfo iconPropInfo;
                if (baseType == typeof (TypedNamedBase).Name && (iconPropInfo = propsByKey.FirstOrDefault(x =>x.Name == "icon")) != null)
                {
                    propsByKey.Remove(iconPropInfo);
                    baseType = typeof (TypedNamedIconedBase).Name;
                }

                if (!notExists && propsByKey.Count == 0)
                    continue;


                var className = hhh.ClassName;
                if (!String.IsNullOrEmpty(baseType))
                {
                    if (notExists)
                        sb.AppendLine($"\t[JsonObject(\"{key}\", MemberSerialization = MemberSerialization.OptIn)]");
                    sb.AppendLine($"\tpublic partial class {className} : {baseType}");
                }
                else
                    sb.AppendLine($"\tpublic partial class {className}");

                sb.AppendLine($"\t{{");
                sb.AppendLine();

                foreach (var propInfo in propsByKey)
                {
                    //if (baseType == typeof (TypedNamedBase).Name && propInfo.Name == "name")
                    //    continue;

                    //if ((baseType == typeof (TypedNamedBase).Name || baseType == typeof (TypedBase).Name) && propInfo.Name == "type")
                    //    continue;

                    var propertyName = NormalizeName(propInfo.Name);

                    var propertyType = propInfo.Type;


                    sb.AppendLine($"\t\t[JsonProperty(\"{propInfo.Name}\")]");
                    sb.AppendLine($"\t\tpublic {propertyType} _{propertyName}{{get;set;}}");
                    sb.AppendLine();
                }

                sb.AppendLine($"\t}}");
                sb.AppendLine();
            }

            sb.AppendLine("}");

            var classes = sb.ToString();
        }

        public static string NormalizeName(string name, params char[] separator)
        {
            if (separator.Length == 0)
                separator = new[] { '-', '_' };

            var result = String.Join("", name.Split(separator).Select(x => new string(Char.ToUpper(x[0]), 1) + new string(x.Skip(1).ToArray())));
            return result;
        }

        public Dictionary<string, Mod> Mods { get; set; }

        public JObject StringsByCategory { get; set; }

        public IEnumerable<RecipeCategory> RecipeCategories => Nodes.OfType<RecipeCategory>().ToArray();

        public IEnumerable<Recipe> Recipies => Nodes.OfType<Recipe>().ToArray();
        public IEnumerable<Item> Items => Nodes.OfType<Item>().ToArray();
        public IEnumerable<ItemGroup> ItemGroups => Nodes.OfType<ItemGroup>().ToArray();
        public IEnumerable<Technology> Technologies => Nodes.OfType<Technology>().ToArray();

        public IEnumerable<AssemblingMachine> AssemblingMachines => Nodes.OfType<AssemblingMachine>().ToArray();

        public static string DataJson = Path.Combine(ModsPath, "_data.json");
        public static string LocaleJson = Path.Combine(ModsPath, "_locale.json");
        public static string ModsJson = Path.Combine(ModsPath, "_mods.json");

        public static JObject LoadJson(string fileName)
        {
            if (!File.Exists(fileName))
                return null;

            return JObject.Parse(File.ReadAllText(fileName, Encoding.UTF8));
        }

        public static T LoadJson<T>(string fileName)
        {
            if (!File.Exists(fileName))
                return default(T);

            var result = JsonConvert.DeserializeObject<T>(File.ReadAllText(fileName, Encoding.UTF8));
            return result;
        }

        public string Localization(string category, string value, params object[] args)
        {
            var token = StringsByCategory[category];
            if (token.HasValues)
            {
                var property = ((JObject)token).Property(value);
                try
                {
                    var pattern = property?.Value.Value<string>();
                    if (!string.IsNullOrEmpty(pattern) && pattern.Contains("__"))
                    {
                        foreach (var key in _regexes.Keys)
                        {
                            var regex = _regexes[key];
                            var match = regex.Match(pattern);
                            if (match.Success)
                            {
                                var groupValue = Localization(key, match.Groups[1].Value);
                                pattern = pattern.Replace(match.Groups[0].Value, groupValue);
                            }
                        }

                        if (!string.IsNullOrEmpty(pattern) && pattern.Contains("__") && args.Length > 0)
                        {
                            for (int i = 0; i < args.Length; i++)
                            {
                                pattern = pattern.Replace($"__{i+1}__", $"{{{i}}}");
                            }
                            pattern = string.Format(pattern, args);
                        }
                    }

                    return pattern;
                }
                catch (Exception e)
                {
                    
                }
            }
            return null;
        }

        public static Regex _entityRegex = new Regex(@"__ENTITY__([a-z,A-Z,\-,0-9]*)__");
        public static Regex _itemRegex = new Regex(@"__ITEM__([a-z,A-Z,\-,0-9]*)__");

        static Dictionary<string, Regex> _regexes = new Dictionary<string, Regex>
        {
            {"entity-name", _entityRegex },
            {"item-name", _itemRegex },
        };

        private static Storage _current;
    }

    //class FF : INodesFactory, IEdgesFactory
    //{
    //    public static readonly FF Instance = new FF();

    //    public TNode Create<TNode>(IGraph graph) where TNode : IGraphNode
    //    {
    //        var instance = Activator.CreateInstance<TNode>();
    //        var @base = (instance as Base);
    //        @base?.SetGraph(graph);
    //        return instance;
    //    }

    //    public TNode Create<TNode, TData>(IGraph graph, TData data) where TNode : IGraphNode<TData>
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public TEdge Create<TEdge>(IGraph graph, IGraphNode @from, IGraphNode to) where TEdge : IGraphEdge
    //    {
    //        return (TEdge) Activator.CreateInstance(typeof (TEdge), graph, @from, to);
    //    }

    //    public TEdge Create<TEdge, TData>(IGraph graph, IGraphNode @from, IGraphNode to, TData data) where TEdge : IGraphEdge<TData>
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}

[XmlRoot("data")]
public class data
{
    [XmlArray("ul")]
    [XmlArrayItem(typeof(PrototypeInfo), ElementName = "li")]
    public PrototypeInfo[] Childs { get; set; }
}

[XmlRoot("li")]
public class PrototypeInfo
{
    private string _text;

    [XmlElement(typeof(TextInfo), ElementName = "a")]
    public TextInfo TextInfo { get; set; }

    [XmlElement("b")]
    public string Text
    {
        get { return _text ?? TextInfo.Text; }
        set { _text = value; }
    }

    [XmlArray("ul")]
    [XmlArrayItem(typeof(PrototypeInfo), ElementName = "li")]
    public PrototypeInfo[] Childs { get; set; }

    public override string ToString()
    {
        return $"{TextInfo} [{Text}] (Items={Childs?.Length ?? 0})";
    }

    [XmlIgnore]
    public PrototypeInfo Parent { get; private set; }

    [XmlIgnore]
    public IEnumerable<PrototypeInfo> All => GetAll();

    private IEnumerable<PrototypeInfo> GetAll()
    {
        var first = new[] { this };
        var second = Childs?.SelectMany(x => x.All);
        return second != null ? first.Union(second) : first;
    }

    public void SetParent(PrototypeInfo parent)
    {
        Parent = parent;
        if (Childs == null)
            return;

        foreach (var child in Childs)
        {
            child.SetParent(this);
        }
    }


    [XmlIgnore]
    public string BaseType => GetBaseType();

    [XmlIgnore]
    public string ClassName => Storage.NormalizeName(Text.Split('/').Last());

    [XmlIgnore]
    public List<Storage.PropInfo> Properties { get; set; } = new List<Storage.PropInfo>();

    [XmlIgnore]
    public Dictionary<string, uint> ExistsProperties { get; set; } = new Dictionary<string, uint>();


    private string GetBaseType()
    {
        if (Parent != null)
            return Parent.ClassName;

        var isTyped = Properties.Any(x => x.Name == "type");
        var isNamed = Properties.Any(x => x.Name == "name");

        return isTyped && isNamed ? typeof(TypedNamedBase).Name : isTyped ? typeof(TypedBase).Name : typeof(Base).Name;
    }

    public bool ParentsContainsProperty(string property)
    {
        return Parent?.Properties.Any(x => x.Name == property) == true || Parent?.ExistsProperties.ContainsKey(property) == true || Parent?.ParentsContainsProperty(property) == true;
    }
}


[XmlRoot("a")]
public class TextInfo
{
    [XmlText]
    public string Text { get; set; }

    public override string ToString()
    {
        return Text;
    }
}
