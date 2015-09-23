using UnityEngine;
using System.Collections.Generic;

public class Gyzmo : MonoBehaviour
{
    public Transform XAnchor;
    public Transform YAnchor;
    public Transform BoxAnchor;

    private bool _moving = false;
    private Transform _anchor = null;
    private Vector2 _start;

    public void UpdatePosition(IEnumerable<Vertex> vertexes)
    {
        float x = 0f;
        float y = 0f;
        int count = 0;
        foreach (var vertex in vertexes)
        {
            x += vertex.transform.position.x;
            y += vertex.transform.position.y;
            count++;
        }
        if (count != 0)
        {
            transform.position = new Vector3(x, y, count) / count;
            gameObject.SetActive(true);
        }
        else
            gameObject.SetActive(false);
    }

    public void BeginMoving(Transform anchor, Vector2 pos)
    {
        _anchor = anchor;
        _start = pos;
        _moving = true;
    }

    public Vector2 ContinueMoving(Vector2 pos)
    {
        if (!_moving) return Vector2.zero;
        Vector2 offset = pos - _start;
        offset = new Vector2(_anchor == YAnchor ? 0f : offset.x, _anchor == XAnchor ? 0f : offset.y);
        _start = pos;
        return offset;
    }

    public void EndMoving()
    {
        _moving = false;
        _anchor = null;
    }

}
