using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class PrefabInformation {
   public int Id;
   public string Name;
   public string PrefabPath;
   public string ScreenShotPath;
};
#if UNITY_EDITOR
[Serializable]
public class ScreenShotsTaker {
   [SerializeField]
   public Camera UsedCam;
   public List<GameObject> SSObjects;
   public int ResWidth = 128;
   public int ResHeight = 128;
};

public class CarPrefabsGenerator : MonoBehaviour {
   [SerializeField]
   public ScreenShotsTaker SSTaker;
   public GameObject SSObj;
   public List<GameObject> Cars;
   public List<GameObject> Tires;
   public List<Car> ReferencedAccelerationSettings;
   public Car ReferenceCar;
   public GameObject ImportedCar;
   public GameObject ImportedTire;
   public List<TimeClass> Classes;
   public string ScreenShotsPath;
   public int NumberOfClasses;
   public string AvailableCarPrefabsPath = "Assets/CG/pfb/";
   public string AvailableTires = "Assets/CG/pfb/Tires/";
   public string AvailableReferenceCarsPath = "Assets/CG/pfb/ReferenceCars/";
   public string PrefabsDirectory = "Assets/Resources/Racers/";
   public string TireMaterialsPath = "Assets/Resources/Racers/Tirematerials/";
   public string AvailableMaterialsPath = "Assets/CG/mat/";
   public float WidthEditor;
   public float ColorPickersWidth = 31;
   public float MaterialPickerWidth = 35;
   public float XAxesOffset = 4;
   public float ZAxesOffset = 7;
   public string ScreenShotExtension;
   public string PrefabsManifestFileName;
   List<GameObject> carVariations;
   List<CarParts> availableParts;
   List<FileInfo> availableMaterials;
   List<string> originalMaterialsPath;
#if UNITY_EDITOR
   PrefabGeneratorWindow generatorWindow;
#endif
   public void CloseCurrentWindow() {
      generatorWindow.Close();
   }

   public void GetNewGeneratorWindow() {
      if(generatorWindow == null) {
         generatorWindow = PrefabGeneratorWindow.GetNewWindow();
         generatorWindow.SetController(this);
      }
   }

   public void GenerateVariations() {
      availableParts = new List<CarParts>();
      GetAvailableMaterials();
      ExtractMaterialsForEachCar(PrefabsDirectory);
      GetAvailableParts();
      ExtractTireMaterials();

      Vector3 carPos = new Vector3();
      carVariations = new List<GameObject>();
      for(int i = 0; i < Cars.Count; i++) {
         carPos.x = 0;
         for(int j = 0; j < Classes.Count; j++) {
            for(int k = 0; k < Classes[j].NumberOfVariations; k++) {
               GameObject tempVariation = Instantiate(Cars[i]);
               var tempOtherParent = tempVariation.transform.FindChild("FWD").GetChild(0).GetChild(0);
               while(tempOtherParent.transform.childCount > 1) {
                  if(!tempOtherParent.transform.GetChild(0).name.Contains("driver"))
                     DestroyImmediate(tempOtherParent.transform.GetChild(0).gameObject);

                  if(tempOtherParent.transform.GetChild(0).name.Contains("driver") && tempOtherParent.transform.childCount > 1)
                     DestroyImmediate(tempOtherParent.transform.GetChild(1).gameObject);
               }
               tempVariation.name = GetVariationName(i, j, k);
               tempVariation.AddComponent<Variation>().VariationConstructor(availableParts[i], Classes[j].Setups[k], originalMaterialsPath[i], TireMaterialsPath);

               if(ReferencedAccelerationSettings == null || ReferencedAccelerationSettings.Count <= 0)
                  Debug.LogError("No acceleration settings referenced");

               var randomAccIndex = UnityEngine.Random.Range(0, ReferencedAccelerationSettings.Count);
               var referenceCarSettings = ReferencedAccelerationSettings[randomAccIndex];
               tempVariation.AddComponent<Car>().GetCopyOf(referenceCarSettings);
               carVariations.Add(tempVariation);
               carPos.x += XAxesOffset;
               var usedTires = availableParts[i].Tires[(int)Classes[j].Setups[k].Tires.UsedPart].PartObject;
               if(int.Parse(usedTires.name.Split('_')[2].Substring(1)) < 4)
                  carPos.y = -0.024f;
               tempVariation.transform.position = carPos;
            }
            carPos.x = 0;
            carPos.z -= ZAxesOffset;
         }
      }

      AssetDatabase.Refresh();

      for(int i = 0; i < carVariations.Count; i++)
         carVariations[i].GetComponent<Variation>().AssignMaterials();

      for(int i = 0; i < carVariations.Count; i++)
         carVariations[i].GetComponent<Variation>().RemoveOriginalMatDirectory();

      UpdateVariationsColors();
   }

