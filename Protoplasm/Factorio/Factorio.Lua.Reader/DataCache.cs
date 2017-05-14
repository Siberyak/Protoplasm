using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Factorio.Lua.Reader.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLua;

namespace Factorio.Lua.Reader
{
    public class DataCache
    {
        private const float defaultRecipeTime = 0.5f;

        public static List<Mod> Mods = new List<Mod>();
        public static List<Language> Languages = new List<Language>();

        public static Dictionary<string, Exception> failedFiles = new Dictionary<string, Exception>();
        public static Dictionary<string, Exception> failedPathDirectories = new Dictionary<string, Exception>();
        private static readonly Dictionary<Bitmap, Color> colourCache = new Dictionary<Bitmap, Color>();
        public static Bitmap UnknownIcon;
        public static Dictionary<string, Dictionary<string, string>> LocaleFiles = new Dictionary<string, Dictionary<string, string>>();
        private static string DataPath => Path.Combine(Settings.Default.FactorioPath, "data");
        private static string TempModPath = GetTempModPath();
        private static string[] _filenames = new[]
                {
                    "settings.lua",
                    "settings-updates.lua",
                    "settings-final-fixes.lua",
                    "data.lua",
                    "data-updates.lua",
                    "data-final-fixes.lua"
                };

        private static string GetTempModPath()
        {

            var path = Storage.ModsPath; //Path.Combine(Path.GetTempPath(), "_Factorio_mods");

            string suffix = $"{DateTime.UtcNow.Ticks}";

            if(Directory.Exists(path))
                Directory.Move(path, $"{path}.{suffix}");

            //foreach (var directory in Directory.GetDirectories(Path.GetTempPath(), "_Factorio_mods"))
            //{
            //    try
            //    {
            //        Directory.Delete(directory, true);

            //    }
            //    catch (Exception e)
            //    {
                    
            //    }
            //}

            //var path = Path.Combine(Path.GetTempPath(), "_Factorio_mods");
            Directory.CreateDirectory(path);
            return path;
        }



