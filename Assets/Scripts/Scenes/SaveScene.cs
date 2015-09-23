using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SaveScene : SceneBase
{
    public override bool Transparent { get { return true; } }

    public Text ErrorText;
    public RectTransform ExistPanel;
    public InputField FilenameText;

    public Animation Animation;

    public Action OKClick;
    public static SaveScene GetInstance(Core core)
    {
        var scene = Instantiate<SaveScene>(core);
        return scene;
    }

    public void OnOKClick()
    {
        SaveAnimation(false);
    }

    public void OnCancelClick()
    {
        _core.Scenes.Pop(this);
    }

    public void OnExistOkClick()
    {
        ExistPanel.gameObject.SetActive(false);
        SaveAnimation(true);
    }

    public void OnExistCancelClick()
    {
        ExistPanel.gameObject.SetActive(false);
    }

    void SaveAnimation(bool overwrite)
    {
        if (FilenameText.text == "") return;
        string filename = string.Format("Animations\\{0}.man", FilenameText.text
            .Replace('\\', '_')
            .Replace('/', '_')
            .Replace(':', '_')
            .Replace('*', '_')
            .Replace('?', '_')
            .Replace('"', '_')
            .Replace('<', '_')
            .Replace('>', '_')
            .Replace('|', '_'));
        if (System.IO.File.Exists(filename) && !overwrite)
        {
            ErrorText.text = string.Format("Файл {0} уже существует.\r\nПерезаписать его?", filename);
            ExistPanel.gameObject.SetActive(true);
        }
        else
        {
            this.Animation.SaveTo(System.IO.Path.GetFullPath(filename));
            _core.Scenes.Pop(this);
            ExistPanel.gameObject.SetActive(false);
        }
    }
}
