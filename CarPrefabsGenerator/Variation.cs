using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum PartValues
{
    A,
    B,
    C,
    D,
    E,
    F,
    G
};

[Serializable]
public enum MaterialValues
{
    A,
    B,
    C,
    D,
    E,
    F,
    G
};

public enum SpoilerValues
{
    NONE,
    A,
    B,
    C
};
public enum Decals
{
    A,
    B,
    C,
    D
};

public enum PartToDraw
{
    SPOILER,
    HOOD,
    FRONTBUMPER,
    REARBUMPER,
    TIRES,
    RIDECOLOR
};

[Serializable]
public class PartSetup
{
    public PartValues UsedPart;
    public SpoilerValues UsedSpoiler;
    public MaterialValues UsedMaterial;
    public Decals UsedDecal;

    public void CopyValues(PartSetup inputSetup)
    {
        UsedPart = inputSetup.UsedPart;
        UsedSpoiler = inputSetup.UsedSpoiler;
        UsedMaterial = inputSetup.UsedMaterial;
        UsedDecal = inputSetup.UsedDecal;
    }
};

[Serializable]
public class VariationSetup
{
    public PartSetup Spoiler;
    public PartSetup Hood;
    public PartSetup FrontBumper;
    public PartSetup RearBumper;
    public PartSetup Tires;
    public PartSetup Body;
    public PartSetup Glasses;
    public PartSetup Interior;
    public Decals UsedDecal;
    public Color RideColor;
    public Color SpoilerColor;
    public Color HoodColor;
    public Color FrontBumperColor;
    public Color RearBumperColor;
    public Color TiresColor;

    public void Initializer()
    {
        Spoiler = new PartSetup();
        Hood = new PartSetup();
        FrontBumper = new PartSetup();
        RearBumper = new PartSetup();
        Tires = new PartSetup();
        Body = new PartSetup();
        Glasses = new PartSetup();
        Interior = new PartSetup();
        UsedDecal = new Decals();
    }

    public void CopyValues(VariationSetup setup)
    {
        Spoiler.CopyValues(setup.Spoiler);
        Hood.CopyValues(setup.Hood);
        FrontBumper.CopyValues(setup.FrontBumper);
        RearBumper.CopyValues(setup.RearBumper);
        Body.CopyValues(setup.Body);
        Tires.CopyValues(setup.Tires);
        RideColor = setup.RideColor;
        SpoilerColor = setup.SpoilerColor;
        HoodColor = setup.HoodColor;
        FrontBumperColor = setup.FrontBumperColor;
        RearBumperColor = setup.RearBumperColor;
        UsedDecal = setup.UsedDecal;
    }
};

[Serializable]
public class TimeClass
{
    public int ClassTime;
    public int NumberOfVariations;
    public List<VariationSetup> Setups;
};

public class PartContainer
{
    public GameObject PartObject;
    public List<string> Materials;
};

[Serializable]
public class CarParts
{
    public List<PartContainer> FrontBumpers;
    public List<PartContainer> RearBumpers;
    public List<PartContainer> Spoilers;
    public List<PartContainer> HoodScoops;
    public List<PartContainer> Axles;
    public List<PartContainer> Tires;
    public PartContainer Interior;
    public PartContainer Glasses;
    public PartContainer CarBody;
    public GameObject CarBodyShadow;
    public GameObject LightGlass;
};

