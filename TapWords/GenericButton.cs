using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GenericButton : MonoBehaviour
{

    private Button button;
    public System.Action buttonPressedCallback;

    void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Missing button component! "  + gameObject.name);
        }
    }

    public void SetButtonAction (System.Action callback)
    {
        buttonPressedCallback = callback;
    }

    public void ButtonPressed ()
    {
        if (buttonPressedCallback != null)
        {
            buttonPressedCallback.Invoke();
        }
    }

 }
