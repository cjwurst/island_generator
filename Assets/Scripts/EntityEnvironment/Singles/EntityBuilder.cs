using System;
using System.Reflection;
using UnityEngine;
using UnityEditor;

public class EntityBuilder
{
    readonly CallbackDirector director;
    readonly CellGrid grid;
    readonly PathFinder pathFinder;

    public EntityBuilder(CallbackDirector _director, CellGrid _grid, PathFinder _pathFinder, InvertibleInvoker _invoker)
    {
        director = _director;
        grid = _grid;
        pathFinder = _pathFinder;
    }

    public bool TryInstantiateEntity(GameObject prefab, Vector2Int cell)
    {
        var args = new EntitiesAtRequestedArgs(cell);
        director.RaiseEntitiesAtRequested(args);
        var mover = prefab.GetComponent<Mover>();
        if (mover != null && mover.IsObstruction && args.IncludesObstruction)
            return false;

        var entity = UnityEngine.Object.Instantiate(prefab, grid.CellToWorld(cell), Quaternion.identity);
        foreach (EntityComponent component in entity.GetComponents<EntityComponent>())
            component.Init(grid, director, pathFinder, this);
        return true;
    }

    public void AddComponentToEntity<T>(GameObject entity) where T : EntityComponent
    {
        var component = entity.AddComponent<T>();
        component.Init(grid, director, pathFinder, this);
    }

    #if UNITY_EDITOR
    [MenuItem("Assets/Create/Entity Toolbox/Empty Entity")]
    static void CreateEmptyPrefab() { CreateEntityPrefab(); }

    [MenuItem("Assets/Create/Entity Toolbox/Effect Entity")]
    static void CreateEffectPrefab()
    {
        CreateEntityPrefab
        (
            typeof(AppearanceController), 
            typeof(Mover)
        );
    }

    [MenuItem("Assets/Create/Entity Toolbox/Creature Entity")]
    static void CreateCreaturePrefab()
    {
        CreateEntityPrefab
        (
            typeof(AppearanceController),
            typeof(Mover),
            typeof(StatController)
        );
    }

    static void CreateEntityPrefab(params Type[] components)
    {
        GameObject emptyEntity = new GameObject("NewEntityPrefab");
        foreach (Type type in components)
            emptyEntity.AddComponent(type);

        Type projectWindowUtilType = typeof(ProjectWindowUtil);
        MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);
        object obj = getActiveFolderPath.Invoke(null, new object[0]);
        string localPath = $"{obj.ToString()}/{emptyEntity.name}.prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);

        PrefabUtility.SaveAsPrefabAsset(emptyEntity, localPath);
        GameObject.DestroyImmediate(emptyEntity);
    }
    #endif
}