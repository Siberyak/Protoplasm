﻿using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using Factorio.Lua.Reader;
using Factorio.Lua.Reader.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Factorio.Lua.Reader
{
    internal class Program
    {

        [STAThread]
        private static void Main(string[] args)
        {

            var str = "__ENTITY__electrolyser-2__";

            var ism = Storage._entityRegex.IsMatch(str);
            var matchCollection = Storage._entityRegex.Matches(str);
            var match = Storage._entityRegex.Match(str);

            var g1 = match.Groups[0];
            var g2 = match.Groups[1];
            //Load();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            SkinManager.EnableFormSkins();
            DevExpress.UserSkins.BonusSkins.Register();

            var skins = SkinManager.Default.Skins.OfType<SkinContainer>().Select(x => x.SkinName).ToArray();

            // DevExpress Style
            // DevExpress Dark Style
            //Valentine
            //Pumpkin
            //Blueprint
            //Whiteprint
            //Black
            //"Dark Side"
            UserLookAndFeel.Default.SetSkinStyle("Visual Studio 2013 Blue");


            Application.Run(new Form1());


            //var current = Environment.CurrentDirectory;
            //Environment.CurrentDirectory = @"C:\Games\Steam\steamapps\common\Factorio\data\core\lualib\";
            //var lua = new Lua();
            //lua["container_module_loader"] = null;
            //var tmp = Directory.GetFiles(Environment.CurrentDirectory, "*.lua").Select(x => lua.DoFile(x)).ToArray();

            //lua.DoFile(@".\data.lua");
            //Environment.CurrentDirectory = @"C:\Games\Steam\steamapps\common\Factorio\data\base\";
            //lua.DoFile(@".\data.lua");
            //var table = lua.GetTable("data");
        }

        public static void Load()
        {
            //I changed the name of the variable, so this copies the value over for people who are upgrading their Foreman version
            if (Settings.Default.FactorioPath == "" && Settings.Default.FactorioDataPath != "")
            {
                Settings.Default["FactorioPath"] = Path.GetDirectoryName(Settings.Default.FactorioDataPath);
                Settings.Default["FactorioDataPath"] = "";
            }

            if (!Directory.Exists(Settings.Default.FactorioPath))
            {
                foreach (var defaultPath in new[]
                {
                    Path.Combine(Environment.GetEnvironmentVariable("PROGRAMFILES(X86)"), "Factorio"),
                    Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432"), "Factorio"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Applications", "factorio.app", "Contents")
                }) //Not actually tested on a Mac
                {
                    if (Directory.Exists(defaultPath))
                    {
                        Settings.Default["FactorioPath"] = defaultPath;
                        Settings.Default.Save();
                        break;
                    }
                }
            }

            if (!Directory.Exists(Settings.Default.FactorioPath))
            {
                using (var dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Settings.Default["FactorioPath"] = dialog.SelectedPath;
                        Settings.Default.Save();
                    }
                    else
                    {
                        //Close();
                        //Dispose();
                        return;
                    }
                }
            }

            if (!Directory.Exists(Settings.Default.FactorioModPath))
            {
                if (Directory.Exists(Path.Combine(Settings.Default.FactorioPath, "mods")))
                {
                    Settings.Default["FactorioModPath"] = Path.Combine(Settings.Default.FactorioPath, "mods");
                }
                else if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "factorio", "mods")))
                {
                    Settings.Default["FactorioModPath"] = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "factorio", "mods");
                }
            }

            if (Settings.Default.EnabledMods == null) Settings.Default.EnabledMods = new StringCollection();
            if (Settings.Default.EnabledAssemblers == null) Settings.Default.EnabledAssemblers = new StringCollection();
            if (Settings.Default.EnabledMiners == null) Settings.Default.EnabledMiners = new StringCollection();
            if (Settings.Default.EnabledModules == null) Settings.Default.EnabledModules = new StringCollection();


            DataCache.LoadAllData(null);
        }
    }
}