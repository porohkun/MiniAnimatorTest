using UnityEngine;
using System.Collections.Generic;

public class Vertex : MonoBehaviour
{
    static int _lastId = 0;
    private static int NextId()
    {
        _lastId++;
        return _lastId;
    }

    public static Vertex Prefab { get; private set; }
    static Vertex()
    {
        Prefab = Resources.Load<Vertex>("Prefabs/Vertex");
    }

    public Material Normal;
    public Material Selected;
    public Material Key;
    public Material KeySelected;

    private MeshRenderer _renderer;
    private bool _selected = false;
    private int _lastFrame = 0;

    public int Id { get; private set; }
    List<Vector2> _positions = new List<Vector2>();
    List<bool> _keyFrames = new List<bool>();
    public int Frames { get; private set; }

    public void Init(int frames, int frame, Transform parent, Vector2 position)
    {
        Init();
        transform.SetParent(parent);
        Id = NextId();
        while (Frames != frames)
            InsertFrame(Frames);
        this[frame] = position;
        ShowFrame(frame);
    }

    public void Init()
    {
        _renderer = GetComponent<MeshRenderer>();
    }

    public Vector2 this[int frame]
    {
        get
        {
            return _positions[frame];
        }
        set
        {
            _positions[frame] = value;
            bool full = !_keyFrames[frame];
            _keyFrames[frame] = true;
            RecalculateBetween(GetPrevKeyFrame(frame), frame, full);
            RecalculateBetween(frame, GetNextKeyFrame(frame), full);
        }
    }

    int GetPrevKeyFrame(int frame)
    {
        for (int i = GetPrevFrame(frame); i != frame; i = GetPrevFrame(i))
            if (_keyFrames[i]) return i;
        return frame;
    }

    int GetPrevFrame(int frame) { return frame == 0 ? Frames - 1 : frame - 1; }

    int GetNextKeyFrame(int frame)
    {
        for (int i = GetNextFrame(frame); i != frame; i = GetNextFrame(i))
            if (_keyFrames[i]) return i;
        return frame;
    }

    int GetNextFrame(int frame) { return frame == Frames - 1 ? 0 : frame + 1; }

    int FramesBetween(int start, int end)
    {
        if (start < end) return end - start;
        if (start > end) return Frames - start + end;
        return Frames;
    }

    void RecalculateBetween(int start, int end, bool full = false)
    {
        //if (start == end && !full) return;
        Vector2 atStart = _positions[start];
        Vector2 atEnd = _positions[end];
        int count = FramesBetween(start, end);
        Vector2 offset = (atEnd - atStart) / count;
        Vector2 last = atStart;
        for (int i = GetNextFrame(start); i != end; i = GetNextFrame(i))
        {
            last += offset;
            _positions[i] = last;
        }
    }

    public void ShowFrame(int frame)
    {
        transform.position = this[frame];
        _lastFrame = frame;
        UpdateMaterial();
    }

    public void InsertFrame(int atPosition)
    {
        Frames++;
        _positions.Insert(atPosition, Vector2.zero);
        _keyFrames.Insert(atPosition, false);
        RecalculateBetween(GetPrevKeyFrame(atPosition), GetNextKeyFrame(atPosition), true);
    }

    public void RemoveFrame(int frame)
    {
        Frames--;
        _positions.RemoveAt(frame);
        _keyFrames.RemoveAt(frame);
        RecalculateBetween(GetPrevKeyFrame(frame), GetNextKeyFrame(frame - 1), true);
    }

    public void RemoveValueAt(int frame)
    {
        int prev = GetPrevKeyFrame(frame);
        int next = GetNextKeyFrame(frame);
        if (prev!=frame)
        {
            _keyFrames[frame] = false;
            RecalculateBetween(prev, next);
        }
    }

    public void Select(bool select)
    {
        _selected = select;
        UpdateMaterial();
    }

    public void UpdateMaterial()
    {
        _renderer.material = _keyFrames[_lastFrame] ? (_selected ? KeySelected : Key) : (_selected ? Selected : Normal);
    }

    public void WriteTo(System.IO.BinaryWriter writer)
    {
        writer.Write(Id);
        writer.Write(Frames);
        writer.Write(_keyFrames.FindAll(frame => frame).Count);
        for (int i = 0; i < Frames; i++)
        {
            if (_keyFrames[i])
            {
                writer.Write(i);
                writer.Write(_positions[i].x);
                writer.Write(_positions[i].y);
            }
        }
    }

    public static Vertex ReadFrom(System.IO.BinaryReader reader)
    {
        var vertex = GameObject.Instantiate<Vertex>(Vertex.Prefab);
        vertex.Init();
        int id = reader.ReadInt32();
        vertex.Id = id;
        _lastId = System.Math.Max(_lastId, id);
        int frames = reader.ReadInt32();
        int keyFrames = reader.ReadInt32();
        while (vertex.Frames != frames)
            vertex.InsertFrame(vertex.Frames);
        for (int i = 0; i < keyFrames; i++)
        {
            int index = reader.ReadInt32();
            float x = reader.ReadSingle();
            float y = reader.ReadSingle();
            vertex[index] = new Vector2(x, y);
        }
        return vertex;
    }

}
