using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SceneBase : MonoBehaviour
{
    private static Dictionary<string, SceneBase> _instances = new Dictionary<string, SceneBase>();

    protected Core _core { get; private set; }

    public virtual bool Enabled
    {
        get
        {
            return gameObject.activeSelf;
        }
        set
        {
            if (gameObject.activeSelf != value)
                gameObject.SetActive(value);
        }
    }
    public virtual bool OnTop
    {
        get
        {
            return this == _core.Scenes.TopScene;
        }
    }
    public virtual bool IsUi { get { return true; } }
    public virtual bool Transparent { get { return false; } }

    protected static T Instantiate<T>(Core core) where T : SceneBase
    {
        var sceneName = typeof(T).Name;
        if (_instances.ContainsKey(sceneName))
            return _instances[sceneName] as T;

        var instance = Instantiate<T>(Resources.Load<T>(string.Format("Scenes/{0}", sceneName)));
        if (instance.IsUi)
        {
            var rt = instance.transform as RectTransform;
            rt.SetParent(core.UiCanvas.transform);
            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector3.zero;
            rt.sizeDelta = Vector3.zero;
        }
        else
        {

        }
        instance.SetCore(core);
        instance.Enabled = false;
        _instances.Add(sceneName, instance);
        return instance;
    }

    public static void Destroy(SceneBase scene)
    {
        var sceneName = scene.GetType().Name;
        if (_instances.ContainsKey(sceneName))
            _instances.Remove(sceneName);
        GameObject.Destroy(scene);
    }

    protected virtual void SetCore(Core core)
    {
        if (_core == null)
            _core = core;
    }

}