public class Variation : MonoBehaviour
{
    public VariationSetup VariationSetup;
    public bool ApplyChangesToMainEditor;
    VariationSetup MainEditorSetup;
    GameObject body;
    GameObject hood;
    GameObject frontBumper;
    GameObject rearBumper;
    GameObject spoiler;
    GameObject frontAxle;
    GameObject backAxle;
    GameObject glasses;
    GameObject interior;
    CarParts availableCarParts;
    string originalMatDirectory;
    string variationParentDirectory;
    string materialsDirectory;
    string tireMaterialsPath;
    public void VariationConstructor(CarParts inputAvailableParts, VariationSetup variationSetup, string originalMaterialsPath, string tireMatPath)
    {
        if (VariationSetup == null)
        {
            VariationSetup = new VariationSetup();
            VariationSetup.Initializer();
        }

        MainEditorSetup = variationSetup;
        tireMaterialsPath = tireMatPath;
        availableCarParts = inputAvailableParts;
        CreateVariationDirectory(originalMaterialsPath);
        SetMaterialsPath(originalMaterialsPath);
        CreateMaterialsDirectory();

        var axlesChild = gameObject.transform.FindChild("Axles").gameObject;
        var rwdGradChild = gameObject.transform.FindChild("FWD").GetChild(0).GetChild(0).gameObject;

        for (int i = 0; i < axlesChild.transform.childCount; i++)
        {
            if (axlesChild.transform.GetChild(i).name.Contains("axle_back"))
                backAxle = axlesChild.transform.GetChild(i).gameObject;
            if (axlesChild.transform.GetChild(i).name.Contains("axle_front"))
                frontAxle = axlesChild.transform.GetChild(i).gameObject;
        }

        body = ReplacePart(body, availableCarParts.CarBody.PartObject, variationSetup.RideColor, rwdGradChild, variationSetup.Body, true);
        frontBumper = ReplacePart(frontBumper, availableCarParts.FrontBumpers[(int)variationSetup.FrontBumper.UsedPart].PartObject, variationSetup.FrontBumperColor, rwdGradChild, variationSetup.FrontBumper, true);
        rearBumper = ReplacePart(rearBumper, availableCarParts.RearBumpers[(int)variationSetup.RearBumper.UsedPart].PartObject, variationSetup.RearBumperColor, rwdGradChild, variationSetup.RearBumper, true);
        hood = ReplacePart(hood, availableCarParts.HoodScoops[(int)variationSetup.Hood.UsedPart].PartObject, variationSetup.HoodColor, rwdGradChild, variationSetup.Hood);
        glasses = ReplacePart(glasses, availableCarParts.Glasses.PartObject, variationSetup.RideColor, rwdGradChild);
        interior = ReplacePart(interior, availableCarParts.Interior.PartObject, variationSetup.RideColor, rwdGradChild);
        VariationSetup.UsedDecal = variationSetup.UsedDecal;
        for (int i = 0; i < 2; i++)
        {
            var tempLeftTire = Instantiate(availableCarParts.Tires[(int)variationSetup.Tires.UsedPart].PartObject);
            var tempRightTire = Instantiate(availableCarParts.Tires[(int)variationSetup.Tires.UsedPart].PartObject);

            if (i == 0)
            {
                tempLeftTire.transform.SetParent(backAxle.transform.GetChild(0).transform.GetChild(0), false);
                tempRightTire.transform.SetParent(backAxle.transform.GetChild(0).transform.GetChild(1), false);
            }
            else
            {
                tempLeftTire.transform.SetParent(frontAxle.transform.GetChild(0).transform.GetChild(0), false);
                tempRightTire.transform.SetParent(frontAxle.transform.GetChild(0).transform.GetChild(1), false);
            }

            tempLeftTire.transform.localPosition = new Vector3(0, 0, 0);
            tempRightTire.transform.localPosition = new Vector3(0, 0, 0);
            RemoveCloneSufix(tempLeftTire);
            RemoveCloneSufix(tempRightTire);
        }
        if (availableCarParts.CarBodyShadow != null)
        {
            var tempBodyShadow = Instantiate(availableCarParts.CarBodyShadow);
            tempBodyShadow.transform.SetParent(rwdGradChild.transform);
            RemoveCloneSufix(tempBodyShadow);
            tempBodyShadow.SetActive(false);
        }

        if (availableCarParts.LightGlass != null)
        {
            var tempLightGlass = Instantiate(availableCarParts.LightGlass);
            tempLightGlass.transform.SetParent(rwdGradChild.transform);
            RemoveCloneSufix(tempLightGlass);
        }

        if (variationSetup.Spoiler.UsedSpoiler > 0)
            spoiler = ReplacePart(spoiler, availableCarParts.Spoilers[(int)variationSetup.Spoiler.UsedSpoiler - 1].PartObject, variationSetup.SpoilerColor, rwdGradChild, variationSetup.Spoiler);
    }

