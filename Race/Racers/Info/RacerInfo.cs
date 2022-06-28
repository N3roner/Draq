namespace Product.RacerInfo {
   public class RacerInfo {
      public int Id { get; private set; }
      public string Name { get; private set; }
      public string CarName { get; private set; }
      public string CarType { get; private set; }
      public int Class { get; private set; }
      public string PrefabPath { get; private set; }
      public int Strength { get; private set; }

      public RacerInfo(int id, string name, string carName, string carType, int raceClass, string prefabPath, int strength) {
         Id = id;
         Name = name;
         CarName = carName;
         CarType = carType;
         Class = raceClass;
         PrefabPath = prefabPath;
         Strength = strength;
      }
   }
}