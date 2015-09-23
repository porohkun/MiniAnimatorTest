using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LoadScene : SceneBase
{
    public override bool Transparent { get { return true; } }

    public RectTransform WarningPanel;
    public ToggleGroup FilesList;
    public Toggle ListItemPrefab;

    public Action<string> OKClick;

    public static LoadScene GetInstance(Core core)
    {
        var scene = Instantiate<LoadScene>(core);
        return scene;
    }

    public void Init()
    {
        for (int i = 0; i < FilesList.transform.childCount; i++)
            Destroy(FilesList.transform.GetChild(i).gameObject);

        foreach (var filename in Directory.GetFiles("Animations"))
        {
            string file = Path.GetFileName(filename);
            var toggle = GameObject.Instantiate<Toggle>(ListItemPrefab);
            var item = toggle.GetComponent<FileItem>();
            item.Text.text = file;
            item.Filename = filename;
            item.transform.SetParent(FilesList.transform);
            toggle.group = FilesList;
        }
    }

    public void OnOKClick()
    {
        WarningPanel.gameObject.SetActive(true);
    }

    public void OnCancelClick()
    {
        _core.Scenes.Pop(this);
    }

    public void OnWarningOkClick()
    {
        var toggle = FilesList.ActiveToggles().FirstOrDefault();
        if (toggle != null)
        {
            string filename = toggle.GetComponent<FileItem>().Filename;
            if (OKClick != null)
                OKClick(filename);
            _core.Scenes.Pop(this);

            WarningPanel.gameObject.SetActive(false);
            FilesList.SetAllTogglesOff();
        }
    }

    public void OnWarningCancelClick()
    {
        WarningPanel.gameObject.SetActive(false);
    }
}
