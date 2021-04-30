using System.Collections.Generic;
using System.IO;
using TShockAPI;
using Newtonsoft.Json;

namespace HalfSuperadmin
{
    public static class Config
    {
        public static Dictionary<string, List<string>> List = new Dictionary<string, List<string>>();

        public static string Path = TShock.SavePath + "\\Half-Superadmin\\ProtectedGroups.json";

        public static void Load()
        {
            var list = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(File.ReadAllText(Path));

            foreach (var x in list)
                List.Add(x.Key, new List<string>(x.Value)); // зачем так делать? JSON не умеет работать со списками
        }

        public static void Save()
        {
            var list = new Dictionary<string, string[]>();

            foreach (var x in List)
                list.Add(x.Key, x.Value.ToArray()); // та же самая причина хотя может я зря переживаю 

            File.WriteAllText(Path, JsonConvert.SerializeObject(List, Formatting.Indented));
        }
    }
}
