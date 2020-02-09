using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(PlayerController))]
public class PlayerControllerEditor : Editor
{
    PlayerController controller;

    private void OnEnable()
    {
        controller = (PlayerController)target;
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement container = new VisualElement();

        IMGUIContainer baseInspector = new IMGUIContainer(base.OnInspectorGUI);

        container.Add(baseInspector);

        return container;
    }

    public override void OnInspectorGUI()
    {
    }
}
