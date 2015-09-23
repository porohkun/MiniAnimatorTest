using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Animation
{
    public int Frames { get { return Vertexes.Count == 0 ? 0 : Vertexes[0].Frames; } }
    public List<Vertex> Vertexes { get; private set; }
    public List<Line> Lines { get; private set; }

    public Animation()
    {
        Vertexes = new List<Vertex>();
        Lines = new List<Line>();
    }

    public IEnumerable<Line> GetLines(List<Vertex> vertForRemove)
    {
        foreach (var line in Lines)
        {
            foreach (var vert in vertForRemove)
                if (line.Vertex1 == vert || line.Vertex2 == vert)
                { 
                    yield return line;
                    break;
                }
        }
    }

    /// <summary>
    /// Insert frame at selected position. Insert after end by default
    /// </summary>
    public void InsertFrame(int atPosition = -1)
    {
        if (atPosition == -1) atPosition = Frames;
        foreach (var vertex in Vertexes)
            vertex.InsertFrame(atPosition);
    }

    public void RemoveFrame(int frame)
    {
        foreach (var vertex in Vertexes)
            vertex.RemoveFrame(frame);
    }

    public void SaveTo(string filename)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filename));
        using (var writer = new BinaryWriter(System.IO.File.Create(filename)))
        {
            writer.Write(Vertexes.Count);
            foreach (var vertex in Vertexes)
                vertex.WriteTo(writer);
            writer.Write(Lines.Count);
            foreach (var line in Lines)
                line.WriteTo(writer);
        }
    }

    public static Animation OpenFrom(string filename)
    {
        Animation result = new Animation();
        using (var reader = new BinaryReader(File.OpenRead(filename)))
        {
            int vertexCount = reader.ReadInt32();
            Dictionary<int, Vertex> vertexDict = new Dictionary<int, Vertex>();
            for (int i = 0; i < vertexCount; i++)
            {
                var vertex = Vertex.ReadFrom(reader);
                result.Vertexes.Add(vertex);
                vertexDict.Add(vertex.Id, vertex);
            }

            int lineCount = reader.ReadInt32();
            for (int i = 0; i < lineCount; i++)
                result.Lines.Add(Line.ReadFrom(reader, vertexDict));
        }
        return result;
    }
}
