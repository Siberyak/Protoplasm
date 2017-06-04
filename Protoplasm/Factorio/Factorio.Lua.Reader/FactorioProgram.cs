using System;
using System.Collections;
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
    internal class FactorioProgram
    {

        [STAThread]
        private static void Main(string[] args)
        {

            var flag = false;
            if (flag)
            {
                Load();
                return;
            }



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
            //UserLookAndFeel.Default.SetSkinStyle("Visual Studio 2013 Blue");

            var skin = SkinManager.Default.GetSkin(SkinProductId.Docking, UserLookAndFeel.Default);
            var tmp = skin.Elements.OfType<DictionaryEntry>().Select(x => x.Key).ToArray();

            if(tmp.Length == 0)
            { }

            //Application.Run(new Form1());

            var storageExist = false;
            ViewsExtender.ShowProgress(() => storageExist = Storage.Current != null);

            if (!storageExist)
            {
                return;
            }
            else
                Application.Run(new RibbonForm2());
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