   public void EditVariation(int classIndex, int variationIndex, bool editOnlyColors = false) {
      for(int i = 0; i < Cars.Count; i++) {
         var tempVariation = GetVariation(i, classIndex, variationIndex);
         if(tempVariation != null)
            tempVariation.EditCarPart(Classes[classIndex].Setups[variationIndex], editOnlyColors, true);
      }
      if(!editOnlyColors)
         AutosavePrefabSetup();
   }

   public void ImportNewCar() {
      var prefab = PrefabUtility.GetPrefabParent(ImportedCar);
      if(prefab == null)
         Debug.LogWarning("Game object : " + ImportedCar.name + " doesn't have a prefab. Create prefab and than import");
      else
         Cars.Add(prefab as GameObject);
      ImportedCar = null;
   }

   public void ImportNewReferenceCar() {
      ReferencedAccelerationSettings.Add(ReferenceCar);
      ReferenceCar = null;
   }

   public void GeneratePrefabs() {
      if(!Directory.Exists(PrefabsDirectory))
         Directory.CreateDirectory(PrefabsDirectory);

      for(int i = 0; i < carVariations.Count; i++) {
         var variationComponent = carVariations[i].GetComponent<Variation>();
         PrefabUtility.CreatePrefab(variationComponent.GetVariationDirectory() + ".prefab", carVariations[i]);
         TakePrefabScreenShot(carVariations[i], variationComponent.GetVariationDirectory());
         if(variationComponent != null)
            DestroyImmediate(variationComponent);
      }
      AssetDatabase.Refresh();
   }

   public void EditVariationsPosition() {
      if(carVariations.Count == 0)
         return;
      Vector3 carPos = new Vector3();
      for(int i = 0; i < Cars.Count; i++) {
         carPos.x = 0;
         for(int j = 0; j < Classes.Count; j++) {
            for(int k = 0; k < Classes[j].NumberOfVariations; k++) {
               GameObject tempVariation = GetCarToEdit(i, j, k);
               if(tempVariation)
                  tempVariation.transform.position = carPos;
               carPos.x += XAxesOffset;
            }
            carPos.x = 0;
            carPos.z -= ZAxesOffset;
         }
      }
   }

   public void ImportAll() {
      ImportAllCars();
      ImportAllReferenceCars();
      ImportAllTires();
   }

   public void ClearAll() {
      Tires = new List<GameObject>();
      Cars = new List<GameObject>();
      ReferencedAccelerationSettings = new List<Car>();
   }

   public void ImportAllTires() {
      Tires = new List<GameObject>();
      var info = new DirectoryInfo(AvailableTires);
      foreach(var file in info.GetFiles())
         if(!file.Name.Contains(".meta") && file.Name.Contains("tire"))
            Tires.Add(AssetDatabase.LoadAssetAtPath<GameObject>(AvailableTires + file.Name));

   }

   public void ImportAllCars() {
      Cars = new List<GameObject>();
      var info = new DirectoryInfo(AvailableCarPrefabsPath);
      foreach(var file in info.GetFiles())
         if(!file.Name.Contains(".meta") && !file.Name.Contains("tire") && file.Name.Contains("pfb"))
            Cars.Add(AssetDatabase.LoadAssetAtPath<GameObject>(AvailableCarPrefabsPath + file.Name));

   }

