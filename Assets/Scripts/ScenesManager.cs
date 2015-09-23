using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ScenesManager
{
    public SceneBase TopScene
    {
        get
        {
            return _stack.Last();
        }
    }

    private List<SceneBase> _stack = new List<SceneBase>();

    public void Push(SceneBase scene)
    {
        push(scene, true);
    }

    private void push(SceneBase scene, bool withSwitch)
    {
        _stack.Add(scene);
        SwitchScenesActivity();
    }

    public SceneBase Pop()
    {
        return pop(true);
    }

    private SceneBase pop(bool withSwitch)
    {
        var count = _stack.Count;
        SceneBase result = null;
        if (count > 1)
        {
            result = _stack[count - 1];
            result.Enabled = false;
            _stack.RemoveAt(count - 1);
        }
        if (withSwitch)
            SwitchScenesActivity();
        return result;
    }

    public void Pop(SceneBase scene)
    {
        if (_stack.Contains(scene))
        {
            _stack.Remove(scene);
            scene.Enabled = false;
            SwitchScenesActivity();
        }
    }

    public void Pop(Type sceneType)
    {
        SceneBase sceneForRemove = null;
        foreach (var scene in _stack)
        {
            if (scene.GetType() == sceneType)
            {
                sceneForRemove = scene;
                break;
            }
        }
        if (sceneForRemove != null)
            Pop(sceneForRemove);
    }

    public void PopTill(Type sceneType, bool destroy = false, bool include = false)
    {
        while (_stack.Count > 1)
        {
            var scene = pop(false);
            if (scene != null)
            {
                if (scene.GetType() == sceneType)
                {
                    if (!include)
                        push(scene, false);
                    else
                        SceneBase.Destroy(scene);
                    break;
                }
                else
                    SceneBase.Destroy(scene);
            }
        }
        SwitchScenesActivity();
    }

    public void PopTill(SceneBase scene, bool destroy = false, bool include = false)
    {
        while (_stack.Count > 1)
        {
            var pScene = pop(false);
            if (pScene != null)
            {
                if (pScene == scene)
                {
                    if (!include)
                        push(pScene, false);
                    else
                        SceneBase.Destroy(pScene);
                    break;
                }
                else
                    SceneBase.Destroy(pScene);
            }
        }
        SwitchScenesActivity();
    }

    private void SwitchScenesActivity()
    {
        bool active = true;
        for (int i = _stack.Count - 1; i >= 0; i--)
        {
            var scene = _stack[i];
            scene.Enabled = active;
            active = active && scene.Transparent;
        }
    }
}
