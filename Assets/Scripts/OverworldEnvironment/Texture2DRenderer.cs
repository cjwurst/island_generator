using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class Texture2DRenderer : MonoBehaviour
{
    [SerializeField] Material material;
    [SerializeField] Texture2DHolder textureHolder;

    void Awake()
    {
        GetComponent<MeshRenderer>().sharedMaterial = material;
        textureHolder.Subscribe((t) => material.SetTexture("_MainTex", t));
    }
}
