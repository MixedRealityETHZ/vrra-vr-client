using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Api.Models;
using Oculus.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomSelector : MonoBehaviour
{
    public static Room SelectedRoom;
    public GameObject roomListItem;
    public GameObject roomList;
    public ApiClient apiClient;

    // Start is called before the first frame update
    void Start()
    {
        StartLoadingRooms();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void StartLoadingRooms()
    {
        StartCoroutine(GetRooms());
    }

    IEnumerator GetRooms()
    {
        foreach (var child in roomList.GetComponentsInChildren<Transform>())
        {
            if (child != roomList.transform)
            {
                Destroy(child.gameObject);
            }
        }

        List<Room> rooms = null;
        yield return StartCoroutine(apiClient.GetRooms(res => rooms = res, err => Debug.Log(err)));
        foreach (var room in rooms)
        {
            var go = Instantiate(roomListItem, roomList.transform);
            go.transform.Find("Caption").GetComponent<TextMeshProUGUI>().text = room.Name;
            var toggle = go.transform.Find("Toggle").GetComponent<ToggleDeselect>();
            toggle.group = roomList.GetComponent<ToggleGroup>();
            toggle.onValueChanged.AddListener((b) =>
            {
                if (!b) return;
                StartCoroutine(SwitchRoom(room));
            });
            if (room.ThumbnailAssetId.HasValue)
            {
                var image = toggle.transform.Find("Image").GetComponent<Image>();
                StartCoroutine(apiClient.DownloadSprite(room.ThumbnailAssetId.Value, sprite => image.sprite = sprite,
                    err => Debug.Log(err)));
            }
        }
    }

    IEnumerator SwitchRoom(Room room)
    {
        SelectedRoom = room;
        yield return SceneManager.LoadSceneAsync("Room", LoadSceneMode.Single);
    }
}