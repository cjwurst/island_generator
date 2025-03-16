using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputListener : MonoBehaviour
{
    Camera mainCamera;

    EntityEnvironment environment;
    CellGrid grid;

    bool isInitialized = false;

    static readonly (KeyCode code, Vector2Int v)[] directionalKeys = new (KeyCode, Vector2Int)[]
    {
        (KeyCode.Keypad6, Vector2Int.right),
        (KeyCode.Keypad9, Vector2Int.right + Vector2Int.up),
        (KeyCode.Keypad8, Vector2Int.up),
        (KeyCode.Keypad7, Vector2Int.up + Vector2Int.left),
        (KeyCode.Keypad4, Vector2Int.left),
        (KeyCode.Keypad1, Vector2Int.left + Vector2Int.down),
        (KeyCode.Keypad2, Vector2Int.down),
        (KeyCode.Keypad3, Vector2Int.down + Vector2Int.right)
    };

    void Start()
    {
        mainCamera = GetComponent<Camera>();
    }

    public void Init(CellGrid _grid, EntityEnvironment _environment)
    {
        grid = _grid;
        environment = _environment;

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) return;
        var mouseCell = grid.WorldToCell(mainCamera.ScreenToWorldPoint(Input.mousePosition));
        if (Input.GetMouseButtonDown(0))
            environment.director.RaiseLeftMouseClicked(new MouseClickedArgs(mouseCell));
        if (Input.GetMouseButtonDown(1))
            environment.director.RaiseRightMouseClicked(new MouseClickedArgs(mouseCell));
        if (Input.GetMouseButtonDown(2))
            environment.director.RaiseMiddleMouseClicked(new MouseClickedArgs(mouseCell));

        var directional = Vector2Int.zero;
        foreach(var directionalKey in directionalKeys)
        {
            if (Input.GetKeyDown(directionalKey.code)) 
            {
                directional = directionalKey.v;
                break;
            }
        }
        if (directional != Vector2Int.zero)
            environment.director.RaiseDirectionalKeyPressed(new DirectionalKeyPressedArgs(directional));

        if (Input.GetKeyDown(KeyCode.Backspace))
            environment.director.RaiseReverseRoundRequested(1);

        environment.director.RaiseUpdated();
    }
}
