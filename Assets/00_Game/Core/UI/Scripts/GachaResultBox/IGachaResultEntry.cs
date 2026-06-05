using UnityEngine;

public interface IGachaResultEntry
{
    bool IsNew { get; }
    GameObject Spawn(Transform holder);
}