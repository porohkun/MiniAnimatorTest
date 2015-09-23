using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class InputController:MonoBehaviour
{
    Dictionary<string, InputButton> _buttons = new Dictionary<string, InputButton>();

    public SceneBase SceneOwner;

    public InputButton this[string axis]
    {
        get
        {
            if (!_buttons.ContainsKey(axis))
                _buttons.Add(axis, new InputButton(axis));
            return _buttons[axis];
        }
    }

    void Update()
    {
        if (SceneOwner == null || SceneOwner.OnTop)
            foreach (var button in _buttons.Values)
                button.Update();
    }

    public class InputButton
    {
        public string Axis { get; private set; }
        public float FirstDelay { get; set; }
        public float RepeatDelay { get; set; }

        public bool Pressed { get { return _state != BtnState.Released; } }

        public event Action Press;
        public event Action FirstClick;
        public event Action RepeatClick;
        public event Action Click;
        public event Action Release;

        private BtnState _state = BtnState.Released;
        private float _last = 0f;

        internal InputButton(string axis)
        {
            Axis = axis;
            FirstDelay = 400f;
            RepeatDelay = 100f;
        }

        public void SetTiming(float firstDelay, float repeatDelay)
        {
            FirstDelay = firstDelay;
            RepeatDelay = repeatDelay;
        }

        internal void Update()
        {
            if (Input.GetButton(Axis))
            {
                float delta = (Time.realtimeSinceStartup - _last) * 1000f;
                switch (_state)
                {
                    case BtnState.Released:
                        {
                            _state = BtnState.FirstClick;
                            _last = Time.realtimeSinceStartup;
                            if (Press != null) Press();
                            if (FirstClick != null) FirstClick();
                            if (Click != null) Click();
                        }
                        break;
                    case BtnState.FirstClick:
                        if (delta >= FirstDelay)
                        {
                            _state = BtnState.Repeating;
                            _last = Time.realtimeSinceStartup;
                            if (RepeatClick != null) RepeatClick();
                            if (Click != null) Click();
                        }
                        break;
                    case BtnState.Repeating:
                        if (delta >= RepeatDelay)
                        {
                            _last = Time.realtimeSinceStartup;
                            if (RepeatClick != null) RepeatClick();
                            if (Click != null) Click();
                        }
                        break;
                }
            }
            else if (_state!= BtnState.Released)
            {
                _state = BtnState.Released;
                if (Release != null) Release();
            }
            
        }

        private enum BtnState
        {
            Released = 0,
            FirstClick = 1,
            Repeating = 2
        }
    }
}
