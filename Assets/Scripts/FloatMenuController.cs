using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FloatMenuController : MonoBehaviour
{
    public GameObject floatMenu;
    public GameObject objectStoreMenu;
    public GameObject menuCanvas;
    public Vector2 objectStoreMenuSize = new Vector2(450, 600);
    public Vector2 floatMenuSize = new Vector2(350, 400);

    // Start is called before the first frame update
    void Start()
    {
        ShowFloatMenu();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ChangeRoom()
    {
        SceneManager.LoadSceneAsync("Welcome", LoadSceneMode.Single);
    }

    public void ShowObjectStoreMenu()
    {
        floatMenu.SetActive(false);
        objectStoreMenu.SetActive(true);
        menuCanvas.GetComponent<RectTransform>().sizeDelta = objectStoreMenuSize;
    }

    public void ShowFloatMenu()
    {
        floatMenu.SetActive(true);
        objectStoreMenu.SetActive(false);
        menuCanvas.GetComponent<RectTransform>().sizeDelta = floatMenuSize;
    }
}