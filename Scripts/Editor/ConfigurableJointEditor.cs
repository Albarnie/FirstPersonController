using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(ConfigurableJoint), true), CanEditMultipleObjects]
public class ConfigurableJointEditor : Editor
{
    //This is a very large UI, so certain parts have been compacted
    //some curly brackets are used to organise the containers to make it more obvious
    public override VisualElement CreateInspectorGUI()
    {
        SerializedObject so = serializedObject;
        VisualElement container = new VisualElement();
        //Edit button

        //      Header: Joint
        FoldableArea jointContainer = new FoldableArea("Joint");
        {

            //  Connected body
            jointContainer.Add(so.CreatePropertyField("m_ConnectedBody"));

            //Axis
            jointContainer.Add(so.CreatePropertyField("m_Axis"));
            jointContainer.Add(so.CreatePropertyField("m_SecondaryAxis"));

            //Anchor
            jointContainer.Add(so.CreatePropertyField("m_AutoConfigureConnectedAnchor"));
            jointContainer.Add(so.CreatePropertyField("m_Anchor"));
        }

        //      Header: Linear Motion
        FoldableArea linearContainer = new FoldableArea("Linear Motion");
        {
            //  X Motion Y Motion Z Motion
            linearContainer.Add(CreateMotionField("m_XMotion"));
            linearContainer.Add(CreateMotionField("m_YMotion"));
            linearContainer.Add(CreateMotionField("m_ZMotion"));
            //  Linear Limit
            linearContainer.Add(so.CreatePropertyField("m_LinearLimit"));
            //  Linear Limit Spring
            linearContainer.Add(so.CreatePropertyField("m_LinearLimitSpring"));
        }

        //      Header: Angular Motion
        FoldableArea angularContainer = new FoldableArea("Angular Motion");
        {
            //  X Motion Y Motion Z Motion
            angularContainer.Add(CreateMotionField("m_AngularXMotion"));
            angularContainer.Add(CreateMotionField("m_AngularYMotion"));
            angularContainer.Add(CreateMotionField("m_AngularZMotion"));

            //      Low Angular X Limit
            angularContainer.Add(so.CreatePropertyField("m_LowAngularXLimit"));
            //      High Angular X Limit
            angularContainer.Add(so.CreatePropertyField("m_HighAngularXLimit"));
            //      Angular X Limit Spring
            angularContainer.Add(so.CreatePropertyField("m_AngularXLimitSpring"));
 
            //      Angular Y Limit
            angularContainer.Add(so.CreatePropertyField("m_AngularYLimit"));
            //     Angular Z Limit
            angularContainer.Add(so.CreatePropertyField("m_AngularZLimit"));
            //  Angular YZ Limit Spring
            angularContainer.Add(so.CreatePropertyField("m_AngularYZLimitSpring"));
        }

        //      Header: Linear Drive
        FoldableArea linearDriveContainer = new FoldableArea("Linear Drive");
        {
            //Target Position
            linearDriveContainer.Add(so.CreatePropertyField("m_TargetPosition"));
            //Target Velocity
            linearDriveContainer.Add(so.CreatePropertyField("m_TargetVelocity"));
            //X Drive
            linearDriveContainer.Add(so.CreatePropertyField("m_XDrive"));
            //Y Drive
            linearDriveContainer.Add(so.CreatePropertyField("m_YDrive"));
            //Z Drive
            linearDriveContainer.Add(so.CreatePropertyField("m_ZDrive"));
        }

        //      Header: Angular Drive
        FoldableArea angularDriveContainer = new FoldableArea("Angular Drive");
        {
            //Target Rotation
            angularDriveContainer.Add(so.CreatePropertyField("m_TargetRotation"));
            //Target Angular Velocity
            angularDriveContainer.Add(so.CreatePropertyField("m_TargetAngularVelocity"));
            //Rotation Drive mode
            angularDriveContainer.Add(so.CreatePropertyField("m_AngularDriveMode"));
            //Angular X Drive
            angularDriveContainer.Add(so.CreatePropertyField("m_AngularXDrive"));
            //Angular YZ Drive
            angularDriveContainer.Add(so.CreatePropertyField("m_AngularYZDrive"));
            //Slerp Drive
            angularDriveContainer.Add(so.CreatePropertyField("m_SlerpDrive"));
        }

        //      Header: Projection
        FoldableArea projectionContainer = new FoldableArea("Projection");
        {
            //Projection Mode
            projectionContainer.Add(so.CreatePropertyField("m_ProjectionMode"));
            //Projection Distance
            projectionContainer.Add(so.CreatePropertyField("m_ProjectionDistance"));
            //Projection Angle
            projectionContainer.Add(so.CreatePropertyField("m_ProjectionAngle"));
        }

        //      Header: Joint Settings
        FoldableArea settingsContainer = new FoldableArea("Settings");
        {
            // Configured in world space
            settingsContainer.Add(so.CreatePropertyField("m_ConfiguredInWorldSpace"));
            //Swap Bodies
            settingsContainer.Add(so.CreatePropertyField("m_SwapBodies"));
            //Break Force
            settingsContainer.Add(so.CreatePropertyField("m_BreakForce"));
            //Break Torque
            settingsContainer.Add(so.CreatePropertyField("m_BreakTorque"));
            //Enable Collision
            settingsContainer.Add(so.CreatePropertyField("m_EnableCollision"));
            //Enable Preprocessing
            settingsContainer.Add(so.CreatePropertyField("m_EnablePreprocessing"));
            //Mass Scale
            settingsContainer.Add(so.CreatePropertyField("m_MassScale"));
            //Connected Mass Scale
            settingsContainer.Add(so.CreatePropertyField("m_ConnectedMassScale"));
        }

        container.Add(jointContainer);
        container.Add(linearContainer);
        container.Add(angularContainer);

        container.Add(linearDriveContainer);
        container.Add(angularDriveContainer);

        container.Add(projectionContainer);
        container.Add(settingsContainer);

        return container;
    }

    PropertyField CreateMotionField(string propertyName)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);

        PropertyField field = new PropertyField(property);
        field.style.flexGrow = 2;
        field.style.flexShrink = 2;
        field.label = "";

        field.BindProperty(property);

        return field;
    }
}

public class FoldableArea : VisualElement
{
    VisualElement container;

    public FoldableArea(string name)
    {
        Label header = new Label(name);
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.style.position = Position.Absolute;

        container = new Foldout();

        base.Add(header);
        base.Add(container);
    }

    public new void Add(VisualElement child)
    {
        container.Add(child);
    }
}

public static class UIElementsHelpers
{
    public static PropertyField CreatePropertyField(this SerializedObject serializedObject, string propertyName, string label = "")
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        PropertyField field = new PropertyField(property, label);
        return field;
    }
}
