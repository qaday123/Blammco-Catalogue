using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Alexandria.ItemAPI;
using System.IO;
using HarmonyLib;

namespace TF2Stuff
{
    [HarmonyPatch]
    public static class Patches
    {
        public static void EnumeratorSetField(this object obj, string name, object value) => obj.GetType().EnumeratorField(name).SetValue(obj, value);
        public static T EnumeratorGetField<T>(this object obj, string name) => (T)obj.GetType().EnumeratorField(name).GetValue(obj);
        public static FieldInfo EnumeratorField(this MethodBase method, string name) => method.DeclaringType.EnumeratorField(name);
        public static FieldInfo EnumeratorField(this Type tp, string name) => tp.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).First(x => x != null && x.Name != null && (x.Name.Contains($"<{name}>") || x.Name == name));
    }
}
