using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Collections.Generic;
namespace LitEngineEditor
{
    public class MeshTool : ExportBase
    {
        private Vector2 mScrollPosition = Vector2.zero;
        private StringBuilder mContext = new StringBuilder();
        private Object SelectedObject = null;
        public MeshTool() : base()
        {
            ExWType = ExportWType.MeshToolWindow;
        }

        override public void OnGUI()
        {
            GUILayout.Label("MeshExport", EditorStyles.boldLabel);

            if(SelectedObject != Selection.activeObject)
            {
                SelectedObject = Selection.activeObject;
                RestText();
            }

            mScrollPosition = PublicGUI.DrawScrollview("MeshList", mContext.ToString(), mScrollPosition, mWindow.position.size.x, 150);

            GUILayout.Label("ExportPath", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField("", ExportSetting.Instance.sMeshExportPath, EditorStyles.textField);
            if (GUILayout.Button("...", GUILayout.Width(25)))
            {
                string toldstr = ExportSetting.Instance.sMeshExportPath;
                toldstr = EditorUtility.OpenFolderPanel("file Path", toldstr, "");
                if (!string.IsNullOrEmpty(toldstr) && !toldstr.Equals(ExportSetting.Instance.sMeshExportPath))
                {
                    ExportSetting.Instance.sMeshExportPath = toldstr;
                    NeedSaveSetting();
                }

            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Combine"))
            {
                string tfilepath = EditorUtility.SaveFilePanelInProject("Save CombineMesh", "NewCombineMesh", "asset", "Save a new file.", ExportSetting.Instance.sMeshExportPath);
                if (!string.IsNullOrEmpty(tfilepath))
                    Combine(tfilepath);
            }

            if (GUILayout.Button("Combine And Export Obj"))
            {
                string tfilepath = EditorUtility.SaveFilePanelInProject("Save Obj", "NewObjMesh", "obj", "Save a new file.", ExportSetting.Instance.sMeshExportPath);
                if (!string.IsNullOrEmpty(tfilepath))
                    CombineAndExportToObj(tfilepath);
            }
        }

        public void AddContext(string _text)
        {
            lock (mContext)
            {
                mContext.AppendLine(_text);
            }

        }

        private void RestText()
        {
            mContext.Remove(0, mContext.Length);
            if (SelectedObject == null || !SelectedObject.GetType().Equals(typeof(GameObject))) return;

            GameObject trootobj = (GameObject)SelectedObject;
            MeshFilter[] meshFilters = trootobj.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combines = new CombineInstance[meshFilters.Length];
            var materialList = new List<Material>();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                AddContext(meshFilters[i].name);
            }
        }


        private Mesh GetCombineMesh()
        {
            if (SelectedObject == null ) return null;
            if (!SelectedObject.GetType().Equals(typeof(GameObject))) return null;
            GameObject trootobj = (GameObject)SelectedObject;
            MeshFilter[] meshFilters = trootobj.GetComponentsInChildren<MeshFilter>();
            CombineInstance[] combines = new CombineInstance[meshFilters.Length];
            var materialList = new List<Material>();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                combines[i].mesh = meshFilters[i].sharedMesh;
                combines[i].transform = Matrix4x4.TRS(meshFilters[i].transform.position - trootobj.transform.position,
                    meshFilters[i].transform.rotation, meshFilters[i].transform.lossyScale);
                var materials = meshFilters[i].GetComponent<MeshRenderer>().sharedMaterials;
                foreach (var material in materials)
                {
                    materialList.Add(material);
                }
            }

            Mesh newMesh = new Mesh();
            newMesh.CombineMeshes(combines, false);
            newMesh.name = "CombineMesh";
            return newMesh;
        }

        private void Combine(string newPath)
        {
            Mesh newMesh = GetCombineMesh();
            if (newMesh == null) return;
            AssetDatabase.CreateAsset(newMesh, newPath);
            AssetDatabase.Refresh();
        }

        private void CombineAndExportToObj(string newPath)
        {
            Mesh newMesh = GetCombineMesh();
            if (newMesh == null) return;
            MeshToFile(newMesh, newPath);
            AssetDatabase.Refresh();
        }

        public string MeshToString(Mesh targetMesh)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("g ").Append(targetMesh.name).Append("\n");
            foreach (Vector3 v in targetMesh.vertices)
            {
                sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 v in targetMesh.normals)
            {
                sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 v in targetMesh.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }
            for (int material = 0; material < targetMesh.subMeshCount; material++)
            {
                sb.Append("\n");
                sb.Append("usemtl ").Append("").Append("\n");
                sb.Append("usemap ").Append("").Append("\n");

                int[] triangles = targetMesh.GetTriangles(material);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                        triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
                }
            }
            return sb.ToString();
        }

        public void MeshToFile(Mesh targetMesh, string newPath)
        {
            using (StreamWriter sw = new StreamWriter(newPath))
            {
                sw.Write(MeshToString(targetMesh));
            }
        }
    }
}
