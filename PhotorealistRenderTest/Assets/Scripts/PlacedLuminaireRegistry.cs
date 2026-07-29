using System.Collections.Generic;
using UnityEngine;

public class PlacedLuminaireRegistry : MonoBehaviour
{
    public static PlacedLuminaireRegistry Instance;

    private List<GameObject> placedLuminaires = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    public void Register(GameObject go)
    {
        placedLuminaires.Add(go);
    }

    public void Unregister(GameObject go)
    {
        placedLuminaires.Remove(go);
    }

    public List<GameObject> GetAll() => placedLuminaires;

    public void RemoveLast()
    {
        if (placedLuminaires.Count == 0) return;
        GameObject last = placedLuminaires[placedLuminaires.Count - 1];
        placedLuminaires.Remove(last);
        Destroy(last);
    }

    public void RemoveSpecific(GameObject go)
    {
        if (placedLuminaires.Contains(go))
        {
            placedLuminaires.Remove(go);
            Destroy(go);
        }
    }

    public void ClearAll()
    {
        foreach (var go in placedLuminaires)
            Destroy(go);
        placedLuminaires.Clear();
    }
}