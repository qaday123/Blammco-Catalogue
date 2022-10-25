using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using System.Reflection;

namespace ExampleMod
{
    public class AudioResourceLoader
    {

        public static void LoadFromFolder(string modName)
        {
            ResourceLoaderSoundbanks LoaderSoundbanks = new ResourceLoaderSoundbanks();
            LoaderSoundbanks.AutoloadFromPath(ExampleMod.Module.instance.FolderPath(), modName);
        }
        public static void LoadFromAssembly(string bankName, string modName)
        {
            ResourceLoaderSoundbanks LoaderSoundbanks = new ResourceLoaderSoundbanks();
            LoaderSoundbanks.AutoloadFromAssembly(Assembly.GetExecutingAssembly(), bankName, modName);
        }
    }
}