   public void ImportAllReferenceCars() {
      ReferencedAccelerationSettings = new List<Car>();
      var info = new DirectoryInfo(AvailableReferenceCarsPath);
      foreach(var file in info.GetFiles())
         if(!file.Name.Contains(".meta"))
            ReferencedAccelerationSettings.Add(AssetDatabase.LoadAssetAtPath<GameObject>(AvailableReferenceCarsPath + file.Name).gameObject.GetComponent<Car>());

   }

   public void OneClick() {
      if(CheckForMissingComponents())
         return;
      ClearAll();
      ImportAll();
      GenerateVariations();
      GeneratePrefabs();
      CreatePrefabsManifestFile(PrefabsManifestFileName);
      for(int i = 0; i < carVariations.Count; i++)
         DestroyImmediate(carVariations[i]);
   }

   public void MouseListener() {
      if(Event.current.type == EventType.MouseUp && Event.current.button == 0)
         AutosavePrefabSetup();
   }

   public void Init() {
      if(Cars == null || Cars.Count == 0)
         Cars = new List<GameObject>();
      if(Classes == null || Classes.Count == 0)
         Classes = new List<TimeClass>();
      if(Tires == null || Tires.Count == 0)
         Tires = new List<GameObject>();
      if(SSTaker == null)
         SSTaker = new ScreenShotsTaker();
      if(SSTaker.SSObjects == null)
         SSTaker.SSObjects = new List<GameObject>();
      if(ReferencedAccelerationSettings == null || ReferencedAccelerationSettings.Count == 0)
         ReferencedAccelerationSettings = new List<Car>();
      ScreenShotExtension = (ScreenShotExtension == null || ScreenShotExtension == string.Empty) ? ".png" : ScreenShotExtension;
      PrefabsManifestFileName = (PrefabsManifestFileName == null || PrefabsManifestFileName == string.Empty) ? "PrefabsManifest.txt" : PrefabsManifestFileName;
   }

   public void EditTimeClassesList() {
      if(NumberOfClasses > Classes.Count)
         for(int i = Classes.Count; i < NumberOfClasses; i++)
            Classes.Add(new TimeClass());
      else
         for(int i = Classes.Count - 1; i >= NumberOfClasses; i--)
            Classes.Remove(Classes[i]);
   }

   public void EditTimeClassSetups(int TimeClassIndex) {
      if(Classes[TimeClassIndex].NumberOfVariations <= 0)
         return;

      if(Classes[TimeClassIndex].Setups == null)
         Classes[TimeClassIndex].Setups = new List<VariationSetup>();

      if(Classes[TimeClassIndex].Setups.Count != Classes[TimeClassIndex].NumberOfVariations) {
         if(Classes[TimeClassIndex].NumberOfVariations > Classes[TimeClassIndex].Setups.Count)
            for(int i = Classes[TimeClassIndex].Setups.Count; i < Classes[TimeClassIndex].NumberOfVariations; i++)
               Classes[TimeClassIndex].Setups.Add(new VariationSetup());
         else
            for(int i = Classes[TimeClassIndex].Setups.Count - 1; i > Classes[TimeClassIndex].NumberOfVariations; i--)
               Classes[TimeClassIndex].Setups.Remove(Classes[TimeClassIndex].Setups[i]);
      }
   }

