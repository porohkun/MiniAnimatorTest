using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Core : MonoBehaviour
{
    public static Core Glob { get; private set; }
    public Canvas UiCanvas;
    public Camera Camera;

    public Transform Field;
    public Transform[] Border;
    public Gyzmo Gyzmo;

    public ScenesManager Scenes;

    void Start()
    {
        Glob = this;
        Scenes = new ScenesManager();
        Scenes.Push(MainScene.GetInstance(this));
        ZoomScale = 1f;
    }

    public void MoveCamera(Vector2 offset)
    {
        float x = Camera.transform.position.x + offset.x;
        float y = Camera.transform.position.y + offset.y;

        Camera.transform.position = new Vector3(
            x < 0f ? 0f : (x > 64f ? 64f : x),
            y < 0f ? 0f : (y > 64f ? 64f : y),
            -10f);
    }

    public bool ZoomCamera(float zoom)
    {
        if ((Camera.orthographicSize < 15 && zoom < 1) || (Camera.orthographicSize > 35 && zoom > 1))
            return false;
        Camera.orthographicSize = Camera.orthographicSize * zoom;
        ZoomScale = ZoomScale * zoom;
        return true;
    }

    public float ZoomScale { get; private set; }

    void Update()
    {

    }

}