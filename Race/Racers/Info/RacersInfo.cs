using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Linq;
using Product.Utilities;
using UnityEngine;

namespace Product.RacerInfo {
   public class RacersInfo {
      public static RacerInfo[] AllRacers { get; private set; }

      public static void Init() {
         AllRacers = GetAllRacers();
      }

      public static RacerInfo[] GetAllRacers() {
         JArray racers = LoadJSON(Application.dataPath + "/RacersInfo.json");

         var allRacers = new RacerInfo[racers.Count];

         for(int i = 0; i < allRacers.Length; ++i) {
            JToken racer = racers[i];

            int id = racer["id"].ToObject<int>();
            string name = racer["name"].ToString();
            string carName = racer["carName"].ToString();
            string carType = racer["carType"].ToString();
            int raceClass = racer["raceClass"].ToObject<int>();
            string prefabPath = racer["prefabPath"].ToString();
            int strength = racer["strength"].ToObject<int>();

            allRacers[i] = new RacerInfo(id, name, carName, carType, raceClass, prefabPath, strength);
         }
         return allRacers;
      }

      public static JArray LoadJSON(string path) {
         string json = Utils.ReadFile(path);

         return JsonConvert.DeserializeObject<JArray>(json);
      }

      public static string GenerateForBackend() {
         var racers = GetAllRacers();

         string array = "\n// ID, Name, Class, Strength\n\narray(\n";

         for(int i = 0; i < racers.Length; ++i)
            array += "\tarray(" + (i + 1) + ", \"" + racers[i].Name + "\", " + racers[i].Class + ", " + racers[i].Strength + ")" + (i < racers.Length - 1 ? "," : string.Empty) + "\n";

         array += ");";

         return array;
      }

      public static RacerInfo GetRacer(int id) {
         if(AllRacers == null)
            Init();
         return AllRacers.FirstOrDefault(racer => racer.Id == id);
      }

      public static RacerInfo GetRacer(string name) {
         if(AllRacers == null)
            Init();
         return AllRacers.FirstOrDefault(racer => racer.Name == name);
      }
   }
}
