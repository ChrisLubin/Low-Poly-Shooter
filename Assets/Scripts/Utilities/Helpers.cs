using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

    public static T GetRandomElement<T>(this T[] array) => array[UnityEngine.Random.Range(0, array.Length - 1)];

    public static bool WillCollide(Vector3 startPosition, Vector3 endPosition, out Vector3 collidePosition, out GameObject collideObject)
    {
        collidePosition = Vector3.zero;
        collideObject = default(GameObject);
        Debug.DrawLine(startPosition, endPosition, Color.black, 2f);

        if (Physics.Linecast(startPosition, endPosition, out RaycastHit hit))
        {
            collidePosition = hit.point;
            Debug.DrawLine(startPosition, hit.point, Color.red, 2f);
            collideObject = hit.transform.gameObject;
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

    public static bool TrySphereCastAll<T>(Vector3 position, float radius, out CastAllData<T>[] castAllData, string layerName)
    {
        castAllData = new CastAllData<T>[0];
        RaycastHit[] hits = Physics.SphereCastAll(position, radius, Vector3.forward, 0.0001f, LayerMask.GetMask(layerName));

        if (hits.Length == 0)
            return false;

        List<CastAllData<T>> castAllDataList = new();

        foreach (RaycastHit hit in hits)
        {
            T hitObject = hit.collider.GetComponentInParent<T>();
            if (hitObject == null && !hit.collider.TryGetComponent(out hitObject)) { continue; }

            castAllDataList.Add(new(hit.point, hitObject));
        }

        if (castAllDataList.Count() == 0)
            return false;

        castAllData = castAllDataList.ToArray();
        return true;
    }

    public static string SplitCamelCase(string original, string joinBy = " ") => string.Join(joinBy, Regex.Split(original, @"(?<!^)(?=[A-Z](?![A-Z]|$))"));
}

public struct CastAllData<T>
{
    public Vector3 HitPosition;
    public T HitObject;

    public CastAllData(Vector3 hitPosition, T hitObject)
    {
        this.HitPosition = hitPosition;
        this.HitObject = hitObject;
    }
}
