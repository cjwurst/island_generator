using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAppearance", menuName = "Entity Toolbox/Appearance")]
public class Appearance : ScriptableObject
{
    [SerializeField] Sprite sprite;
    public Sprite Sprite { get { return sprite; } }
}
