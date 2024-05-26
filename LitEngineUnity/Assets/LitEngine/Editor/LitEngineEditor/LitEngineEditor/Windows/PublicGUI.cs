using UnityEngine;
using UnityEditor;
using System.Text;

namespace LitEngineEditor
{
    public class PublicGUI
    {
        public static Vector2 DrawScrollview(string _title,string _context,Vector2 _pos,float _width,float height)
        {
            GUILayout.Label(_title, EditorStyles.boldLabel);
            _pos = GUILayout.BeginScrollView(_pos, GUILayout.Width(_width), GUILayout.Height(height));
            GUILayout.Box(_context.ToString(), EditorStyles.textField);
            GUILayout.EndScrollView();
            return _pos;
        }
    }
}
