using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[CreateAssetMenu(fileName = "NewLuminaire", menuName = "Lighting/Luminaire Profile")]
public class LuminaireProfile : ScriptableObject
{
    public string luminaireName = "Light";
    public GameObject lightPrefab;
    public Texture2D thumbnail;
    [TextArea] public string description;
}