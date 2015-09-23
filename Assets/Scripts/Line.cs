using UnityEngine;
using System.Collections.Generic;

public class Line : MonoBehaviour
{
    public Vertex Vertex1;
    public Vertex Vertex2;
    
    public static Line Prefab { get; private set; }
    static Line()
    {
        Prefab = Resources.Load<Line>("Prefabs/Line");
    }

    public void UpdatePosition()
    {
        Vector3 pos1 = Vertex1.transform.position;
        Vector3 pos2;
        if (Vertex2 == null)
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos2 = new Vector3(mousePos.x, mousePos.y, 0f);
        }
        else
            pos2 = Vertex2.transform.position;

        transform.position = (pos1 + pos2) / 2 + new Vector3(0f, 0f, 1f);
        transform.localScale = new Vector3(Vector3.Distance(pos1, pos2), 3f / 8f, 1f);
        if (pos1.y != pos2.y)
            transform.rotation = Quaternion.FromToRotation(new Vector3(1f, 0f, 0f), (pos2 - pos1).normalized);
        else
            transform.rotation = new Quaternion();
    }

    public void WriteTo(System.IO.BinaryWriter writer)
    {
        writer.Write(Vertex1.Id);
        writer.Write(Vertex2.Id);
    }

    public static Line ReadFrom(System.IO.BinaryReader reader, Dictionary<int, Vertex> dict)
    {
        var line = GameObject.Instantiate<Line>(Line.Prefab);
        line.Vertex1 = dict[reader.ReadInt32()];
        line.Vertex2 = dict[reader.ReadInt32()];
        return line;
    }
}
