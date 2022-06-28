using UnityEngine;

namespace Product {
   #region GlobalObjects description
   /// <summary> 
   //  GlobalObjects
   //
   /// This class contains references for all globally used objects.
   /// </summary>
   #endregion
   public class GlobalObjects {
      public static GameObject UI { get; private set; }
      public static GameObject Race { get; private set; }
      public static GameObject Global { get; private set; }
      public static GameObject SocketIO { get; private set; }
      public static GameObject Environment { get; private set; }
      public static GameObject Advertisement { get; private set; }
      
      /** GlobalObjects initialization. */
      public static void Init() {
         FindAllGlobalObjects();
      }
      
      /** Finds all Global objects and references them. */
      static void FindAllGlobalObjects() {
         UI = Find("/_UI");
         Race = Find("/_Race");
         Global = Find("/_Global");
         SocketIO = Find("/_SocketIO");
         Environment = Find("/Environment");
         // Advertisement = Find("/_Advertisement");
      }
      
      /** Searches for objects by name and returns it, if it is found. */
      static GameObject Find(string name) {
         return GameObject.Find(name);
      }
   }
}