   public void CreatePrefabsManifestFile(string fileName, string fullPath = null) {
      var mainDirectoryInfo = new DirectoryInfo(PrefabsDirectory);
      var level1Directories = mainDirectoryInfo.GetDirectories();

      int id = 1;
      var manifestPath = fullPath == null ? PrefabsDirectory + "/" + fileName : fullPath + fileName;
      var manifestFile = new StreamWriter(manifestPath, false);

      foreach(var dir in level1Directories) {
         var level1DirectoriesInfo = new DirectoryInfo(dir.FullName);
         var level2Directories = level1DirectoriesInfo.GetDirectories();

         Array.Sort(level2Directories, (f1, f2) => CompareVariations(f1.Name, f2.Name));

         foreach(var dirLvl2 in level2Directories) {
            var level2DirectoriesInfo = new DirectoryInfo(dirLvl2.FullName);
            var files = level2DirectoriesInfo.GetFiles();

            for(int i = 0; i < files.Length; i++) {
               if(files[i].Name.Contains("prefab") && !files[i].Name.Contains(ScreenShotExtension) && !files[i].Name.Contains("meta")) {
                  PrefabInformation tempInfo = new PrefabInformation();
                  tempInfo.Id = id;
                  tempInfo.Name = RemovePrefabSufix(files[i].Name);
                  tempInfo.ScreenShotPath = GetScreenshotAssetPath(files[i]);
                  tempInfo.PrefabPath = GetPrefabAssetPath(files[i]);
                  string json = JsonConvert.SerializeObject(tempInfo);
                  manifestFile.WriteLine(json);
                  id++;
               }
            }
         }
      }
      manifestFile.Close();
      AssetDatabase.Refresh();
   }

   public void EditNumberOfVariations(int classIndex) {
      if(Classes[classIndex].NumberOfVariations < 0)
         Classes[classIndex].NumberOfVariations = 1;
   }

   void GetAvailableParts() {
      availableParts = new List<CarParts>();

      for(int i = 0; i < Cars.Count; i++) {
         InitCarParts(i);
         var partsParent = Cars[i].transform.FindChild("FWD").GetChild(0).GetChild(0);

         for(int j = 0; j < partsParent.childCount; j++) {
            var tempContainer = new PartContainer();
            tempContainer.PartObject = partsParent.GetChild(j).gameObject;

            if(partsParent.GetChild(j).name.Contains("bumper") && partsParent.GetChild(j).name.Contains("back"))
               availableParts[i].RearBumpers.Add(tempContainer);

            if(partsParent.GetChild(j).name.Contains("bumper") && partsParent.GetChild(j).name.Contains("front"))
               availableParts[i].FrontBumpers.Add(tempContainer);

            if(partsParent.GetChild(j).name.Contains("spoiler") && !partsParent.GetChild(j).name.Contains("shadow"))
               availableParts[i].Spoilers.Add(tempContainer);

            if(partsParent.GetChild(j).name.Contains("hood"))
               availableParts[i].HoodScoops.Add(tempContainer);

            if(partsParent.GetChild(j).name.Contains("body") && !partsParent.GetChild(j).name.Contains("shadow"))
               availableParts[i].CarBody = tempContainer;

            if(partsParent.GetChild(j).name.Contains("body") && partsParent.GetChild(j).name.Contains("shadow"))
               availableParts[i].CarBodyShadow = partsParent.GetChild(j).gameObject;

            if(partsParent.GetChild(j).name.Contains("lightglass"))
               availableParts[i].LightGlass = partsParent.GetChild(j).gameObject;

            if(partsParent.GetChild(j).name.Contains("glasses"))
               availableParts[i].Glasses = tempContainer;

            if(partsParent.GetChild(j).name.Contains("interior"))
               availableParts[i].Interior = tempContainer;
         }

         var axlesParent = Cars[i].transform.FindChild("Axles");

         for(int k = 0; k < 2; k++) {
            var axleContainer = new PartContainer();
            axleContainer.PartObject = axlesParent.GetChild(k).gameObject;
            availableParts[i].Axles.Add(axleContainer);
         }

         if(availableParts[i].Tires == null)
            availableParts[i].Tires = new List<PartContainer>();

         foreach(var tire in Tires) {
            var tempTireContainer = new PartContainer();
            tempTireContainer.PartObject = tire;
            availableParts[i].Tires.Add(tempTireContainer);
         }
      }
   }