        public static void LoadAllData(List<string> enabledMods)
        {
            Clear();

            using (var lua = new NLua.Lua())
            {
                FindAllMods(enabledMods);

                _AddLuaPackagePath(lua, Path.Combine(DataPath, "core", "lualib")); //Core lua functions
                var basePackagePath = lua["package.path"] as string;

                
                if(!LoadLibFile("dataloader.lua", lua))
                    return;

                lua.DoString(@"
					function module(modname,...)
					end
					require ""util""
					util = {}
					util.table = {}
					util.table.deepcopy = table.deepcopy
					util.multiplystripes = multiplystripes
util.by_pixel = by_pixel

settings = {}

function log(...)
end

");

                if (!LoadLibFile("defines.lua", lua))
                    return;


                
                foreach (var filename in _filenames)
                {
                    if (filename == "data.lua")
                    {
                        var dt = (LuaTable) lua["data"];
                        var raw = (LuaTable) dt["raw"];

                        var bS = raw["bool-setting"] as LuaTable;
                        var iS = raw["int-setting"] as LuaTable;
                        var dS = raw["double-setting"] as LuaTable;
                        var sS = raw["string-setting"] as LuaTable;


                        var sb = new List<string>();

                        ProcessSettings(bS, sb);
                        ProcessSettings(iS, sb);
                        ProcessSettings(dS, sb);
                        ProcessSettings(sS, sb);

                        var cmd = $@"
settings.startup = 
{{
{string.Join("," + Environment.NewLine, sb)}
}}
";
                        lua.DoString(cmd);

                    }

                    foreach (var mod in Mods.Where(m => m.Enabled))
                    {
                        //Mods use relative paths, but if more than one mod is in package.path at once this can be ambiguous
                        lua["package.path"] = basePackagePath;

                        var tmp = AddLuaPackagePath(lua, mod.dir);

                        //Because many mods use the same path to refer to different files, we need to clear the 'loaded' table so Lua doesn't think they're already loaded
                        lua.DoString(@"
							for k, v in pairs(package.loaded) do
								package.loaded[k] = false
							end");

                        var dataFile = Path.Combine(mod.dir, filename);
                        if (File.Exists(dataFile))
                        {
                            try
                            {
                                lua.DoFile(dataFile);
                            }
                            catch (Exception e)
                            {
                                failedFiles[dataFile] = e;
                            }
                        }
                    }
                }

                //------------------------------------------------------------------------------------------
                // Lua files have all been executed, now it's time to extract their data from the lua engine
                //------------------------------------------------------------------------------------------

                

                var datatable = lua.GetTable("data");

                var table = lua.GetTable("data.raw");

                var keys = table.Keys.OfType<string>().OrderBy(x => x).ToArray();

                LoadAllLanguages();
                LoadLocaleFiles();


                //ShowTable(table);

                var result = GetValue(table);

                var groups = result["item-group"].ToObject<JObject>();
                var props = groups.Properties();

                var propValues = props.Select(x => x.Value.Path).ToArray();

                var subgroups = result["item-subgroup"];


                //var tmp = groups.Select(x => x).ToArray();


                SaveJson(result, Storage.DataJson);
                SaveJson(JObject.FromObject(LocaleFiles), Storage.LocaleJson);
                Dictionary<string, Mod> dictionary = Mods.ToDictionary(x => x.Name);
                SaveJson(JObject.FromObject(dictionary), Storage.ModsJson);

                //foreach (String type in new List<String> { "item", "fluid", "capsule", "module", "ammo", "gun", "armor", "blueprint", "deconstruction-item", "mining-tool", "repair-tool", "tool" })
                //{
                //    InterpretItems(lua, type);
                //}

                //LuaTable recipeTable = lua.GetTable("data.raw")["recipe"] as LuaTable;
                //if (recipeTable != null)
                //{
                //    var recipeEnumerator = recipeTable.GetEnumerator();
                //    while (recipeEnumerator.MoveNext())
                //    {
                //        InterpretLuaRecipe(recipeEnumerator.Key as String, recipeEnumerator.Value as LuaTable);
                //    }
                //}

                //LuaTable assemblerTable = lua.GetTable("data.raw")["assembling-machine"] as LuaTable;
                //if (assemblerTable != null)
                //{
                //    var assemblerEnumerator = assemblerTable.GetEnumerator();
                //    while (assemblerEnumerator.MoveNext())
                //    {
                //        InterpretAssemblingMachine(assemblerEnumerator.Key as String, assemblerEnumerator.Value as LuaTable);
                //    }
                //}

                //LuaTable furnaceTable = lua.GetTable("data.raw")["furnace"] as LuaTable;
                //if (furnaceTable != null)
                //{
                //    var furnaceEnumerator = furnaceTable.GetEnumerator();
                //    while (furnaceEnumerator.MoveNext())
                //    {
                //        InterpretFurnace(furnaceEnumerator.Key as String, furnaceEnumerator.Value as LuaTable);
                //    }
                //}

                //LuaTable minerTable = lua.GetTable("data.raw")["mining-drill"] as LuaTable;
                //if (minerTable != null)
                //{
                //    var minerEnumerator = minerTable.GetEnumerator();
                //    while (minerEnumerator.MoveNext())
                //    {
                //        InterpretMiner(minerEnumerator.Key as String, minerEnumerator.Value as LuaTable);
                //    }
                //}

                //LuaTable resourceTable = lua.GetTable("data.raw")["resource"] as LuaTable;
                //if (resourceTable != null)
                //{
                //    var resourceEnumerator = resourceTable.GetEnumerator();
                //    while (resourceEnumerator.MoveNext())
                //    {
                //        InterpretResource(resourceEnumerator.Key as String, resourceEnumerator.Value as LuaTable);
                //    }
                //}

                //LuaTable moduleTable = lua.GetTable("data.raw")["module"] as LuaTable;
                //if (moduleTable != null)
                //{
                //    foreach (String moduleName in moduleTable.Keys)
                //    {
                //        InterpretModule(moduleName, moduleTable[moduleName] as LuaTable);
                //    }
                //}

                //UnknownIcon = LoadImage("UnknownIcon.png");
            }

            ReportErrors();
        }

        private static void ProcessSettings(LuaTable table, List<string> sb)
        {
            if (table == null)
                return;

            foreach (var key in table.Keys)
            {

                var setting = table[key] as LuaTable;
                if(setting == null)
                    continue;

                var defVal = setting["default_value"];
                if (defVal is string)
                    defVal = $"\"{defVal}\"";
                else if(defVal is bool)
                    defVal = defVal.ToString().ToLower();
                else if (defVal is double)
                    defVal = ((double)defVal).ToString(NumberFormatInfo.InvariantInfo);

                var type = setting["setting_type"].ToString();
                if(type != "startup")
                    continue;

                sb.Add($"[\"{key}\"]={{value={defVal}}}");
            }
        }
        private static void ProcessBoolSettings(LuaTable table, StringBuilder sb)
        {
            
        }
        private static void ProcessDoubleSettings(LuaTable table, StringBuilder sb)
        {
            if (table == null)
                return;
        }
        private static void ProcessIntSettings(LuaTable table, StringBuilder sb)
        {
            if (table == null)
                return;
        }
        private static void ProcessStringSettings(LuaTable table, StringBuilder sb)
        {
            if (table == null)
                return;
        }

        private static bool LoadLibFile(string fileName, NLua.Lua lua)
        {
            var dataloaderFile = Path.Combine(DataPath, "core", "lualib", fileName);
            try
            {
                lua.DoFile(dataloaderFile);
            }
            catch (Exception e)
            {
                failedFiles[dataloaderFile] = e;
                ErrorLogging.LogLine($"Error loading {fileName}. This file is required to load any values from the prototypes. Message: '{e.Message}'");
                return false;
            }
            return true;
        }

        private static void SaveJson(JObject result, string fileName)
        {
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
                File.AppendAllText(fileName, result.ToString(), Encoding.UTF8);
            }

        }

        private static JObject GetValue(object value)
        {
            var writer = new JTokenWriter();
            writer.Write(value);
            return (JObject)writer.Token;
        }

        private static void ShowTable(LuaTable table, string prefix = null)
        {
            Console.WriteLine(prefix + "{");

            var isArray = table.Keys.Cast<object>().All(x => x is double);

            foreach (var key in table.Keys)
            {
                var value = table[key];
                var keyType = key.GetType();
                var valueType = value.GetType();

                if (!isArray)
                    Console.Write(prefix + key + " = ");

                if (value is LuaTable)
                {
                    if (!isArray)
                        Console.WriteLine();

                    ShowTable((LuaTable)value, prefix + "  ");
                    Console.WriteLine(prefix + ",");
                }
                else
                    Console.WriteLine((isArray ? prefix + "  " : "") + value + ",");
            }
            Console.WriteLine(prefix + "}");
        }

        public static void LoadLocaleFiles(string locale = "en")
        {
            foreach (var mod in Mods.Where(m => m.Enabled))
            {
                var localeDir = Path.Combine(mod.dir, "locale", locale);
                if (Directory.Exists(localeDir))
                {
                    foreach (var file in Directory.GetFiles(localeDir, "*.cfg"))
                    {
                        try
                        {
                            using (var fStream = new StreamReader(file))
                            {
                                var currentIniSection = "none";

                                while (!fStream.EndOfStream)
                                {
                                    var line = fStream.ReadLine();
                                    if (line.StartsWith("[") && line.EndsWith("]"))
                                    {
                                        currentIniSection = line.Trim('[', ']');
                                    }
                                    else
                                    {
                                        if (!LocaleFiles.ContainsKey(currentIniSection))
                                        {
                                            LocaleFiles.Add(currentIniSection, new Dictionary<string, string>());
                                        }
                                        var split = line.Split('=');
                                        if (split.Count() == 2)
                                        {
                                            LocaleFiles[currentIniSection][split[0].Trim()] = split[1].Trim();
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            failedFiles[file] = e;
                        }
                    }
                }
            }
        }

        private static Bitmap LoadImage(string fileName)
        {
            string fullPath;
            if (File.Exists(fileName))
            {
                fullPath = fileName;
            }
            else
            {
                var splitPath = fileName.Split('/');
                splitPath[0] = splitPath[0].Trim('_');
                fullPath = Mods.FirstOrDefault(m => m.Name == splitPath[0]).dir;

                if (!string.IsNullOrEmpty(fullPath))
                {
                    for (var i = 1; i < splitPath.Count(); i++) //Skip the first split section because it's the mod name, not a directory
                    {
                        fullPath = Path.Combine(fullPath, splitPath[i]);
                    }
                }
            }

            try
            {
                using (var image = new Bitmap(fullPath)) //If you don't do this, the file is locked for the lifetime of the bitmap
                {
                    return new Bitmap(image);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Color IconAverageColour(Bitmap icon)
        {
            if (icon == null)
            {
                return Color.LightGray;
            }

            Color result;
            if (colourCache.ContainsKey(icon))
            {
                result = colourCache[icon];
            }
            else
            {
                using (var pixel = new Bitmap(1, 1))
                using (var g = Graphics.FromImage(pixel))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(icon, new Rectangle(0, 0, 1, 1)); //Scale the icon down to a 1-pixel image, which does the averaging for us
                    result = pixel.GetPixel(0, 0);
                }
                //Set alpha to 255, also lighten the colours to make them more pastel-y
                result = Color.FromArgb(255, result.R + (255 - result.R) / 2, result.G + (255 - result.G) / 2, result.B + (255 - result.B) / 2);
                colourCache.Add(icon, result);
            }

            return result;
        }


        private static void LoadAllLanguages()
        {
            var dirList = Directory.EnumerateDirectories(Path.Combine(Mods.First(m => m.Name == "core").dir, "locale"));

            foreach (var dir in dirList)
            {
                var newLanguage = new Language();
                newLanguage.Name = Path.GetFileName(dir);
                try
                {
                    var infoJson = File.ReadAllText(Path.Combine(dir, "info.json"));
                    newLanguage.LocalName = (string)JObject.Parse(infoJson)["language-name"];
                }
                catch
                {
                }
                Languages.Add(newLanguage);
            }
        }

        public static void Clear()
        {
            colourCache.Clear();
            LocaleFiles.Clear();
            failedFiles.Clear();
            failedPathDirectories.Clear();

            Mods.Clear();
            //Items.Clear();
            //Recipes.Clear();
            //Assemblers.Clear();
            //Miners.Clear();
            //Resources.Clear();
            //Modules.Clear();
            //Inserters.Clear();
            Languages.Clear();
        }

        private static float ReadLuaFloat(LuaTable table, string key, bool canBeMissing = false, float defaultValue = 0f)
        {
            if (table[key] == null)
            {
                if (canBeMissing)
                {
                    return defaultValue;
                }
                throw new MissingPrototypeValueException(table, key, "Key is missing");
            }

            try
            {
                return Convert.ToSingle(table[key]);
            }
            catch (FormatException)
            {
                throw new MissingPrototypeValueException(table, key, string.Format("Expected a float, but the value ('{0}') isn't one", table[key]));
            }
        }

        private static int ReadLuaInt(LuaTable table, string key, bool canBeMissing = false, int defaultValue = 0)
        {
            if (table[key] == null)
            {
                if (canBeMissing)
                {
                    return defaultValue;
                }
                throw new MissingPrototypeValueException(table, key, "Key is missing");
            }

            try
            {
                return Convert.ToInt32(table[key]);
            }
            catch (FormatException)
            {
                throw new MissingPrototypeValueException(table, key, string.Format("Expected an Int32, but the value ('{0}') isn't one", table[key]));
            }
        }

        private static string ReadLuaString(LuaTable table, string key, bool canBeMissing = false, string defaultValue = null)
        {
            if (table[key] == null)
            {
                if (canBeMissing)
                {
                    return defaultValue;
                }
                throw new MissingPrototypeValueException(table, key, "Key is missing");
            }

            return Convert.ToString(table[key]);
        }

        private static LuaTable ReadLuaLuaTable(LuaTable table, string key, bool canBeMissing = false)
        {
            if (table[key] == null)
            {
                if (canBeMissing)
                {
                    return null;
                }
                throw new MissingPrototypeValueException(table, key, "Key is missing");
            }

            try
            {
                return table[key] as LuaTable;
            }
            catch (Exception)
            {
                throw new MissingPrototypeValueException(table, key, "Could not convert key to LuaTable");
            }
        }

        private static void ReportErrors()
        {
            if (failedPathDirectories.Any())
            {
                ErrorLogging.LogLine("There were errors setting the lua path variable for the following directories:");
                foreach (var dir in failedPathDirectories.Keys)
                {
                    ErrorLogging.LogLine(string.Format("{0} ({1})", dir, failedPathDirectories[dir].Message));
                }
            }

            if (failedFiles.Any())
            {
                ErrorLogging.LogLine("The following files could not be loaded due to errors:");
                foreach (var file in failedFiles.Keys)
                {
                    ErrorLogging.LogLine($"{file} ({failedFiles[file].Message})");
                }
            }
        }

        private static string AddLuaPackagePath(NLua.Lua lua, string dir)
        {
            if(Directory.GetFiles(dir, "*.lua").Length > 0)
                _AddLuaPackagePath(lua, dir);

            foreach (var directory in Directory.GetDirectories(dir))
            {
                AddLuaPackagePath(lua, directory);
            }
            return (lua["package.path"] as string).Replace(";", Environment.NewLine);
        }

        private static void _AddLuaPackagePath(NLua.Lua lua, string dir)
        {
            try
            {
                var luaCommand = $"package.path = package.path .. ';{dir}{Path.DirectorySeparatorChar}?.lua'";
                luaCommand = luaCommand.Replace("\\", "\\\\");
                lua.DoString(luaCommand);
            }
            catch (Exception e)
            {
                failedPathDirectories[dir] = e;
            }
        }

        private static void FindAllMods(List<string> enabledMods) //Vanilla game counts as a mod too.
        {


            if (Directory.Exists(DataPath))
            {
                foreach (var dir in Directory.EnumerateDirectories(DataPath))
                {
                    ReadModInfoFile(dir);
                }
            }
            if (Directory.Exists(Settings.Default.FactorioModPath))
            {
                foreach (var dir in Directory.EnumerateDirectories(Settings.Default.FactorioModPath))
                {
                    ReadModInfoFile(CopyToTempDir(dir));
                }

                foreach (var zipFile in Directory.EnumerateFiles(Settings.Default.FactorioModPath, "*.zip"))
                {
                    ReadModInfoFile(UnzipToTempDir(zipFile));
                }
            }

            var enabledModsFromFile = new Dictionary<string, bool>();

            var modListFile = Path.Combine(Settings.Default.FactorioModPath, "mod-list.json");
            if (File.Exists(modListFile))
            {
                var json = File.ReadAllText(modListFile);
                dynamic parsedJson = JsonConvert.DeserializeObject(json);
                foreach (var mod in parsedJson.mods)
                {
                    string name = mod.name;
                    var enabled = (bool)mod.enabled;
                    enabledModsFromFile.Add(name, enabled);
                }
            }

            if (enabledMods != null)
            {
                foreach (var mod in Mods)
                {
                    mod.Enabled = enabledMods.Contains(mod.Name);
                }
            }
            else
            {
                var splitModStrings = new Dictionary<string, string>();
                foreach (var s in Settings.Default.EnabledMods)
                {
                    var split = s.Split('|');
                    splitModStrings.Add(split[0], split[1]);
                }
                foreach (var mod in Mods)
                {
                    if (splitModStrings.ContainsKey(mod.Name))
                    {
                        mod.Enabled = splitModStrings[mod.Name] == "True";
                    }
                    else if (enabledModsFromFile.ContainsKey(mod.Name))
                    {
                        mod.Enabled = enabledModsFromFile[mod.Name];
                    }
                    else
                    {
                        mod.Enabled = true;
                    }
                }
            }

            var modGraph = new DependencyGraph(Mods);
            modGraph.DisableUnsatisfiedMods();
            Mods = modGraph.SortMods();
        }


        private static string CopyToTempDir(string dir)
        {
            var fileName = Path.GetFileName(dir);
            var destination = Path.Combine(TempModPath, fileName);
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(dir, destination);
            return destination;
        }

        private static string UnzipToTempDir(string zipFile)
        {
            using (var arch = ZipFile.OpenRead(zipFile))
            {
                arch.ExtractToDirectory(TempModPath);
                var folder = arch.Entries.First().FullName.Split('/')[0];
                var path = Path.Combine(TempModPath, folder);
                return path;
            }
        }

        private static void ReadModInfoZip(string zipFile)
        {
        }

        private static void ReadModInfoFile(string dir)
        {
            try
            {
                if (!File.Exists(Path.Combine(dir, "info.json")))
                {
                    return;
                }
                var json = File.ReadAllText(Path.Combine(dir, "info.json"));
                ReadModInfo(json, dir);
            }
            catch (Exception)
            {
                ErrorLogging.LogLine(string.Format("The mod at '{0}' has an invalid info.json file", dir));
            }
        }

        private static void ReadModInfo(string json, string dir)
        {
            var newMod = JsonConvert.DeserializeObject<Mod>(json);
            newMod.dir = dir;

            if (!Version.TryParse(newMod.version, out newMod.parsedVersion))
            {
                newMod.parsedVersion = new Version(0, 0, 0, 0);
            }
            ParseModDependencies(newMod);

            Mods.Add(newMod);
        }

        private static void ParseModDependencies(Mod mod)
        {
            if (mod.Name == "base")
            {
                mod.dependencies.Add("core");
            }

            foreach (var depString in mod.dependencies)
            {
                var token = 0;

                var newDependency = new ModDependency();

                var split = depString.Split(' ');

                if (split[token] == "?")
                {
                    newDependency.Optional = true;
                    token++;
                }

                newDependency.ModName = split[token];
                token++;

                if (split.Count() == token + 2)
                {
                    switch (split[token])
                    {
                        case "=":
                            newDependency.VersionType = DependencyType.EqualTo;
                            break;
                        case ">":
                            newDependency.VersionType = DependencyType.GreaterThan;
                            break;
                        case ">=":
                            newDependency.VersionType = DependencyType.GreaterThanOrEqual;
                            break;
                    }
                    token++;

                    newDependency.Version = Version.Parse(split[token]);
                    token++;
                }

                mod.parsedDependencies.Add(newDependency);
            }
        }

        private class MissingPrototypeValueException : Exception
        {
            public string Key;
            public LuaTable Table;

            public MissingPrototypeValueException(LuaTable table, string key, string message = "")
                : base(message)
            {
                Table = table;
                Key = key;
            }
        }
    }
}