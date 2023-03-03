using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Runtime;
using UnityEngine.Events;

[System.Serializable]
public struct ModelData
{
    public string modelName;
    public string modelURL;
    public string imagePath;
    public float scale;
}
[System.Serializable]
public class ModelLibrary
{
    public List<ModelData> modelLibrary;
}
public class InstantiateAssetCards : MonoBehaviour
{
    public ModelButtonList mbl;
    public GameObject cardPrefab;
    public GameObject loadScreen;
    void Start()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("modelLibrary");
        string jsonText = jsonFile.text;
        ModelLibrary modelLibrary = JsonUtility.FromJson<ModelLibrary>(jsonText);
        UnityAction onAssetCardClicked = () => { loadScreen.gameObject.SetActive(true); };
        UnityAction onAssetLoaded = () => { loadScreen.gameObject.SetActive(false); };

        foreach (ModelData modelData in modelLibrary.modelLibrary)
        {
            GameObject cardObject = Instantiate(cardPrefab, transform);
            ModelAssetCard card = cardObject.GetComponent<ModelAssetCard>();
            card.mbl = mbl;
            card.name = modelData.modelName;
            card.url = modelData.modelURL;
            card.onAssetCardClicked = onAssetCardClicked;
            card.onAssetLoaded = onAssetLoaded;
            card.scale = modelData.scale;

            card.Instantiate();
            // Load the sprite for the card's image and assign it to the card's 'sprite' field
            // ...
        }
    }

  
}
