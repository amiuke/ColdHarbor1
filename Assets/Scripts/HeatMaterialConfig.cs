using UnityEngine;

[CreateAssetMenu(fileName = "HeatMaterialConfig", menuName = "ScriptableObjects/HeatMaterialConfig")]
public class HeatMaterialConfig : ScriptableObject
{
    [Header("Materials")]
    public Material iceMaterial; // Blue material for ICE
    public Material ironMaterial; // Black material for IRON
    public Material flammableMaterial; // Yellow material for FLAMMABLE
    public Material heatedMaterial; // Red material for HEATED
    public Material ignitedMaterial; // Orange material for ignited FLAMMABLE
}
