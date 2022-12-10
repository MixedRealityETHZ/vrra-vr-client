using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Api.Models;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ModelSelector : MonoBehaviour
{
    public GameObject modelListItem;
    public GameObject modelList;
    public ApiClient apiClient;
    public RoomController roomController;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public Coroutine StartGettingModels()
    {
        return StartCoroutine(GetModels());
    }

    IEnumerator GetModels()
    {
        List<Model> models = null;
        yield return StartCoroutine(apiClient.GetModels(RoomSelector.SelectedRoom.Id, res => models = res,
            err => Debug.Log(err)));
        foreach (var model in models)
        {
            var go = Instantiate(modelListItem, modelList.transform);
            go.transform.Find("Caption").GetComponent<TextMeshProUGUI>().text = model.Name;
            var toggle = go.transform.Find("Toggle").GetComponent<ToggleDeselect>();
            toggle.group = modelList.GetComponent<ToggleGroup>();
            toggle.onValueChanged.AddListener((b) =>
            {
                if (!b) return;
                StartCoroutine(roomController.AddObject(model, toggle.transform));
            });
            if (model.ThumbnailAssetId.HasValue)
            {
                var image = toggle.transform.Find("Image").GetComponent<Image>();
                StartCoroutine(apiClient.DownloadSprite(model.ThumbnailAssetId.Value, sprite => image.sprite = sprite,
                    err => Debug.Log(err)));
            }
        }
    }
}