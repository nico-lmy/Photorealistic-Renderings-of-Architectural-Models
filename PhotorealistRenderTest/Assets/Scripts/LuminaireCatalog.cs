using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LuminaireCatalog", menuName = "Lighting/Luminaire Catalog")]
public class LuminaireCatalog : ScriptableObject
{
    public List<LuminaireProfile> luminaires;
}