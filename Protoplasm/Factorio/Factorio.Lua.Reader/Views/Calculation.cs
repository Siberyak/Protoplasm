using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using Protoplasm.Collections;

namespace Factorio.Lua.Reader
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Calculation
    {
        private const string FactorioCalculationFson = "Factorio Calculation | *.fson";
        private const string DefaultExt = "fjson";
        private Storage _storage => Storage.Current;

        private string _path;

        public void Save()
        {
            if(string.IsNullOrEmpty(_path))
                SaveAs();

            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            if(File.Exists(_path))
                File.Delete(_path);

            using (var w = File.CreateText(_path))
            {
                w.Write(json);
                w.Close();
            }

            Changed = false;
        }

        public void SaveAs()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.DefaultExt = DefaultExt;
                dialog.AddExtension = true;
                dialog.Filter = FactorioCalculationFson;
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                _path = dialog.FileName;
                Save();
            }
        }

        public void Reload()
        {
            if(string.IsNullOrEmpty(_path))
                return;

            Load(this);

        }

        public static Calculation Load(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = GetPath();
            }

            if (path == null)
                return null;

            using (var r = File.OpenText(path))
            {
                var calculation = JsonSerializer.CreateDefault().Deserialize<Calculation>(new JsonTextReader(r));
                calculation.Changed = false;
                return calculation;
            }
        }

        private static void Load(Calculation calculation)
        {
            LoadFrom(calculation, calculation._path);
        }

        public static void LoadFrom(Calculation calculation, string path)
        {
            if (calculation == null)
                return;

            calculation.Clear();

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                calculation._path = path = GetPath();

                if (path == null)
                    return;
            }

            using (var r = File.OpenText(path))
            {
                var json = r.ReadToEnd();
                JsonConvert.PopulateObject(json, calculation);
            }

            calculation.Changed = false;

        }

        private static string GetPath()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.AddExtension = true;
                dialog.DefaultExt = DefaultExt;
                dialog.Filter = FactorioCalculationFson;
                dialog.Multiselect = false;

                return dialog.ShowDialog() != DialogResult.OK ? null : dialog.FileName;
            }
        }

        [JsonProperty("id")]
        private Guid ID
        {
            get { return _id; }
            set
            {
                Clear();
                _id = value;
            }
        }

        public void Clear()
        {
            foreach (var craft in _crafts.ToArray())
                craft.OnDeleted();
        }

        [JsonProperty("crafts")]
        public IBindingList<CraftInfo> Crafts => _crafts;

        public IBindingList<Part> Parts => _parts;
        public Part CurrentPart { get; set; }
        public CraftInfo CurrentCraftInfo { get; set; }
        public bool Changed { get; private set; }

        DataList<CraftInfo> _crafts = new DataList<CraftInfo>();
        DataList<Part> _parts = new DataList<Part>();
        private Guid _id = Guid.NewGuid();

        [JsonConstructor]
        public Calculation()
        {
            _crafts.BeforeRemoveItem += BeforeRemoveCrafts;
            _crafts.ListChanged += CraftsListChanged;
        }



        private void BeforeRemoveCrafts(object sender, BeforeRemoveEventArgs e)
        {
            var craftInfo = ((IList<CraftInfo>)sender)[e.Index];

            var allParts = craftInfo.Recipe.AllParts;

            var infos = _parts.Where(x => allParts.Contains(x._part)).Select(x => new {Part = x, Crafts = _crafts.Where(y => y != craftInfo && y.Recipe.AllParts.Contains(x._part)).ToArray()}).ToArray();
            foreach (var info in infos)
            {
                if (info.Crafts.Length == 0)
                    _parts.Remove(info.Part);
            }

        }

        private void CraftsListChanged(object sender, ListChangedEventArgs e)
        {
            Changed = true;

            if(e.ListChangedType == ListChangedType.ItemChanged)
            {
                var craftInfo = ((IList<CraftInfo>)sender)[e.NewIndex];
                switch (e.PropertyDescriptor?.Name)
                {
                    case nameof(craftInfo.Crafter):
                    case nameof(craftInfo.Count):
                        break;
                    case nameof(craftInfo.RequestRecipe):
                        Add(craftInfo.RequestRecipe);
                        craftInfo.RequestRecipe = null;
                        break;
                    default:
                        return;
                }
            }

            var parts = _crafts.SelectMany(x => x.Recipe.AllParts).Distinct().Except(_parts.Select(x => x._part)).ToArray();
            foreach (var part in parts)
            {
                _parts.Add(new Part(part, _crafts));
            }
        }

        public void Add(Recipe recipe)
        {
            //if (_crafts.Any(x => x.Recipe == recipe))
            //    return;
            if(recipe == null)
                return;

            _crafts.Add(new CraftInfo(recipe));
        }
    }
}