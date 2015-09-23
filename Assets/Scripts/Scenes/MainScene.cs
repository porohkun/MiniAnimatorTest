using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainScene : SceneBase
{
    public float CameraSpeed = 100f;
    public int __frame = 0;
    public int __frames = 5;
    public int Frame { get { return __frame; } set { __frame = value; UpdateFramesText(); } }
    public int Frames { get { return __frames; } set { __frames = value; UpdateFramesText(); } }
    
    private void UpdateFramesText()
    {
        FrameText.text = (Frame + 1).ToString();
        FramesText.text = string.Format("/{0}", Frames);
    }


    public Text FrameText;
    public Text FramesText;
    public ToggleGroup Tools;
    public InputField FrameDelay;
    public Toggle PlayButton;

    private string _currentTool = "Cursor";
    private EventSystem _eventSystem;
    private InputController _input;
    private Transform _field;
    private Gyzmo _gyzmo;
    private Animation _animation = new Animation();
    private bool _play = false;
    private float _lastTick = 0f;

    void Start()
    {
        UpdateFramesText();
    }

    public static MainScene GetInstance(Core core)
    {
        var scene = Instantiate<MainScene>(core);

        scene._eventSystem = EventSystem.current;
        scene._field = core.Field;
        scene._gyzmo = core.Gyzmo;
        scene._input = scene.GetComponent<InputController>();
        scene._input.SceneOwner = scene;
        scene._input["Fire1"].Press += scene.MouseLeftPress;
        scene._input["Fire1"].Release += scene.MouseLeftRelease;
        scene._input["Fire2"].FirstClick += scene.MouseRightClick;
        var t = scene._input["Shift"];
        scene._input["Delete"].FirstClick += scene.DeleteClick;
        scene._input["Preview"].Click += scene.OnPreviewFrame;
        scene._input["Next"].Click += () => { scene.OnNextFrame(false); };
        scene._input["Key"].Click += scene.RemoveKeyFrameOnSelected;
        scene._input["Tool1"].FirstClick += () => { scene.Tools.transform.GetChild(0).GetComponent<Toggle>().isOn = true; };
        scene._input["Tool2"].FirstClick += () => { scene.Tools.transform.GetChild(1).GetComponent<Toggle>().isOn = true; };
        scene._input["Tool3"].FirstClick += () => { scene.Tools.transform.GetChild(2).GetComponent<Toggle>().isOn = true; };
        scene._input["Tool4"].FirstClick += () => { scene.Tools.transform.GetChild(3).GetComponent<Toggle>().isOn = true; };
        

        return scene;
    }

    #region UI methods

    public void OnNewAnimation()
    {
        var newScene = NewScene.GetInstance(_core);
        newScene.OKClick = ClearAnimation;
        _core.Scenes.Push(newScene);
    }

    public void OnLoadAnimation()
    {
        var loadScene = LoadScene.GetInstance(_core);
        loadScene.OKClick = LoadAnimaton;
        loadScene.Init();
        _core.Scenes.Push(loadScene);
    }

    public void OnSaveAnimation()
    {
        var newScene = SaveScene.GetInstance(_core);
        newScene.Animation = _animation;
        _core.Scenes.Push(newScene);
    }

    public bool OnCursorTool
    {
        set
        {
            if (value && _currentTool != "Cursor")
                ChangeTool("Cursor");
        }
    }

    public bool OnVertexTool
    {
        set
        {
            if (value && _currentTool != "Vertex")
                ChangeTool("Vertex");
        }
    }

    public bool OnLineTool
    {
        set
        {
            if (value && _currentTool != "Line")
                ChangeTool("Line");
        }
    }

    public bool OnEraserTool
    {
        set
        {
            if (value && _currentTool != "Eraser")
                ChangeTool("Eraser");
        }
    }

    private void ChangeTool(string newTool)
    {
        switch (_currentTool)
        {
            case "Cursor":
                DeselectAllVertexes();
                break;
            case "Line":
                if (_lineAtCreation != null)
                    GameObject.Destroy(_lineAtCreation.gameObject);
                _lineAtCreation = null;
                break;
        }
        _currentTool = newTool;
    }

    public bool OnPlay
    {
        set
        {
            if (value != _play)
            {
                _play = value;
                _lastTick = Time.realtimeSinceStartup;
                _input.enabled = !value;
            }
        }
    }

    public void OnPause()
    {
        PlayButton.isOn = false;
    }

    public void OnPreviewFrame()
    {
        if (_play) return;
        Frame--;
        if (Frame < 0) Frame = Frames - 1;
        ShowFrame();
    }

    public void OnNextFrame(bool play = false)
    {
        if (_play && !play) return;
        Frame++;
        if (Frame == Frames) Frame = 0;
        ShowFrame();
    }

    public void OnAddFrameBefore()
    {
        if (_play) return;
        _animation.InsertFrame(Frame);
        OnNextFrame();
    }

    public void OnAddFrameAfter()
    {
        if (_play) return;
        _animation.InsertFrame(Frame + 1);
        Frames++;
    }

    public void OnDeleteFrame()
    {
        if (_play || Frames == 1) return;
        _animation.RemoveFrame(Frame);
        if (Frame == Frames)
            Frame = 0;
        Frames--;
        ShowFrame();
    }

    public void OnExit()
    {
        Application.Quit();
    }

    #endregion

    #region vertexes & lines modifying

    bool _lBtn = false;
    List<Vertex> _selectedVertexes = new List<Vertex>();
    Line _lineAtCreation = null;

    private void SelectVertexes(Vertex[] vertexes)
    {
        if (_input["Shift"].Pressed)
        {
            bool contains = true;
            foreach (var vertex in vertexes)
                contains = contains && _selectedVertexes.Contains(vertex);
            if (contains)
                foreach (var vertex in vertexes)
                    DeselectVertex(vertex);
            else
                foreach (var vertex in vertexes)
                    if (!_selectedVertexes.Contains(vertex))
                    {
                        vertex.Select(true);
                        _selectedVertexes.Add(vertex);
                    }
        }
        else
        {
            DeselectAllVertexes();
            foreach (var vertex in vertexes)
            {
                vertex.Select(true);
                _selectedVertexes.Add(vertex);
            }
        }
    }

    private void DeselectVertex(Vertex vertex)
    {
        vertex.Select(false);
        _selectedVertexes.Remove(vertex);
    }

    private void DeselectAllVertexes()
    {
        foreach (var vertex in _selectedVertexes)
            vertex.Select(false);
        _selectedVertexes.Clear();
        _gyzmo.UpdatePosition(_selectedVertexes);
    }

    private Vertex CreateVertex(Vector3 pos)
    {
        var vertex = GameObject.Instantiate<Vertex>(Vertex.Prefab);
        vertex.Init(Frames, Frame, _field, new Vector2(pos.x, pos.y));
        vertex.transform.localScale = new Vector3(_core.ZoomScale, _core.ZoomScale, 1f);
        _animation.Vertexes.Add(vertex);
        return vertex;
    }

    private Line CreateLine(Vertex vertex1)
    {
        var line = GameObject.Instantiate<Line>(Line.Prefab);
        line.Vertex1 = vertex1;
        line.transform.parent = _field;
        line.transform.localScale = new Vector3(line.transform.localScale.x, _core.ZoomScale*0.375f, 1f);
        return line;
    }

    private void DestroyVertexes(IEnumerable<Vertex> vertexes)
    {
        List<Vertex> vertForRemove = new List<Vertex>(vertexes);
        List<Line> lineForRemove = new List<Line>(_animation.GetLines(vertForRemove));
        foreach (var vert in vertForRemove)
        {
            _animation.Vertexes.Remove(vert);
            GameObject.Destroy(vert.gameObject);
        }
        foreach (var line in lineForRemove)
        {
            _animation.Lines.Remove(line);
            GameObject.Destroy(line.gameObject);
        }
        _gyzmo.UpdatePosition(_selectedVertexes);
    }

    private void ClearAnimation()
    {
        DestroyVertexes(_animation.Vertexes);
        PlayButton.isOn = false;
    }

    private void LoadAnimaton(string filename)
    {
        ClearAnimation();
        _animation = Animation.OpenFrom(filename);
        Frames = _animation.Frames;
        Frame = 0;
        foreach (var vert in _animation.Vertexes)
        {
            vert.transform.SetParent(_field);
            vert.transform.localScale = new Vector3(_core.ZoomScale, _core.ZoomScale, 1f);
        }
        foreach (var line in _animation.Lines)
        {
            line.transform.SetParent(_field);
            line.transform.localScale = new Vector3(line.transform.localScale.x, _core.ZoomScale * 0.375f, 1f);
        }
        ShowFrame();
    }

    #endregion

    #region input methods

    void MouseLeftPress()
    {
        if (!_eventSystem.IsPointerOverGameObject())
        {
            _lBtn = true;
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (pos.x < 0f || pos.x > 64f || pos.y < 0f || pos.y > 64f) return;
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero);
            var collider = hit.collider;
                
            switch (_currentTool)
            {
                case "Cursor":
                    if (hit && collider != null)
                    {
                        var vertex = collider.GetComponent<Vertex>();
                        var line = collider.GetComponent<Line>();
                        if (vertex != null)
                        {
                            SelectVertexes(new[] { vertex });
                            _gyzmo.UpdatePosition(_selectedVertexes);
                        }
                        else if (line != null)
                        {
                            SelectVertexes(new[] { line.Vertex1, line.Vertex2 });
                            _gyzmo.UpdatePosition(_selectedVertexes);
                        }
                        else if (collider.transform.parent == _gyzmo.transform)
                        {
                            _gyzmo.BeginMoving(collider.transform, pos);
                        }
                    }
                    else
                        DeselectAllVertexes();
                    break;
                case "Vertex":
                    {
                        CreateVertex(pos);
                    }
                    break;
                case "Line":
                    {
                        Vertex vertex = null;
                        if (hit && collider != null)
                            vertex = collider.GetComponent<Vertex>();
                        if (vertex == null)
                            vertex = CreateVertex(pos);
                        if (_lineAtCreation == null)
                        {
                            _lineAtCreation = CreateLine(vertex);
                        }
                        else
                        {
                            _lineAtCreation.Vertex2 = vertex;
                            _lineAtCreation.UpdatePosition();
                            _animation.Lines.Add(_lineAtCreation);
                            if (_input["Shift"].Pressed)
                                _lineAtCreation = CreateLine(vertex);
                            else
                                _lineAtCreation = null;
                        }
                    }
                    break;
                case "Eraser":
                    if (hit && collider != null)
                    {
                        var vertex = collider.GetComponent<Vertex>();
                        if (vertex != null)
                            DestroyVertexes(new[] { vertex });
                        else
                        {
                            var line = collider.GetComponent<Line>();
                            if (line != null)
                            {
                                _animation.Lines.Remove(line);
                                GameObject.Destroy(line.gameObject);
                            }
                        }
                    }
                    break;
            }
        }
    }

    void MouseLeftRelease()
    {
        _lBtn = false;
        _gyzmo.EndMoving();
        _gyzmo.UpdatePosition(_selectedVertexes);
    }

    void MouseRightClick()
    {
        if (!_eventSystem.IsPointerOverGameObject())
        {
            switch (_currentTool)
            {
                case "Line":
                    if (_lineAtCreation != null)
                        GameObject.Destroy(_lineAtCreation.gameObject);
                    _lineAtCreation = null;
                    break;
            }
        }
    }

    void DeleteClick()
    {
        DestroyVertexes(_selectedVertexes);
    }

    void RemoveKeyFrameOnSelected()
    {
        foreach (var vertex in _selectedVertexes)
            vertex.RemoveValueAt(Frame);
    }

    #endregion

    private void ShowFrame()
    {
        foreach (var vert in _animation.Vertexes)
            vert.ShowFrame(Frame);
        foreach (var line in _animation.Lines)
            line.UpdatePosition();
        _gyzmo.UpdatePosition(_selectedVertexes);
    }

    void Update()
    {
        #region camera moving
        if (OnTop)
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (v != 0f || h != 0f)
                _core.MoveCamera(new Vector2(h, v) * CameraSpeed * Time.deltaTime);
            if (scroll != 0f)
            {
                float zoom = scroll > 0 ? 4f / 5f : 5f / 4f;
                if (_core.ZoomCamera(zoom))
                {
                    foreach (var vertex in _animation.Vertexes)
                        vertex.transform.localScale = new Vector3(_core.ZoomScale, _core.ZoomScale, 1f);
                    foreach (var line in _animation.Lines)
                        line.transform.localScale = new Vector3(line.transform.localScale.x, _core.ZoomScale * 0.375f, 1f);
                    _gyzmo.transform.localScale = new Vector3(_core.ZoomScale, _core.ZoomScale, 1f);
                }
            }
        }
        #endregion

        if (_lineAtCreation != null)
            _lineAtCreation.UpdatePosition();

        if (_lBtn)
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var offset = _gyzmo.ContinueMoving(pos);
            if (offset != Vector2.zero)
            {
                bool x = true;
                bool y = true;
                foreach (var vertex in _selectedVertexes)
                {
                    var newPos = vertex[Frame]+offset;
                    x = x && newPos.x >= 0f && newPos.x <= 64f;
                    y = y && newPos.y >= 0f && newPos.y <= 64f;
                }
                if (x || y)
                {
                    foreach (var vertex in _selectedVertexes)
                    {
                        vertex[Frame] += new Vector2(x ? offset.x : 0f, y ? offset.y : 0f);
                        vertex.ShowFrame(Frame);
                    }
                    foreach (var line in _animation.GetLines(_selectedVertexes))
                        line.UpdatePosition();
                    _gyzmo.UpdatePosition(_selectedVertexes);
                }
            }
        }

        if (Input.GetButtonDown("Jump") && OnTop)
            PlayButton.isOn = !_play;

        if (_play && OnTop)
        {
            float time = Time.realtimeSinceStartup;
            if (_lastTick + float.Parse(FrameDelay.text) / 1000f < time)
            {
                OnNextFrame(true);
                _lastTick = time;
            }
        }
    }
}
