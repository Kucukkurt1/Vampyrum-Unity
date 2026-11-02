using System;

[Serializable]
public class MaterialRequirement
{
    public string MaterialId;
    public string DisplayName;
    public int Amount;
}

[Serializable]
public class PotionDefinition
{
    public string Id;
    public string DisplayName;
    public string Description;
    public int GoldCost;
    public MaterialRequirement[] Materials;
}
