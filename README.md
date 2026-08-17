# Visualiseur de lumières naturelles et artificielles pour l'architecture

Prototype Unity (HDRP) permettant d'explorer une maquette architecturale en vue subjective, d'y placer des luminaires, et d'obtenir des rendus photoréalistes accompagnés d'une analyse quantitative des contributions lumineuses (naturelles vs artificielles).

Développé dans le cadre du stage de Nicolas LAMY - 2026.

---

## 1. Prérequis

| Élément | Version / remarque |
|---|---|
| Unity | 6.0 |
| Render pipeline | **HDRP** - Path Tracing, RTGI indispensables |
| GPU | Compatible ray tracing (DXR), RTX 20xx minimum |
| Input System | Package `com.unity.inputsystem` (nouveau système) |
| Stereolab | Plugin `Stereolab.StereoProjection` pour le mode CAVE |

Le projet dépend du package **Stereolab** pour la stéréoscopie CAVE. Sans lui, seul le mode *Simple Camera* est fonctionnel.  

---

## 2. Vue d'ensemble

L'application fonctionne selon un principe de **rendu figé (freeze)** :

1. **Mode mouvement** : L'utilisateur se déplace librement dans la scène. Le rendu est temps réel, en RTGI (rapide, approximatif).
2. **Mode figé** : Sur déclenchement (touche Espace), la scène est rendue en **Path Tracing** par accumulation de samples. Le résultat est stocké dans des RenderTextures et affiché par-dessus la vue.
3. **Analyse** : Une fois figé, l'utilisateur peut cliquer sur un pixel pour lire la luminance, ou basculer sur une heatmap (touche H).

Ce fonctionnement en deux temps est une conséquence directe du coût du path tracing. Le RTGI sert de prévisualisation navigable, le path tracing de rendu de référence pour la mesure.

Pour séparer les contributions lumineuses, la scène est rendue **deux fois** : une fois avec les lumières artificielles désactivées (naturel seul), une fois avec tout. La contribution artificielle est obtenue par soustraction. C'est ce qui permet le calcul du ratio naturel/artificiel dans les sondes.

---

## 3. Architecture des scripts

### 3.1 Contrôleurs de rendu

**`SimpleController.cs`**
Gère le mode mono-caméra (écran classique). Détecte le mouvement du joueur, déclenche la capture path tracing, expose `frozenTexture` (naturel + artificiel) et `frozenTextureNat` (naturel seul).

**`StereoController.cs`**
Équivalent pour le mode CAVE : 3 caméras × 2 yeux. Gère l'alternance des yeux via `StereolabInstance.ForceEye()` (fonction ajoutée pour forcer le rendu dde l'oeil gauche, ne gène pas le fonctionnement du package).

Points clés :
- `CaptureRoutine()` : orchestre la séquence complète (RTGI rapide --> PT naturel --> PT complet, pour chaque œil)
- `maxSamples` / `safetyMargin` : nombre de samples accumulés. Plus élevé = moins de bruit, plus lent
- `movementThreshold` / `rotationThreshold` : sensibilité de détection du mouvement
- Tableaux de RenderTextures : `leftRT`, `rightRT` (complet), `leftRT_Nat`, `rightRT_Nat` (naturel), `leftRTGI`, `rightRTGI` (prévisualisation), `heatmapLeftRTs`, `heatmapRightRTs`

**Attention** : `OnDestroy()` libère toutes les RenderTextures. Si un nouveau tableau de RT est ajouté, penser à l'ajouter là aussi, sinon fuite mémoire GPU.

### 3.2 Éclairage naturel

**`SunController.cs`**
Positionne la Directional Light selon date, heure, latitude, longitude.

- Calcule la **déclinaison solaire**, l'**altitude** et l'**azimut** par les formules astronomiques standard
- `transform.rotation = Quaternion.Euler(sunAltitude, azimut, 0)`
- Interpole entre les heures des données PVGIS pour des transitions douces
- S'abonne à `PVGISManager.OnDataReceived` pour se mettre à jour quand les données arrivent

**`PVGISManager.cs`**
Interroge l'API PVGIS (base européenne d'irradiation solaire) pour récupérer une année météorologique type (TMY) à la position donnée.

Particularité : les noms de champs JSON de PVGIS contiennent des parenthèses (`time(UTC)`, `Gb(n)`, `Gd(h)`) que `JsonUtility` ne sait pas désérialiser. D'où le nettoyage par `Replace()` avant parsing :

```csharp
string cleanJson = rawJson.Replace("time(UTC)", "time")
                          .Replace("Gb(n)", "Gbn")
                          .Replace("Gd(h)", "Gdh");
```

Gestion d'erreurs : PVGIS refuse les positions hors couverture (océans, hautes latitudes) avec une `ProtocolError`. Le message d'erreur est extrait du JSON de réponse quand c'est possible.

### 3.3 Analyse de luminance

**`LuminanceAnalyzer.cs`**
Wrapper autour du compute shader LuminanceHeatmap.compute, se contente de préparer la RenderTexture de sortie, de pousser les paramètres au GPU et de lancer le dispatch.

`maxLuminance` et `minLuminance` sont pilotés en runtime par les sliders de l'onglet Settings du l'UI. 
Les modifier ne relance pas le rendu : seule la colorisation change, puisque le path tracing est déjà accumulé dans la RenderTexture source.

**`LuminanceHeatmap.compute`**
Compute shader convertissant une texture HDR en fausses couleurs.

```hlsl
float Y = dot(color, float3(0.2126, 0.7152, 0.0722)); 
float sceneLuminance = Y * exp2(_EV);                 
float t = (sceneLuminance - _MinLuminance) / (_MaxLuminance - _MinLuminance);
```

