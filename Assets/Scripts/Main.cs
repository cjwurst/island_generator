using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    EntityEnvironment environment;
    CellGrid grid;

    [SerializeField] GameObject spawner;
    [SerializeField] GameObject player;

    private void Start()
    {
        grid = new CellGrid(1f, new Vector2(-4f, -7.5f), Vector2Int.zero, new Vector2Int(8, 15));
        environment = new EntityEnvironment(grid);

        GetComponent<InputListener>().Init(grid, environment);

        environment.CreateEntity(spawner, Vector2Int.zero);
        environment.CreateEntity(player, Vector2Int.zero);
    }
}
