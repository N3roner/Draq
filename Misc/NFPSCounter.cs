using UnityEngine;
using UnityEngine.UI;

public class NFPSCounter : MonoBehaviour {
   float deltaTime = 0.0f;
   Text textComponent;
   int totalFrames;
   float totalFps;
   float avgFps;

   void Start() {
      textComponent = gameObject.GetComponent<Text>();
   }

   void Update() {
      deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
      float fps = (1f / deltaTime);
      totalFrames++;
      totalFps += fps;
      avgFps = Mathf.Round(totalFps / totalFrames);
      textComponent.text = ("FPS : " + 1.0f / deltaTime).ToString() + "\n" + "AVG : " + avgFps;
   }
}
