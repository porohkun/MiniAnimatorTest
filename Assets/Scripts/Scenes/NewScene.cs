using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class NewScene : SceneBase
{
    public override bool Transparent { get { return true; } }

    public Action OKClick;
    public static NewScene GetInstance(Core core)
    {
        var scene = Instantiate<NewScene>(core);
        return scene;
    }

    public void OnOKClick()
    {
        if (OKClick!= null)
            OKClick();
        _core.Scenes.Pop(this);
    }

    public void OnCancelClick()
    {
        _core.Scenes.Pop(this);
    }

}
