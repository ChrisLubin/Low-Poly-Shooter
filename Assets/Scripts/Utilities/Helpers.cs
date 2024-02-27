using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A static class for general helpful methods
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Destroy all child objects of this transform (Unintentionally evil sounding).
    /// Use it like so:
    /// <code>
    /// transform.DestroyChildren();
    /// </code>
    /// </summary>
    public static void DestroyChildren(this Transform t)
    {
        foreach (Transform child in t) Object.Destroy(child.gameObject);
    }

    public static T[] ToArray<T>(IReadOnlyList<T> readOnlyList)
    {
        List<T> list = new();

        foreach (T element in readOnlyList)
        {
            list.Add(element);
        }

        return list.ToArray<T>();
    }

    public static bool WillCollide(Vector3 startPosition, Vector3 endPosition, out Vector3 collidePosition)
    {
        collidePosition = Vector3.zero;
        Debug.DrawLine(startPosition, endPosition, Color.black, 2f);

        if (Physics.Linecast(startPosition, endPosition, out RaycastHit hit))
        {
            collidePosition = hit.point;
            Debug.DrawLine(startPosition, hit.point, Color.red, 2f);
            return true;
        }

        return false;
    }

    public static bool WillCollide<T>(Vector3 startPosition, Vector3 endPosition, out Vector3 collidePosition, out T collideObject, string layerName)
    {
        collidePosition = Vector3.zero;
        collideObject = default(T);
        Debug.DrawLine(startPosition, endPosition, Color.black, 2f);

        if (Physics.Linecast(startPosition, endPosition, out RaycastHit hit, LayerMask.GetMask(layerName)))
        {
            Debug.DrawLine(startPosition, hit.point, Color.red, 2f);
            collidePosition = hit.point;
            hit.collider.TryGetComponent(out collideObject);
            return true;
        }

        return false;
    }
}