   bool CheckForMissingComponents() {
      var somethingWrong = false;
      var pfbFiles = new DirectoryInfo(AvailableCarPrefabsPath).GetFiles();
      var pfbCounter = 0;
      foreach(var file in pfbFiles)
         if(file.Name.Contains("pfb"))
            pfbCounter++;
      if(pfbFiles.Length == 0 || pfbCounter == 0) {
         Debug.LogWarning("No prefabs found");
         somethingWrong = true;
      }

      var refPfb = new DirectoryInfo(AvailableReferenceCarsPath).GetFiles();
      if(refPfb.Length == 0) {
         Debug.LogWarning("No reference cars found");
         somethingWrong = true;
      }

      if(SSTaker == null || SSTaker.UsedCam == null) {
         Debug.LogWarning("No screenshot cam found");
         somethingWrong = true;
      }

      if(SSTaker.SSObjects.Count == 0) {
         Debug.LogWarning("No postures found");
         somethingWrong = true;
      }

      return somethingWrong;
   }

   void TakePrefabScreenShot(GameObject car, string savePath) {
      if(SSTaker == null || SSTaker.UsedCam == null)
         Debug.LogWarning("No screenshot cam found");
      if(SSTaker.SSObjects.Count == 0)
         Debug.LogWarning("No postures found");

      SSTaker.SSObjects[0].SetActive(false);
      var prevPos = car.transform.position;
      var prevRot = car.transform.rotation;
      car.transform.position = SSTaker.SSObjects[0].transform.position;
      car.transform.rotation = SSTaker.SSObjects[0].transform.rotation;

      TakeScreenShot(car, savePath);
      car.transform.position = prevPos;
      car.transform.rotation = prevRot;
   }

   void TakeScreenShot(GameObject carToSS, string savePath) {
      byte[] bytes = GetScreenShotBytesFile();
      string filename = savePath + ScreenShotExtension;
      File.WriteAllBytes(filename, bytes);
   }

   byte[] GetScreenShotBytesFile() {
      bool isTransparent = true;
      var scale = 1;
      int resWidthN = SSTaker.ResWidth * scale;
      int resHeightN = SSTaker.ResHeight * scale;
      RenderTexture rt = new RenderTexture(resWidthN, resHeightN, 24);
      SSTaker.UsedCam.targetTexture = rt;
      TextureFormat tFormat;
      tFormat = isTransparent ? TextureFormat.ARGB32 : TextureFormat.RGB24;
      Texture2D screenShot = new Texture2D(resWidthN, resHeightN, tFormat, false);
      SSTaker.UsedCam.Render();
      RenderTexture.active = rt;
      screenShot.ReadPixels(new Rect(0, 0, resWidthN, resHeightN), 0, 0);
      SSTaker.UsedCam.targetTexture = null;
      RenderTexture.active = null;
      byte[] bytes = screenShot.EncodeToPNG();
      return bytes;
   }

   void GetAvailableMaterials() {
      availableMaterials = new List<FileInfo>();
      var info = new DirectoryInfo(AvailableMaterialsPath);
      foreach(var file in info.GetFiles())
         if(!file.Name.Contains(".meta"))
            availableMaterials.Add(file);
   }

   void UpdateVariationsColors() {
      for(int i = 0; i < Cars.Count; i++)
         for(int j = 0; j < Classes.Count; j++)
            for(int k = 0; k < Classes[j].NumberOfVariations; k++) {
               var tempVariation = GetVariation(i, j, k);
               if(tempVariation != null)
                  tempVariation.UpdateColors(Classes[j].Setups[k], false);
            }
   }

   string GetVariationName(int carIndex, int classIndex, int variationIndex) {
      if(Cars[carIndex] == null) {
         Debug.LogError("Car at index : " + carIndex + " deleted");
         return null;
      }
      return Cars[carIndex].name + "_" + Classes[classIndex].ClassTime + "_V_" + (variationIndex + 1);
   }

   Variation GetVariation(int carIndex, int classIndex, int variationIndex) {
      var tempCar = GetCarToEdit(carIndex, classIndex, variationIndex);
      return tempCar == null ? null : tempCar.GetComponent<Variation>();
   }

   GameObject GetCarToEdit(int carIndex, int classIndex, int variationIndex) {
      return GameObject.Find(GetVariationName(carIndex, classIndex, variationIndex));
   }

