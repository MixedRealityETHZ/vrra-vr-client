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
        StartCoroutine(GetRooms());
    }

    // Update is called once per frame
    void Update()
    {
    }

    IEnumerator GetRooms()
    {
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
        }
    }

    IEnumerator SwitchRoom(Room room)
    {
        SelectedRoom = room;
        yield return SceneManager.LoadSceneAsync("Room", LoadSceneMode.Single);
    }
}