    public void EditCarPart(VariationSetup setup, bool editColorsOnly, bool assignSetupValues = false, bool editByVariation = false)
    {
        if (editColorsOnly)
        {
            UpdateColors(setup, true);
            if (VariationSetup == null)
            {
                VariationSetup = new VariationSetup();
                VariationSetup.Initializer();
            }
            if (editByVariation && ApplyChangesToMainEditor)
                MainEditorSetup.CopyValues(VariationSetup);
            return;
        }

        if (VariationSetup.Body.UsedMaterial != setup.Body.UsedMaterial || editByVariation)
            GetPartMaterial(body, (int)setup.Body.UsedMaterial);

        if (VariationSetup.Hood.UsedPart != setup.Hood.UsedPart || editByVariation)
            hood = ReplacePart(hood, availableCarParts.HoodScoops[(int)setup.Hood.UsedPart].PartObject, setup.HoodColor, null, setup.Hood, true);

        if ((VariationSetup.FrontBumper.UsedPart != setup.FrontBumper.UsedPart || VariationSetup.FrontBumper.UsedMaterial != setup.FrontBumper.UsedMaterial) || editByVariation)
            frontBumper = ReplacePart(frontBumper, availableCarParts.FrontBumpers[(int)setup.FrontBumper.UsedPart].PartObject, setup.FrontBumperColor, null, setup.FrontBumper, true);

        if ((VariationSetup.RearBumper.UsedPart != setup.RearBumper.UsedPart || VariationSetup.RearBumper.UsedMaterial != setup.RearBumper.UsedMaterial) || editByVariation)
            rearBumper = ReplacePart(rearBumper, availableCarParts.RearBumpers[(int)setup.RearBumper.UsedPart].PartObject, setup.RearBumperColor, null, setup.RearBumper, true);

        if ((VariationSetup.Spoiler.UsedSpoiler != setup.Spoiler.UsedSpoiler || VariationSetup.Spoiler.UsedMaterial != setup.Spoiler.UsedMaterial) || editByVariation)
        {
            if (setup.Spoiler.UsedSpoiler == 0)
                spoiler = ReplacePart(spoiler, null, setup.SpoilerColor, null, setup.Spoiler);
            if (setup.Spoiler.UsedSpoiler != 0)
                spoiler = ReplacePart(spoiler, availableCarParts.Spoilers[(int)setup.Spoiler.UsedSpoiler - 1].PartObject, setup.SpoilerColor, null, setup.Spoiler, true);
        }

        if (VariationSetup.Tires != setup.Tires || editByVariation)
            ReplaceTires(setup);

        if (assignSetupValues)
            VariationSetup.CopyValues(setup);

        if (editByVariation && ApplyChangesToMainEditor)
            MainEditorSetup.CopyValues(VariationSetup);

        ChangeDecal(VariationSetup.UsedDecal, body);
        ChangeDecal(VariationSetup.UsedDecal, frontBumper);
        ChangeDecal(VariationSetup.UsedDecal, rearBumper);
        ChangeDecal(VariationSetup.UsedDecal, hood);
    }

