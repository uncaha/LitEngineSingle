using System;
using System.Collections.Generic;
using UnityEngine;
namespace LitEngine.Log
{
    public class LogWindow : MonoBehaviour
    {

        private bool IsOpen = false;

        private static string datePatt = @"M/d/yyyy hh:mm:ss tt";
        private List<string> events = new List<string>();

        private Vector2 scrollPosition = Vector2.zero;

        private const int DpiScalingFactor = 160;
        private float? scaleFactor;
        private GUIStyle buttonStyle;
        private GUIStyle textStyle;

        protected int FontSize
        {
            get
            {
                return (int)Math.Round(this.ScaleFactor * 16);
            }
        }

        protected float ScaleFactor
        {
            get
            {
                if (!this.scaleFactor.HasValue)
                {
                    this.scaleFactor = Screen.dpi / DpiScalingFactor;
                }

                return this.scaleFactor.Value;
            }
        }

        protected GUIStyle TextStyle
        {
            get
            {
                if (this.textStyle == null)
                {
                    this.textStyle = new GUIStyle(GUI.skin.textArea);
                    this.textStyle.alignment = TextAnchor.UpperLeft;
                    this.textStyle.wordWrap = true;
                    this.textStyle.padding = new RectOffset(10, 10, 10, 10);
                    this.textStyle.stretchHeight = true;
                    this.textStyle.stretchWidth = false;
                    this.textStyle.fontSize = this.FontSize;
                }

                return this.textStyle;
            }
        }

        protected GUIStyle ButtonStyle
        {
            get
            {
                if (this.buttonStyle == null)
                {
                    this.buttonStyle = new GUIStyle(GUI.skin.button);
                    this.buttonStyle.fontSize = this.FontSize;
                }

                return this.buttonStyle;
            }
        }

        protected static int ButtonHeight
        {
            get
            {
                return 60;
            }
        }

        protected static int MainWindowWidth
        {
            get
            {
                return Screen.width - 30;
            }
        }

        protected Vector2 ScrollPosition
        {
            get
            {
                return this.scrollPosition;
            }

            set
            {
                this.scrollPosition = value;
            }
        }

        protected static int MainWindowFullWidth
        {
            get
            {
                return Screen.width;
            }
        }

        protected bool Button(string label)
        {
            return GUILayout.Button(
                label,
                this.ButtonStyle,
                GUILayout.MinHeight(ButtonHeight * ScaleFactor),
                GUILayout.MaxWidth(MainWindowWidth));
        }

        public void AddLog(string log)
        {
            events.Insert(0, string.Format("{0}\n{1}\n", DateTime.Now.ToString(datePatt), log));
        }

        protected void OnGUI()
        {
            GUILayout.BeginVertical();
            if(!IsOpen)
            {
                if (this.Button("ShowLog"))
                {
                    IsOpen = true;
                }
            }
            
            if (!IsOpen) return;
            if (this.Button("Back"))
            {
                IsOpen = false;
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            {
                Vector2 scrollPosition = this.ScrollPosition;
                scrollPosition.y += Input.GetTouch(0).deltaPosition.y;
                this.ScrollPosition = scrollPosition;
            }

            this.ScrollPosition = GUILayout.BeginScrollView(
                this.ScrollPosition,
                GUILayout.MinWidth(MainWindowFullWidth));

            GUILayout.TextArea(
                string.Join("\n", events.ToArray()),
                this.TextStyle,
                GUILayout.ExpandHeight(true),
                GUILayout.MaxWidth(MainWindowWidth));

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }
    }
}

