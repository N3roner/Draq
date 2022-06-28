using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Record Manager
///
/// Organizes recorded replays' filenames in a tree, sorted by finish order,
/// and retrieves a random recording filename with a specified finish order
/// </summary>

public class RecordManager {

   public List<string> Records { get; private set; }
   public int RacerCount { get; private set; }
   public int ClassTime { get; private set; }

   RecordContainer[] recordsTree;
   string saveFolder;

   /** constructor defines number of racers, only saves with the same number are compatible */
   public RecordManager(int classTime) {
      ClassTime = classTime;
      saveFolder = Application.dataPath + "/Races/" + ClassTime + '/';
      recordsTree = new RecordContainer[RacerCount];
      Records = new List<string>();
   }

   /** populates the Records list with all .bytes files found in appropriate Races folder */
   public void Load() {
      Records = new List<string>();
      string[] raceFiles;
      if(!Directory.Exists(saveFolder)) {
         Debug.Log("Could not load replays: race records directory has not been created yet");
         return;
      }
      raceFiles = Directory.GetFiles(saveFolder, "*.bytes");
      for(int i = 0; i < raceFiles.Length; ++i) {
         raceFiles[i] = raceFiles[i].Substring(saveFolder.Length);
         AddRecord(raceFiles[i]);
      }
   }

   /** add filename to records tree if it's format is compatible */
   public void AddRecord(string recordName) {
      if(recordName == string.Empty)
         return;
      //var finishOrder = ParseSaveNameByIndex(recordName);
      //if(finishOrder == null || finishOrder.Length != RacerCount)
      //   return;

      Records.Add(recordName);
      //if(recordsTree[finishOrder[0]] == null)
      //   recordsTree[finishOrder[0]] = new RecordContainer(RacerCount);
      //recordsTree[finishOrder[0]].Add(recordName, finishOrder);
   }

   /** retrieve a random recording name with finish order matching the provided array (places beyond array length are ignored) */
   public string GetRecord(int[] finishOrder) {
      if(finishOrder.Length == 0)
         return GetRecord();
      int[] finishOrderClone = finishOrder.Clone() as int[];
      for(int i = 0; i < finishOrderClone.Length; ++i)
         finishOrderClone[i]--;
      if(finishOrderClone[0] > RacerCount || finishOrderClone[0] < 0) {
         Debug.Log("Invalid key - no racer with number: " + (finishOrderClone[0] + 1));
         return string.Empty;
      }
      if(recordsTree[finishOrderClone[0]] == null) {
         Debug.Log("No such record in library");
         return string.Empty;
      }
      List<string> records = recordsTree[finishOrderClone[0]].GetRecords(finishOrderClone);
      if(records.Count == 0)
         return string.Empty;
      return records[UnityEngine.Random.Range(0, records.Count)];
   }

   /** get any random recording filename */
   public string GetRecord() {
      if(Records == null) {
         Debug.Log("Replay library is not ready");
         return string.Empty;
      }
      if(Records.Count == 0) {
         Debug.Log("Error: there are no replays in the library.");
         return string.Empty;
      }
      return Records[UnityEngine.Random.Range(0, Records.Count)];
   }

   /** returns all registered records who's finish places match the given finish order, partial finish order starts from first place */
   public List<string> ListRecords(int[] finishOrder) {
      if(finishOrder.Length == 0)
         return ListRecords();
      int[] finishOrderClone = finishOrder.Clone() as int[];
      for(int i = 0; i < finishOrderClone.Length; ++i)
         finishOrderClone[i]--;
      if(finishOrderClone[0] >= RacerCount || finishOrderClone[0] < 0) {
         Debug.Log("Invalid key - no racer with number " + finishOrder[0] + 1);
         return new List<string>();
      }
      if(recordsTree[finishOrderClone[0]] == null) {
         Debug.Log("No such record in library");
         return new List<string>();
      }
      return recordsTree[finishOrderClone[0]].GetRecords(finishOrderClone);
   }

   /** returns all registered records */
   public List<string> ListRecords() {
      return Records;
   }

   /** convert filename to an int array representing IDs of racers arranged by finish order */
   public static int[] ParseSaveName(string saveName) {
      if(saveName.Length < 1)
         return new int[0];
      int[] indices = ParseSaveNameByIndex(saveName);
      for(int i = 0; i < indices.Length; i++)
         indices[i]++;
      return indices;
   }


   /** convert filename to an int array representing indices of racers arranged by finish orders */
   static int[] ParseSaveNameByIndex(string saveName) {
      if(saveName.Length < 1)
         return new int[0];
      if(saveName[0] == '.')
         saveName = saveName.Substring(1);
      string[] subStrings = saveName.Split('_');
      if(subStrings.Length < 3) {
         Debug.Log("Invalid replay name: " + saveName);
         return null;
      }
      int[] result = new int[subStrings.Length - 2];
      for(int i = 0; i < result.Length; ++i) {
         int.TryParse(subStrings[i + 1], out result[i]);
         result[i]--;
      }
      return result;
   }

}
