using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISelecteble
{
    void Select();
    void Deselect();

    bool IsSelected { get; }
    GameObject SelectableGameObject { get; }
}
