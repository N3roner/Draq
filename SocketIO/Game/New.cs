using Newtonsoft.Json.Linq;
using Product.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace Product.Networking.Data
{
   public class FinishedEvent
   {
      public int EventId;
      public RaceDetails Race;
   }

   public class OpenEvent
   {
      public int EventId;
      public List<RaceDetailsParticipant> Racers;
   }

   public class NewSchedule : Unified.SerializableObject
   {
      public List<FinishedEvent> Finished;
      public List<OpenEvent> Open;

      public override bool FromMessage(JToken data) {
         if (data.IsNullOrEmpty())
            return false;

         Finished = new List<FinishedEvent>();

         if(!data["finished"].IsNullOrEmpty()) {
            if(!data["finished"].ParseList((value, key) => {
               var r = new FinishedEvent();
               r.Race = new RaceDetails();
               r.Race.FromMessage(value);

               int.TryParse(key, out r.EventId);

               Finished.Add(r);
            }, true)) {
               return false;
            }
         }

         // create open list

         Open = new List<OpenEvent>();

         if(!data["open"].ParseList((value, key) => {
            var r = new OpenEvent();

            r.Racers = Results.RaceDetailsParticipantList(value);

            int.TryParse(key, out r.EventId);

            Open.Add(r);
         }, true))
            return false;

         // Create an empty racer if the other racer is not know at this time
         foreach(var open in Open) {
            if(open.Racers.Count == 1) {
               var r = new RaceDetailsParticipant();

               open.Racers.Add(r);
            }
         }

         return true;
      }
   }

   public class NewNextRace : Unified.SerializableObject
   {
      public RaceDetailsParticipant Racer1;
      public RaceDetailsParticipant Racer2;
      public List<RaceDetails> HeadToHead;

      public override bool FromMessage(JToken data) {
         Racer1 = new RaceDetailsParticipant();
         if(!Racer1.FromMessage(data["racer1"]))
            return false;

         Racer2 = new RaceDetailsParticipant();
         if(!Racer2.FromMessage(data["racer2"]))
            return false;

         HeadToHead = new List<RaceDetails>();

         var headToHead = data["headToHead"].TokensList(true);
         if(headToHead == null)
            return false;

         if(headToHead != null && headToHead.Count > 0) {
            foreach(var item in headToHead) {
               var h2h = new RaceDetails();
               if(!h2h.FromMessage(item))
                  return false;

               HeadToHead.Add(h2h);
            }
         }

         return true;
      }
   }

   public class New : Results 
   {
      public NewSchedule Schedule;
      public NewNextRace NextRace;

      public New() : base() {
         RaceDetailsRequired = false;      
      }

      /** Deserialize object from given message */
      public override bool FromMessage(JToken data){
         if(!base.FromMessage(data))
            return false;

         Schedule = new NewSchedule();
         if(!Schedule.FromMessage(data["schedule"]))
            return false;

         NextRace = new NewNextRace();
         if(!NextRace.FromMessage(data["nextRace"]))
            return false;

         return true;
      }
   }
}