    public void AssignMaterials()
    {
        var partsParent = gameObject.transform.FindChild("FWD").transform.FindChild("RWD").GetChild(0);
        var axlesParent = gameObject.transform.FindChild("Axles");

        for (int i = 0; i < partsParent.transform.childCount; i++)
        {
            var tempName = partsParent.transform.GetChild(i).gameObject.name;
            var tempObject = partsParent.transform.GetChild(i).gameObject;

            if (tempName.Contains("spoiler"))
                GetPartMaterial(tempObject, (int)VariationSetup.Spoiler.UsedMaterial);

            if (tempName.Contains("hood"))
                GetPartMaterial(tempObject, (int)VariationSetup.Hood.UsedMaterial);

            if (tempName.Contains("front") && tempName.Contains("bumper"))
                GetPartMaterial(tempObject, (int)VariationSetup.FrontBumper.UsedMaterial);

            if (tempName.Contains("back") && tempName.Contains("bumper"))
                GetPartMaterial(tempObject, (int)VariationSetup.RearBumper.UsedMaterial);

            if (tempName.Contains("body") && !tempName.Contains("shadow"))
                AddMaterial(tempObject);

            if (tempName.Contains("lightglass"))
                AddMaterial(tempObject, "mat_lightglass.mat");

            if (tempName.Contains("interior"))
                AddMaterial(tempObject);

            if (tempName.Contains("glasses"))
                AddMaterial(tempObject);

            if (tempName.Contains("driver"))
            {
                AddMaterial(tempObject, "mat_driver.mat");
                var headIndex = UnityEngine.Random.Range(0, 2);
                var headMat = tempObject.transform.GetChild(headIndex).gameObject;
                tempObject.transform.GetChild(headIndex == 0 ? 1 : 0).gameObject.SetActive(false);
                AddMaterial(headMat, "mat_driver.mat");
            }
        }

        for (int i = 0; i < 2; i++)
        {
            if (axlesParent.transform.GetChild(i))
            {
                var partMaterial = "mat_" + gameObject.name.Substring(4) + "_body.mat";
                AddMaterial(axlesParent.transform.GetChild(i).gameObject, partMaterial);

                var tempLeftTire = axlesParent.transform.GetChild(i).transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
                var tempRightTire = axlesParent.transform.GetChild(i).transform.GetChild(0).GetChild(1).GetChild(0).gameObject;

                AddTireMaterial(tempLeftTire);
                AddTireMaterial(tempRightTire);
            }
        }
    }

    public void RemoveOriginalMatDirectory()
    {
        if (!Directory.Exists(originalMatDirectory))
            return;

        var fileInfo = new DirectoryInfo(originalMatDirectory);
        var files = fileInfo.GetFiles();
        foreach (var file in files)
            file.Delete();

        Directory.Delete(originalMatDirectory);
    }

    public void UpdateColors(VariationSetup variation, bool overrideOtherPickers)
    {
        var defaultColor = new Color();

        if (body.GetComponent<MeshRenderer>() == null)
            Debug.LogError("No MeshRenderer component");

        if ((variation.RideColor != body.GetComponent<MeshRenderer>().sharedMaterial.color && variation.RideColor != defaultColor))
        {
            VariationSetup.RideColor = variation.RideColor;
            var othersParent = transform.FindChild("FWD").GetChild(0).GetChild(0);

            for (int i = 0; i < othersParent.childCount; i++)
            {
                var target = othersParent.GetChild(i).GetComponent<MeshRenderer>();
                if (target.sharedMaterials.Length > 0 && target.sharedMaterials[0] != null && !target.name.Contains("tire") && !target.name.Contains("driver") && !target.name.Contains("interior") && !target.name.Contains("glass"))
                {
                    target.sharedMaterial.color = VariationSetup.RideColor;
                    if (overrideOtherPickers)
                        SetOtherPickers(ref variation);
                }
            }
        }
        CompareColors(variation.FrontBumperColor, frontBumper);
        CompareColors(variation.RearBumperColor, rearBumper);
        CompareColors(variation.HoodColor, hood);
        CompareColors(variation.SpoilerColor, spoiler);
        ChangeDecal(variation.UsedDecal, body);
        ChangeDecal(variation.UsedDecal, frontBumper);
        ChangeDecal(variation.UsedDecal, rearBumper);
        ChangeDecal(variation.UsedDecal, hood);
    }

    public string GetVariationDirectory()
    {
        return variationParentDirectory + "/" + gameObject.name.Substring(4);
    }

