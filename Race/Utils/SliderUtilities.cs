using System.Collections.Generic;
using System;

public struct UiSlider {
   public int SliderIndex;
   public float SliderValue;
   public bool TempLocked;
}

public class SliderUtilities {

   public static void UpdateSliderValues(float[] percentsArray, float[] percentsCopy, bool[] sliderLocks, List<UiSlider> sliders, int indexOfChangedValue) {
      int unlockedCounter = 0;
      for(int i = 0; i < percentsArray.Length; i++) {
         if(i < sliderLocks.Length) {
            if(sliderLocks[i] == false)
               unlockedCounter++;
         }
      }

      if(unlockedCounter == 1)
         percentsArray[indexOfChangedValue] = percentsCopy[indexOfChangedValue];

      float ammount = percentsArray[indexOfChangedValue] - percentsCopy[indexOfChangedValue];

      sliders = new List<UiSlider>();

      for(int i = 0; i < percentsArray.Length; i++) {
         if(sliderLocks[i] == false && i != indexOfChangedValue) {
            UiSlider tempSlider = new UiSlider();
            tempSlider.SliderIndex = i;
            tempSlider.SliderValue = percentsArray[i];
            sliders.Add(tempSlider);
         }
      }

      sliders.Sort((s1, s2) => s1.SliderValue.CompareTo(s2.SliderValue));

      if(ammount < 0f) {
         ammount = Math.Abs(ammount);
         for(int i = sliders.Count - 1; i >= 0; i--) {
            var maxAssign = 100f - percentsArray[sliders[i].SliderIndex];
            var assignment = maxAssign < ammount / (i + 1) ? maxAssign : ammount / (i + 1);

            percentsArray[sliders[i].SliderIndex] += assignment;
            ammount -= assignment;
         }
      }

      if(ammount > 0f) {
         for(int i = 0; i <= sliders.Count - 1; i++) {
            var maxAssign = percentsArray[sliders[i].SliderIndex];
            var assignment = maxAssign < ammount / (sliders.Count - i) ? maxAssign : ammount / (sliders.Count - i);

            percentsArray[sliders[i].SliderIndex] -= assignment;
            ammount -= assignment;
         }
      }

      if(GetSum(percentsArray, sliderLocks, false) != 100f) {
         var sumOfothers = GetSum(percentsArray, sliderLocks, false) - percentsArray[indexOfChangedValue];
         percentsArray[indexOfChangedValue] = 100f - sumOfothers;
      }
   }

   public static float GetSum(float[] passedArray, bool[] locks, bool onlyLocked) {
      float lockedSum = 0;
      for(int i = 0; i < passedArray.Length; i++) {
         if(onlyLocked && locks[i] == true)
            lockedSum += passedArray[i];
         if(!onlyLocked)
            lockedSum += passedArray[i];
      }
      return lockedSum;
   }

   public static void ResetSliders(float[] passedArray, bool[] passedLocks) {
      for(int i = 0; i < passedArray.Length; i++) {
         passedLocks[i] = false;
         passedArray[i] = 100f / passedArray.Length;
      }      
   }

   public static void SaveValuesToCatched(float[] original, float[] copy) {
      for(int i = 0; i < original.Length; i++) {
         copy[i] = original[i];
      }
   }
}
