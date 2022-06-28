using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Record Container
///
/// Node for tree that contains replay names
/// and sorts them by finish order
/// </summary>

public class RecordContainer {

   List<string> Records;
   RecordContainer[] childNodes;

   /** constructor initializes all nodes with the same number of children (matching the number of racers) */
   public RecordContainer(int numberOfBranches) {
      Records = new List<string>();
      childNodes = new RecordContainer[numberOfBranches];
   }

   /** adds given record name to this node's records list and recursively to child nodes with indices matching keys */
   public void Add(string recordName, int[] keys) {
      Records.Add(recordName);
      if(keys.Length > 1) {
         keys = RemoveFirstKey(keys);
         if(keys[0] >= childNodes.Length) {
            Debug.Log("Invalid key in record library: " + keys[0]);
            return;
         }
         if(childNodes[keys[0]] == null)
            childNodes[keys[0]] = new RecordContainer(childNodes.Length);
         childNodes[keys[0]].Add(recordName, keys);
      }
   }

   /** returns a random record name if keys list length is 1, or strips first key and gets record from child with index matching first key */
   public List<string> GetRecords(int[] keys) {
      keys = RemoveFirstKey(keys);
      if(keys.Length == 0)
         return Records;
      if(keys[0] >= childNodes.Length || keys[0] < 0) {
         Debug.Log("Invalid key, no racer with number: " + (keys[0] + 1));
         return new List<string>();
      }
      if(childNodes[keys[0]] == null) {
         Debug.Log("No such key in library");
         return new List<string>();
      }
      return childNodes[keys[0]].GetRecords(keys);
   }

   /** returns a copy of the array without the first element */
   public static int[] RemoveFirstKey(int[] keys) {
      var result = new int[keys.Length - 1];
      for(int i = 0; i < result.Length; ++i)
         result[i] = keys[i + 1];
      return result;
   }
}