    void ChangeDecal(Decals decal, GameObject body)
    {
        var usedRend = body.GetComponent<Renderer>();
        usedRend.sharedMaterial.SetFloat("_Decals", 0);

        if (decal == Decals.A)
            usedRend.sharedMaterial.SetTextureOffset("_DecalTex", new Vector2(0f, 0f));
        if (decal == Decals.B)
            usedRend.sharedMaterial.SetTextureOffset("_DecalTex", new Vector2(0f, 0.5f));
        if (decal == Decals.C)
            usedRend.sharedMaterial.SetTextureOffset("_DecalTex", new Vector2(0.5f, 0f));
        if (decal == Decals.D)
            usedRend.sharedMaterial.SetTextureOffset("_DecalTex", new Vector2(0.5f, 0.5f));

        usedRend.sharedMaterial.SetFloat("_Decals", 1);
    }

    void CreateVariationDirectory(string originalMatPath)
    {
        var parentDir = originalMatPath.Split('/');
        var indexOfMat = originalMatPath.IndexOf(parentDir[parentDir.Length - 1]);
        variationParentDirectory = originalMatPath.Substring(0, indexOfMat - 1) + "/" + gameObject.name.Substring(4);
        if (!Directory.Exists(variationParentDirectory))
            Directory.CreateDirectory(variationParentDirectory);
    }

    void SetOtherPickers(ref VariationSetup variationInfo)
    {
        variationInfo.FrontBumperColor = variationInfo.RideColor;
        variationInfo.RearBumperColor = variationInfo.RideColor;
        variationInfo.HoodColor = variationInfo.RideColor;
        variationInfo.SpoilerColor = variationInfo.RideColor;
    }

    void CompareColors(Color newColor, GameObject part)
    {
        if (part == null || newColor == part.GetComponent<MeshRenderer>().sharedMaterial.color)
            return;
        else
            SetColor(part, newColor);
    }

    void SetMaterialsPath(string originalPath)
    {
        originalMatDirectory = originalPath;
        materialsDirectory = variationParentDirectory + "/" + "mat_" + gameObject.name.Substring(4);
    }

