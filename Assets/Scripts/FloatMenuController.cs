using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FloatMenuController : MonoBehaviour
{
    public GameObject floatMenu;
    public GameObject objectStoreMenu;
    public GameObject menuCanvas;
    public ModelSelector modelSelector;
    public Vector2 objectStoreMenuSize = new Vector2(450, 600);
    public Vector2 floatMenuSize = new Vector2(350, 400);
    public GameObject cameraRig;
    public float moveSpeed = 0.1f;

    private class MenuState
    {
        public GameObject menu;
        public Vector2 size;

        public MenuState(GameObject menu, Vector2 size)
        {
            this.menu = menu;
            this.size = size;
        }
    }

    private Stack<MenuState> _menuStack = new();

    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in menuCanvas.transform)
        {
            child.gameObject.SetActive(false);
        }

        PushMenu("Main", floatMenuSize);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void PushMenu(string name, Vector2 size)
    {
        if (_menuStack.Count > 0)
        {
            var top = _menuStack.Peek();
            top.menu.SetActive(false);
        }

        var rect = menuCanvas.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        
        var menu = menuCanvas.transform.Find(name).gameObject;
        _menuStack.Push(new MenuState(menu, size));
        menu.SetActive(true);
    }

    public void PopMenu()
    {
        if (_menuStack.Count > 0)
        {
            var top = _menuStack.Pop();
            top.menu.SetActive(false);
        }

        if (_menuStack.Count > 0)
        {
            var top = _menuStack.Peek();
            var rect = menuCanvas.GetComponent<RectTransform>();
            rect.sizeDelta = top.size;
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            top.menu.SetActive(true);
        }
    }

    public void ChangeRoom()
    {
        SceneManager.LoadSceneAsync("Welcome", LoadSceneMode.Single);
    }

    public void ShowObjectStoreMenu()
    {
        PushMenu("ObjectStore", objectStoreMenuSize);
        modelSelector.StartGettingModels();
    }

    public void ShowDeleteMenu()
    {
        PushMenu("Delete", floatMenuSize);
    }
    
    public void ShowMoveMenu()
    {
        PushMenu("Move", floatMenuSize);
    }
    
    public void MoveForward()
    {
        cameraRig.transform.position += Vector3.forward * moveSpeed;
    }
    
    public void MoveBackward()
    {
        cameraRig.transform.position += Vector3.back * moveSpeed;
    }
    
    public void MoveLeft()
    {
        cameraRig.transform.position += Vector3.left * moveSpeed;
    }
    
    public void MoveRight()
    {
        cameraRig.transform.position += Vector3.right * moveSpeed;
    }
    
    public void MoveUp()
    {
        cameraRig.transform.position += Vector3.up * moveSpeed;
    }
    
    public void MoveDown()
    {
        cameraRig.transform.position += Vector3.down * moveSpeed;
    }
}