   void InitCarParts(int index) {
      availableParts.Add(new CarParts());
      availableParts[index].RearBumpers = new List<PartContainer>();
      availableParts[index].FrontBumpers = new List<PartContainer>();
      availableParts[index].Spoilers = new List<PartContainer>();
      availableParts[index].HoodScoops = new List<PartContainer>();
      availableParts[index].Axles = new List<PartContainer>();
      availableParts[index].CarBody = new PartContainer();
   }

   void ExtractTireMaterials() {
      if(Directory.Exists(TireMaterialsPath))
         return;
      Directory.CreateDirectory(TireMaterialsPath);

      for(int j = 0; j < availableMaterials.Count; j++)
         if(availableMaterials[j].Name.Contains("tire")) {
            var tempSource = availableMaterials[j].ToString().Replace(@"\", "\\\\");
            var tempDestination = TireMaterialsPath + availableMaterials[j].Name;
            FileUtil.CopyFileOrDirectory(tempSource, tempDestination);
         }
   }

   void ExtractMaterialsForEachCar(string prefabsDirectory) {
      originalMaterialsPath = new List<string>();
      for(int i = 0; i < Cars.Count; i++) {
         var carName = Cars[i].name.Split('_');
         var carPath = prefabsDirectory + carName[1] + "_" + carName[2];
         if(!Directory.Exists(carPath))
            Directory.CreateDirectory(carPath);
         var matPath = carPath + "/mat_" + carName[1] + "_" + carName[2];
         originalMaterialsPath.Add(matPath);
         if(!Directory.Exists(matPath))
            Directory.CreateDirectory(matPath);

         for(int j = 0; j < availableMaterials.Count; j++) {
            if(availableMaterials[j].Name.Contains(carName[1]) && availableMaterials[j].Name.Contains(carName[2]) && !availableMaterials[j].Name.Contains("tire")) {
               var tempSource = availableMaterials[j].ToString().Replace(@"\", "\\\\");
               var tempDestination = matPath + "/" + availableMaterials[j].Name;
               FileUtil.CopyFileOrDirectory(tempSource, tempDestination);
            }
            if(availableMaterials[j].Name == "mat_lightglass.mat" || availableMaterials[j].Name == "mat_driver.mat") {
               var tempSource = availableMaterials[j].ToString().Replace(@"\", "\\\\");
               var tempDestination = matPath + "/" + availableMaterials[j].Name;
               FileUtil.CopyFileOrDirectory(tempSource, tempDestination);
            }
         }
      }
   }

   void AutosavePrefabSetup() {
      var prefab = PrefabUtility.GetPrefabParent(gameObject);
      if(prefab == null)
         Debug.LogWarning("Game object : '' " + gameObject.name + " '' is not saved as prefab. Create prefab to enable autosave");
      else
         PrefabUtility.ReplacePrefab(gameObject, prefab);
   }

   string GetPrefabAssetPath(FileInfo fileInfo) {
      var prefabsDirectoryName = PrefabsDirectory.Split('/');
      var lastSplited = prefabsDirectoryName[prefabsDirectoryName.Length - 2];
      var toReturn = fileInfo.FullName;
      var indexOfAsset = toReturn.IndexOf(lastSplited);
      return toReturn.Substring(indexOfAsset);
   }

   string GetScreenshotAssetPath(FileInfo fileInfo) {
      var prefabAssetPath = GetPrefabAssetPath(fileInfo);
      return RemovePrefabSufix(prefabAssetPath) + ScreenShotExtension;
   }

   static int CompareVariations(string s1, string s2) {
      var s1Splited = s1.Split('_');
      var s1ClassID = int.Parse(s1Splited[2]);
      int s1VariationID = int.Parse(s1Splited[4]);

      var s2Splited = s2.Split('_');
      var s2ClassID = int.Parse(s2Splited[2]);
      var s2VariationID = int.Parse(s2Splited[4]);

      if(s1ClassID == s2ClassID)
         return s1VariationID.CompareTo(s2VariationID);
      return s1ClassID.CompareTo(s2ClassID);
   }

   string RemovePrefabSufix(string withSufix) {
      return withSufix.Substring(0, withSufix.Length - 7);
   }
}
#endif
