using UnityEditor;

[CustomEditor(typeof(TimeController))]
public class TimeControllerEditor : Editor {

   TimeController controller;
   bool slowMotionSection;
   bool simpleSlowMotionSection;
   bool expensiveSlowMotionSection;

   void OnEnable() {
      controller = (TimeController)target;
   }

   public override void OnInspectorGUI() {
      controller.TimeControllerState = (TimeControllerStates)EditorGUILayout.EnumPopup("Timecontroller state ", controller.TimeControllerState);
      if(EditorApplication.isPlaying /*&& controller.TimeSlices != null*/) {
         var sliderRightValue = 0f;
         if(controller.TimeSlices != null)
            sliderRightValue = controller.TimeSlices[controller.TimeSlices.Count - 1].ReplayTimeEnd;
         else
            sliderRightValue = 40f;
         controller.ReplayTime = EditorGUILayout.Slider("Replay time", controller.ReplayTime, 0, sliderRightValue);
      }
      controller.ReplaySpeed = EditorGUILayout.FloatField("Replay speed", controller.ReplaySpeed);
      controller.RecordingTimestep = EditorGUILayout.FloatField("Recording timestep", controller.RecordingTimestep);
      controller.RaceTime = EditorGUILayout.FloatField("Race time", controller.RaceTime);
      controller.RaceSpeed = EditorGUILayout.FloatField("Race speed", controller.RaceSpeed);

      EditorGUILayout.LabelField("- Delays");
      controller.RaceIntroDelay = EditorGUILayout.FloatField("  Race intro delay", controller.RaceIntroDelay);
      controller.RaceInitDelay = EditorGUILayout.FloatField("  Race init delay", controller.RaceInitDelay);
      controller.SlowMoInitDelay = EditorGUILayout.FloatField("  Slowmo init delay", controller.SlowMoInitDelay);
      controller.RaceEndingDelay = EditorGUILayout.FloatField("  Race ending delay", controller.RaceEndingDelay);

      slowMotionSection = EditorGUILayout.Foldout(slowMotionSection, "   Slow motion settings");

      if(slowMotionSection) {
         controller.LinearSlowMo = EditorGUILayout.Toggle("Use linear slow motion", controller.LinearSlowMo);
         simpleSlowMotionSection = EditorGUILayout.Foldout(simpleSlowMotionSection, "   Linear slow motion settings");
         if(simpleSlowMotionSection) {
            //controller.RaceInitDelay = EditorGUILayout.FloatField("Race init delay", controller.RaceInitDelay);
            controller.SlowMoInitDelay = EditorGUILayout.FloatField("Slow motion init delay", controller.SlowMoInitDelay);
            controller.SlowMoRaceSpeed = EditorGUILayout.FloatField("Slow motion race speed", controller.SlowMoRaceSpeed);
            controller.FreezeDuration = EditorGUILayout.FloatField("Freeze duration", controller.FreezeDuration);
            controller.SlowMoTimeOffset = EditorGUILayout.FloatField("Slow motion time", controller.SlowMoTimeOffset);
         }

         expensiveSlowMotionSection = EditorGUILayout.Foldout(expensiveSlowMotionSection, "   Dynamic slow motion settings");

         if(expensiveSlowMotionSection) {
            controller.SlowMoTimeOffset = EditorGUILayout.FloatField("Slow motion time offset", controller.SlowMoTimeOffset);
            controller.SlowmoEntryTransition = EditorGUILayout.FloatField("Slowmo entry transition", controller.SlowmoEntryTransition);
            controller.SlowmoExitTransition = EditorGUILayout.FloatField("Slowmo exit transition", controller.SlowmoExitTransition);
            controller.FreezeDuration = EditorGUILayout.FloatField("Freeze duration", controller.FreezeDuration);
            controller.SlowmoSpeed = EditorGUILayout.FloatField("Slowmo speed", controller.SlowmoSpeed);
         }
      }
   }
}
