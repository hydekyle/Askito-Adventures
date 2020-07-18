using System.Collections;
using Assets.FantasyHeroes.Scripts;
using System.Collections.Generic;
using UnityEngine;

public class EquipManager : MonoBehaviour
{
    public static EquipManager Instance;

    public List<string> skinNames;
    public Dictionary<string, List<Sprite>> dicSkins;

    private void OnValidate()
    {
        ReadAllResources();
    }

    private void Awake()
    {
        if (Instance != null) Destroy(this.gameObject);
        Instance = this;
#if !UNITY_EDITOR
        ReadAllResources();
#endif
    }

    public void ReadAllResources()
    {
        skinNames = new List<string>();
        dicSkins = new Dictionary<string, List<Sprite>>();
        var assets = Resources.LoadAll("Skins", typeof(Texture2D));

        for (var x = 0; x < assets.Length; x++) skinNames.Add(assets[x].name); // Lee los conjuntos

        foreach (var skinName in skinNames)
        {
            List<Sprite> newList = new List<Sprite>();
            var skinSprites = Resources.LoadAll("Skins/" + skinName, typeof(Sprite));
            foreach (var spriteObj in skinSprites) newList.Add((Sprite)spriteObj);
            dicSkins[skinName] = newList;
        }
    }

    public void SetRandomEquipment(Character character)
    {
        List<Sprite> spriteList = GetRandomSkin();
        float colorValue = Random.Range(100f, 255f);
        Color skinColor = new Color(colorValue, colorValue, colorValue, 255f);
        SetSkinColor(character, skinColor);
        character.ArmorArmL = spriteList[0];
        character.ArmorArmR = spriteList[1];
        character.ArmorForearmL = spriteList[2];
        character.ArmorForearmR = spriteList[3];
        character.ArmorHandL = spriteList[4];
        character.ArmorHandR = spriteList[5];
        character.ArmorLeg = spriteList[6];
        character.ArmorPelvis = spriteList[7];
        character.ArmorShin = spriteList[8];
        character.ArmorTorso = spriteList[9];
        character.Initialize();
    }

    public void SetSkinColor(Character character, Color color)
    {
        character.HeadRenderer.color = color;
        character.ArmorArmLRenderer.color = color;
        character.ArmorPelvisRenderer.color = color;
        character.ArmorTorsoRenderer.color = color;
        foreach (var renderer in character.ArmorLegRenderers) renderer.color = color;
        foreach (var renderer in character.ArmorLegRenderers) renderer.color = color;
        foreach (var renderer in character.ArmorArmRRenderers) renderer.color = color;
        foreach (var renderer in character.ArmorForearmLRenderers) renderer.color = color;
        foreach (var renderer in character.ArmorForearmRRenderers) renderer.color = color;
        foreach (var renderer in character.ArmorHandLRenderers) renderer.color = color;
        foreach (var renderer in character.ArmorHandRRenderers) renderer.color = color;
        foreach (var renderer in character.ArmorShinRenderers) renderer.color = color;
    }

    List<Sprite> GetRandomSkin()
    {
        return dicSkins[skinNames[UnityEngine.Random.Range(0, skinNames.Count)]];
    }

}