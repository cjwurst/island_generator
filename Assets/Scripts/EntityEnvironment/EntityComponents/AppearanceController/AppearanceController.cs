using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AppearanceController : EntityComponent
{
    [SerializeField] Appearance appearance;

    void Start()
    {
        GetComponent<SpriteRenderer>().sprite = appearance.Sprite;
    }
}
