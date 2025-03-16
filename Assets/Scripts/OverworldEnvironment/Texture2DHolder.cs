using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewTexture2DHolder", menuName = "Holders/Texture 2D Holder")]
public class Texture2DHolder : ScriptableObject
{
    public Texture2D texture { get; private set; }

    Action<Texture2D> callback;

    public void Push(Texture2D _texture)
    {
        texture = _texture;
        callback.Invoke(texture);
    }

    // returns an unsubscribe action
    public Action Subscribe(Action<Texture2D> _callback)
    {
        callback += _callback;
        return () => callback -= _callback;
    }
}
