using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Protoplasm.Viewer
{
    public class Loader
    {
        public static void Load(string fileName)
        {


            //var json = File.ReadAllText(fileName);
            //var data = JObject.Parse(json);

            //var serializer = JsonSerializer.Create();

            //var heroes = data["heroes"].Select(x => x.Children().First().ToObject<hero>()).ToArray();

            //var attackFormula = heroes.Select(x => x.attackFormula).Distinct().ToArray();
            //var costFormula = heroes.Select(x => x.costFormula).Distinct().ToArray();

            //var upgrades = data["upgrades"].Select(x => x.Children().First().ToObject(typeof(upgrade))).ToArray();


            section<hero> heroes;
            section<upgrade> upgrades;
            section<asset> assets;

            var gd = new GameData()
                .Add("heroes", out heroes)
                .Add("upgrades", out upgrades)
                .Add("assets", out assets)
                ;

            gd.Load(fileName);


            //foreach (var data in heroes)
            //{
            //    var asset = assets[data.assetId];
            //    Console.WriteLine($"{data.name} -> {asset.folder}\\{asset.fileName}");
            //}

            var upgradeFunctions = upgrades.Select(x => x.upgradeFunction).Distinct()
                .Select(x => $"case \"{x}\":"+Environment.NewLine + "return $\"\";")
                .ToArray();
            
            //Console.WriteLine(string.Join(Environment.NewLine, upgradeFunctions));

            foreach (var data in upgrades)
            {
                data.Hero = heroes[data.heroId];
                data.Hero.AddUpgrade(data);

                var @params = data.upgradeParams.Split(',').Select(x => x.Trim()).ToArray();
                data.amount = double.Parse(@params.Last(), NumberFormatInfo.InvariantInfo);
                if (@params.Length > 1)
                    data.UpgradingHero = heroes[int.Parse(@params[0])];

            }


            var sb = new StringBuilder();
            foreach (var hero in heroes)
            {
                sb.Append(hero.name);
                sb.Append("\t");
                sb.Append(string.Join("\t", hero.Upgrades));
                sb.AppendLine();
            }

            var result = sb.ToString();
        }
    }


    public class GameData
    {
        Dictionary<string, section> _sections = new Dictionary<string, section>();

        public GameData Add<T>(string name, out section<T> section) 
            where T : IDataItem
        {
            section = new section<T>(name);
            _sections.Add(section.Name, section);
            return this;
        }

        public void Load(string fileName)
        {
            var json = File.ReadAllText(fileName);
            var data = JObject.Parse(json);

            foreach (var section in _sections.Values)
            {
                section.Load(data);
            }
        }
    }

    public class section<T> : section, IEnumerable<T>
        where T : IDataItem
    {
        private T[] _data = new T[0];

        public section(string name) : base(name)
        {
        }

        public override Type DataType => typeof (T);

        public override void Load(JObject root)
        {
            _data = root[Name].Select(x => x.Children().First().ToObject<T>()).Where(x => x.Alive).ToArray();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)_data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int id] => _data.FirstOrDefault(x => x.ID == id);
    }

    public abstract class section
    {
        public abstract Type DataType { get; }
        public string Name;

        protected section(string name)
        {
            Name = name;
        }

        public abstract void Load(JObject root);
    }

    public interface IDataItem
    {
        int ID { get; }
        bool Alive { get; }
    }

    public class hero : IDataItem
    {
        public override string ToString()
        {
            return name;
        }

        public string attackFormula { get; set; }
        public string costFormula { get; set; }
        public string specialSkill { get; set; }
        public string specialSkillDescription { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public int assetId { get; set; }
        public float baseAttack { get; set; }
        public float baseClickDamage { get; set; }
        public float baseGoldPerSecond { get; set; }
        public string baseCost { get; set; }
        public string _live { get; set; }
        public int id { get; set; }
        public string clickDamageFormula { get; set; }
        public string goldPerSecondFormula { get; set; }
        public int ID => id;
        public bool Alive => _live == "1";

        SortedList<int, upgrade> _upgrades = new SortedList<int, upgrade>();
        public void AddUpgrade(upgrade upgrade)
        {
            _upgrades.Add(upgrade.attribute, upgrade);
        }
        public IEnumerable<upgrade> Upgrades => _upgrades.Values;
    }

    public class upgrade : IDataItem
    {
        public override string ToString()
        {
            var value = amount.ToString(amount < 1 ? "N" : "N0", NumberFormatInfo.InvariantInfo) + "%";

            switch (upgradeFunction)
            {
                case "upgradeClickPercent":
                    return $"+{value} click";
                case "upgradeGetSkill":
                    return $"+ [{name}]";
                case "upgradeHeroPercent":
                    return $"+{value}" + (Hero != UpgradingHero ? $" {UpgradingHero.name} DPS" : "");
                case "upgradeEveryonePercent":
                    return $"all DPS +{value}";
                case "upgradeCriticalChance":
                    return $"+ {amount} crit chance";
                case "upgradeCriticalDamage":
                    return $"+ {amount} crit damage";
                case "upgradeGoldFoundPercent":
                    return $"+ {value} gold found";
                case "upgradeClickDpsPercent":
                    return $"+{value} click DPS";
                case "finalUpgrade":
                    return $"FINAL";
                default:
                    return $"{attribute}. {name}: {heroLevelRequired}";
            }

        }

        public int heroLevelRequired { get; set; }
        public int isPercentage { get; set; }
        public int displayOrder { get; set; }
        public string _live { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public double amount { get; set; }
        public int iconId { get; set; }
        public int heroId { get; set; }
        public string upgradeFunction { get; set; }
        public string upgradeParams { get; set; }
        public int id { get; set; }
        public int attribute { get; set; }
        public int upgradeRequired { get; set; }
        public int ID => id;
        public bool Alive => _live == "1";

        public hero Hero;
        public hero UpgradingHero;
    }

    public class asset : IDataItem
    {
        public override string ToString()
        {
            return $"{name}: {folder}\\{fileName}.{fileExtension}";
        }

        public string folder { get; set; }
        public int pivotY { get; set; }
        public string fileExtension { get; set; }
        public int pivotX { get; set; }
        public string name { get; set; }
        public string fileName { get; set; }
        public int id { get; set; }
        public int version { get; set; }
        public int ID => id;
        public bool Alive => true;
    }

}