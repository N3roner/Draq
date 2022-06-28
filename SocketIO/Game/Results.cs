using Newtonsoft.Json.Linq;
using Product.Extensions;
using System.Collections.Generic;

namespace Product.Networking.Data
{
   public class RaceDetailsParticipant : Unified.SerializableObject
   {
      public int RacerId;
      public string Name = "";
      public float RaceTime;
      public int RacerClass;
      public decimal WinnerOdds;
      public decimal RoundOf32Odds;
      public decimal EighthFinalsOdds;
      public decimal QuarterFinalsOdds;
      public decimal SemiFinalsOdds;
      public decimal FinalOdds;

      public override bool FromMessage(JToken data) {
         if(data.IsNullOrEmpty())
            return false;

         if(!data["racerId"].OutRequired(ref RacerId))
            return false;

         if(!data["name"].OutRequired(ref Name))
            return false;

         data["raceTime"].Out(ref RaceTime);
         data["class"].Out(ref RacerClass);

         data["winnerOdds"].Out(ref WinnerOdds);
         data["roundOf32Odds"].Out(ref RoundOf32Odds);
         data["eighthFinalsOdds"].Out(ref EighthFinalsOdds);
         data["quarterFinalsOdds"].Out(ref QuarterFinalsOdds);
         data["semiFinalsOdds"].Out(ref SemiFinalsOdds);
         data["finalOdds"].Out(ref FinalOdds);

         return true;
      }
   }

   public class RaceDetails : Unified.SerializableObject
   {
      public RaceDetailsParticipant Winner;
      public RaceDetailsParticipant Loser;

      public override bool FromMessage(JToken data) {
         if(data.IsNullOrEmpty())
            return false;

         Winner = new RaceDetailsParticipant();
         Loser = new RaceDetailsParticipant();

         if(!Winner.FromMessage(data["winner"]))
            return false;

         if(!Loser.FromMessage(data["loser"]))
            return false;

         return true;
      }
   }

   public class Results : Unified.SerializableObject
   {
      public int TournamentId;
      public int RaceId;
      public int EventId;
      public string TournamentPhase = "";
      public bool RaceDetailsRequired = true;

      public RaceDetails RaceDetails;
      public List<RaceDetails> PreviousResults;

      /** Deserialize object from given message */
      public override bool FromMessage(JToken data) {
         if(!data["tournamentId"].OutRequired(ref TournamentId))
            return false;

         data["eventId"].Out(ref EventId);

         if(!data["raceId"].OutRequired(ref RaceId))
            return false;

         if(!data["tournamentPhase"].OutRequired(ref TournamentPhase))
            return false;

         RaceDetails = new RaceDetails();
         if(!RaceDetails.FromMessage(data["raceDetails"]) && RaceDetailsRequired)
            return false;

         PreviousResults = new List<RaceDetails>();

         var previousResults = data["previousResults"].TokensList();

         if(previousResults != null) {
            foreach(var result in previousResults) {
               var r = new RaceDetails();

               if(!r.FromMessage(result))
                  return false;

               PreviousResults.Add(r);
            };
         }

         return true;
      }

      public static List<RaceDetailsParticipant> RaceDetailsParticipantList(JToken data) {
         var Details = new List<RaceDetailsParticipant>();

         data.ParseList((value, key) =>  {
            var r = new RaceDetailsParticipant();

            r.FromMessage(value);

            Details.Add(r);
         });

         return Details;
      }

      public static List<RaceDetails> RaceDetailsList(JToken data) {
         var Details = new List<RaceDetails>();

         if(!data.IsNullOrEmpty()) {
            var detailsList = data.TokensList();

            foreach (var d in detailsList) {
               var r = new RaceDetails();
               r.FromMessage(d);

               Details.Add(r);
            }
         }

         return Details;
      }
   }
}