    void CreateMaterialsDirectory()
    {
#if UNITY_EDITOR
        if (!Directory.Exists(materialsDirectory))
        {
            FileUtil.CopyFileOrDirectory(originalMatDirectory, materialsDirectory);
            var parentDirectory = new DirectoryInfo(materialsDirectory);
            var fileInfo = parentDirectory.GetFiles();

            foreach (var file in fileInfo)
            {
                var partNameSplited = file.Name.Split('_');
                if (partNameSplited.Length > 3)
                {
                    var partNameStartIndex = file.Name.IndexOf(partNameSplited[3]);
                    var partName = parentDirectory.Name + "_" + file.Name.Substring(partNameStartIndex);
                    var sourcePath = file.FullName.Replace(@"\", "\\\\");
                    var destinationPath = (parentDirectory.FullName + @"\" + partName).Replace(@"\", "\\\\");
                    if (!File.Exists(destinationPath))
                        File.Move(sourcePath, destinationPath);
                }
            }
        }
#endif
    }

    void AddTireMaterial(GameObject tire)
    {
#if UNITY_EDITOR
        var tirePath = tireMaterialsPath + "mat_" + tire.name.Substring(4) + ".mat";
        if (tire.GetComponent<MeshRenderer>() != null)
            tire.GetComponent<MeshRenderer>().material = AssetDatabase.LoadAssetAtPath<Material>(tirePath);
#endif
    }

    void GetPartMaterial(GameObject part, int skipToIndex, string matName = null)
    {
#if UNITY_EDITOR
        var geoToMat = "mat" + part.name.Substring(3);
        var fileInfo = new DirectoryInfo(materialsDirectory);
        var files = fileInfo.GetFiles();
        int foundIndex = 0;

        foreach (var file in files)
        {
            if (!file.Name.Contains("meta"))
            {
                var nameSplited = file.Name.Split('_');
                string classTversionRemoved = "";
                for (int i = 0; i < nameSplited.Length; i++)
                {
                    if (!(i == 3 || i == 4 || i == 5))
                    {
                        classTversionRemoved += nameSplited[i];
                        if (i < nameSplited.Length - 1)
                            classTversionRemoved += "_";
                    }
                }
                if (classTversionRemoved.Contains(geoToMat))
                {
                    if (foundIndex == skipToIndex)
                    {
                        var matPath = GetPrefabAssetPath(file.FullName);
                        if (part.GetComponent<MeshRenderer>() != null)
                        {
                            var tempMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                            part.GetComponent<MeshRenderer>().material = tempMat;
                        }
                        return;
                    }
                    foundIndex++;
                }
            }
        }
#endif
    }

    void AddMaterial(GameObject part, string matName = null)
    {
#if UNITY_EDITOR
        string matPath = materialsDirectory + "/";
        matPath += matName == null ? GetVariationMaterialName(part.name) : matName;
        if (part.GetComponent<MeshRenderer>() != null)
            part.GetComponent<MeshRenderer>().material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
#endif
    }

    string GetVariationMaterialName(string geoPartName)
    {
        var geoNameSplited = geoPartName.Split('_');
        var indexOfPart = geoPartName.IndexOf(geoNameSplited[3]);
        var toReturn = "mat_" + gameObject.name.Substring(4) + "_" + geoPartName.Substring(indexOfPart) + ".mat";
        return toReturn;
    }

    string GetPrefabAssetPath(string fileInfo)
    {
        var toRet = fileInfo.Replace("\\", "/");
        var indexOfAsset = toRet.IndexOf("Assets");
        var toReturn = toRet.Substring(indexOfAsset);
        return toReturn;
    }

    GameObject ReplacePart(GameObject oldPart, GameObject newPart, Color partColor, GameObject partParent = null, PartSetup partSetup = null, bool getMaterial = true)
    {
        if (oldPart != null)
            DestroyImmediate(oldPart);
        if (newPart == null)
            return null;
        var tempPart = Instantiate(newPart);
        RemoveCloneSufix(tempPart);
        int skipToIndex = 0;
        if (partSetup != null)
            skipToIndex = (int)partSetup.UsedMaterial;
        if (getMaterial)
            GetPartMaterial(tempPart, skipToIndex);
        if (partParent == null)
            tempPart.transform.SetParent(gameObject.transform, false);
        else
            tempPart.transform.SetParent(partParent.transform, false);
        if (!tempPart.name.Contains("axle"))
            tempPart.transform.localPosition = new Vector3(0, 0, 0);
        SetColor(tempPart, partColor);
        return tempPart;
    }

    void SetColor(GameObject part, Color newColor)
    {
        if (part == null)
            return;
        var targetComponent = part.transform.GetComponent<MeshRenderer>();
        if (targetComponent.sharedMaterials.Length > 0 && targetComponent.sharedMaterials[0] != null && !targetComponent.name.Contains("tire"))
            part.GetComponent<MeshRenderer>().sharedMaterial.color = newColor;
    }

    void RemoveCloneSufix(GameObject passedObject)
    {
        passedObject.name = passedObject.name.Substring(0, passedObject.name.Length - 7);
    }

    void ReplaceTires(VariationSetup variation)
    {
        for (int i = 0; i < 2; i++)
        {
            DestroyImmediate(frontAxle.transform.GetChild(0).GetChild(i).GetChild(0).gameObject);
            DestroyImmediate(backAxle.transform.GetChild(0).GetChild(i).GetChild(0).gameObject);
            var tempLeft = Instantiate(availableCarParts.Tires[(int)variation.Tires.UsedPart].PartObject);
            var tempRight = Instantiate(availableCarParts.Tires[(int)variation.Tires.UsedPart].PartObject);
            tempLeft.transform.SetParent(frontAxle.transform.GetChild(0).GetChild(i));
            tempRight.transform.SetParent(backAxle.transform.GetChild(0).GetChild(i));
            RemoveCloneSufix(tempLeft);
            RemoveCloneSufix(tempRight);
            AddTireMaterial(tempLeft);
            AddTireMaterial(tempRight);
            tempLeft.transform.localPosition = new Vector3(0, 0, 0);
            tempRight.transform.localPosition = new Vector3(0, 0, 0);
        }
    }
}
