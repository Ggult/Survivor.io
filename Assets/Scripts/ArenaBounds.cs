using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class ArenaBounds : MonoBehaviour
{
    private Collider arenaCollider;

    public Bounds WorldBounds => GetArenaCollider().bounds;

    private Collider GetArenaCollider()
    {
        if (arenaCollider == null)
        {
            arenaCollider = GetComponent<Collider>();
        }

        return arenaCollider;
    }
}