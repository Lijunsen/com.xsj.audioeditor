﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ypzxAudioEditor.Utility;

namespace ypzxAudioEditor
{
    [CustomEditor(typeof(EventTrigger))]
    public class EventTriggerInspector : Editor
    {
        private EventTrigger trigger;

        private void OnEnable()
        {

            trigger = target as EventTrigger;
            if (null == trigger)
            {
                Debug.LogError("EventTrigger Script is null");
            }

        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (AudioEditorManager.FindManager() == null)
            {
                var color = GUI.color;
                GUI.color = Color.red;
                EditorGUILayout.HelpBox("场景中无法找到AudioEditorManager",MessageType.Error);
                GUI.color = color;
                return;
            }

            Undo.RecordObject(trigger, "AudioEditorTriggerChange");
            EditorGUI.BeginChangeCheck();

            using (new EditorGUILayout.VerticalScope("box", GUILayout.ExpandWidth(true)))
            {
                var newMask = EditorGUILayout.MaskField(new GUIContent("Trigger On:"), trigger.triggerTypes,
                     Enum.GetNames(typeof(MyEventTriggerType)));
                if (newMask != trigger.triggerTypes)
                {
                    trigger.triggerTypes = newMask;
                    //  Debug.Log(Convert.ToString(newMask, 2));
                }
                if ((newMask & 1 << (int)MyEventTriggerType.TriggerEnter) == 0B10)
                {
                    var color = GUI.color;
                    if (trigger.colliderTarget == null)
                    {
                        if (FindObjectOfType<AudioListener>() != null)
                        {
                            trigger.colliderTarget = FindObjectOfType<AudioListener>().gameObject;
                        }
                    }
                    var newObject = EditorGUILayout.ObjectField(new GUIContent("ColliderTarget:"), trigger.colliderTarget, typeof(GameObject), true) as GameObject;
                    if (newObject != trigger.colliderTarget)
                    {
                        trigger.colliderTarget = newObject;
                    }
                    if (newObject!=null &&( !newObject.GetComponent<Collider>() || !newObject.GetComponent<Rigidbody>()))
                    {
                        GUI.color = Color.red;
                        EditorGUILayout.LabelField("检测到目标无Rigidbody和Collider", GUI.skin.box, GUILayout.ExpandWidth(true));
                        if (GUILayout.Button("点击挂载"))
                        {
                            if (!newObject.GetComponent<Collider>())
                            {
                                var collider = newObject.AddComponent<SphereCollider>();
                                collider.isTrigger = true;
                            }
                            if (!newObject.GetComponent<Rigidbody>())
                            {
                                var rigidBody = newObject.AddComponent<Rigidbody>();
                                rigidBody.useGravity = false;
                            }
                        }
                    }
                    if (!trigger.gameObject.GetComponent<Collider>())
                    {
                        GUI.color = Color.red;
                        EditorGUILayout.LabelField("请挂载Collider", GUI.skin.box, GUILayout.ExpandWidth(true));
                    }
                    GUI.color = color;
                }
            }

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

            using (new EditorGUILayout.VerticalScope("box", GUILayout.ExpandWidth(true)))
            {
                var buttonRect = EditorGUI.PrefixLabel(EditorGUILayout.GetControlRect(), new GUIContent("Event: "));
                var selectedEvent = AudioEditorManager.GetAEComponentDataByID<AEEvent>(trigger.targetEventID);
                trigger.eventName = selectedEvent == null ? "null" : selectedEvent.name;
                if (EditorGUI.DropdownButton(buttonRect, new GUIContent(trigger.eventName), FocusType.Passive))
                {
                    PopupWindow.Show(buttonRect, new SelectPopupWindow(WindowOpenFor.SelectEvent, (selectedID) =>
                    {
                        Undo.RecordObject(trigger, "AudioEditorTriggerChange");
                        trigger.targetEventID = selectedID;
                    }));
                }
            }

            using (new EditorGUILayout.VerticalScope("box", GUILayout.ExpandWidth(true)))
            {
                if (GUILayout.Button(new GUIContent("事件触发")))
                {
                    trigger.PostEvent();
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
               // Debug.Log("Undo Trigger");
                Undo.RecordObject(trigger, "AudioEditorTriggerChange");
            }
        }
    }

}