La palette (noir --> violet --> rouge orangé --> jaune) est interpolée par segments de 0.25. Pour la modifier, éditer `GetHeatmapColor()`.

**Important** : le facteur `exp2(_EV)` suppose que l'EV passé correspond bien à l'exposition appliquée par HDRP. Si le mode d'exposition dans le Volume est changé, cette conversion devient fausse.

**`LuminanceProbe.cs` / `SimpleLuminanceProbe.cs`**
Sondes ponctuelles. Au clic en mode figé :
1. Convertit la position souris en coordonnées UV
2. Lit le pixel dans `frozenTexture` et `frozenTextureNat`
3. Calcule `totalLuminance`, `naturalLuminance`, `artificialLuminance = total - natural`, et le `ratio`
4. Applique `calibrationFactor` pour passer des unités Unity aux cd/m²

**Le `calibrationFactor` (défaut : 1000) est empirique.** Il doit être recalibré si l'exposition ou les unités photométriques de la scène changent. 
Point faible de la chaîne de mesure : sans calibration contre une mesure physique réelle (luxmètre sur maquette, ou scène de référence normalisée), les valeurs en cd/m² sont indicatives et non métrologiques. À documenter clairement dans tout rendu de résultats.

### 3.4 Luminaires

**`LuminaireProfile.cs`** : ScriptableObject décrivant un luminaire : nom, prefab, vignette, description. Créer via `Assets > Create > Lighting > Luminaire Profile`.

**`LuminaireCatalog`** : ScriptableObject `List<LuminaireProfile>`, créable via *Assets --> Create --> Lighting --> Luminaire Catalog*
Source de données qui alimente la liste de luminaires de l'UI. Ajouter un modèle au projet = créer un `LuminaireProfile`, puis l'ajouter à la liste du catalogue.

**`LuminairePlacementController.cs`** : Gère le placement interactif d'un luminaire dans la scène, en mode aperçu suivi d'une confirmation.
Fonctionnement :
- `StartPlacement(profile)` : appelé par l'UI quand l'utilisateur choisit un luminaire dans la liste. Annule tout placement en cours, mémorise le profil et ferme le panneau UI.
- Chaque frame, un raycast est lancé depuis la caméra active vers l'avant (`cam.transform.forward`), donc au centre de l'écran, pas sous le curseur. Le premier impact filtré par `placementLayerMask` donne la position candidate.
- À la première intersection, le `lightPrefab` du profil est instancié comme aperçu (nommé `"… (Preview)"`), parenté à `lightsContainer` si renseigné, avec éventuellement un `previewMarkerPrefab` en enfant pour la visibilité.
- L'aperçu est repositionné à `hit.point + hit.normal * surfaceOffset` et orienté avec `Quaternion.LookRotation(hit.normal)`, ce qui le plaque perpendiculairement à la surface visée.
- Clic gauche confirme : le marqueur est détruit, l'objet est renommé au nom du profil et enregistré dans `PlacedLuminaireRegistry`. Échap annule et détruit l'aperçu.

Caméra de référence : `ActiveReferenceCamera` renvoie `simpleCam` ou `caveCenterCam` selon celle qui est active dans la hiérarchie — c'est ce qui rend le placement fonctionnel dans les deux modes de rendu sans code conditionnel ailleurs.
Si le panneau UI est ouvert (`uiController.IsPanelOpen`), l'aperçu est masqué et le raycast suspendu, pour éviter qu'il ne « suive » la caméra pendant qu'on manipule l'interface.

**`PlacedLuminaireRegistry.cs`**
Singleton (`Instance`) tenant la liste des luminaires instanciés à l'exécution.

### 3.5 Interface runtime

**`RuntimeUIController.cs`** : le plus gros fichier, entièrement en **IMGUI** (`OnGUI`).

Choix de l'IMGUI plutôt que de l'UI Toolkit / uGUI : simplicité de prototypage et surtout compatibilité avec l'affichage par-dessus les RenderTextures figées, qui sont elles-mêmes dessinées en `OnGUI`.

Structure :

```
Écran de choix de mode (Simple / CAVE)
  └─ Panneau (bouton burger)
       ├─ Onglet Settings
       │    ├─ Latitude / Longitude + "Load New Position"
       │    ├─ Heure (HH:MM), mois, jour, fuseau
       │    └─ Min / Max luminance (bornes heatmap)
       └─ Onglet Lights
            ├─ Catalogue (placement)
            ├─ Liste des luminaires placés (scrollable)
            └─ Éditeur du luminaire sélectionné
                 ├─ Transform (position / rotation / scale)
                 └─ Light (intensité, couleur/température, range, spot angle, on-off)
```

**`PlayerController.cs`**
Déplacement FPS via CharacterController + nouveau Input System. Désactivé quand `panelOpen` est vrai (voir `ApplyCursorState()`).

---

## 4. Contrôles

| Touche | Action |
|---|---|
| ZQSD / WASD | Déplacement |
| Souris | Regard |
| `Tab` | Ouvrir / fermer le panneau (`toggleKey`) |
| `Espace` | Forcer une capture (`captureKey`) |
| `Backspace` | Forcer le dégel (`forceUnfreezeKey`) |
| Clic gauche (figé) | Sonde de luminance |
| `H` (figé) | Affichage de la heatmap |
| Clic gauche (après `Place` dans onglet `Lights`) | Placement du luminaire |

---

## 7. Limitations connues

- Contrôles en mode CAVE non configurées sur les manettes
- Performance en déplacement en mode CAVE insuffisants
- Stéréoscopie en mode CAVE en image figé : besoin de perfectionnement pour alignement de l'oeil gauche et droit

