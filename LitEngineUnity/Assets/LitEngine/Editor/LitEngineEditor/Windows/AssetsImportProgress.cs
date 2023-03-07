using UnityEngine;
using UnityEditor;
public class AssetsImportProgress : AssetPostprocessor
{
    public void OnPostprocessModel(GameObject _model)
    {
       /// ModelImporter modelImp = (ModelImporter)assetImporter;
        // Renderer[] trenders = _model.GetComponentsInChildren<Renderer>();
        // for (int i = 0; i < trenders.Length; i++)
        // {
        //     trenders[i].sharedMaterial = null;
        //
        //     if (trenders[i].sharedMaterials != null)
        //     {
        //         trenders[i].sharedMaterials = new Material[0];
        //     }
        // }
    }

    public void OnPostprocessTexture(Texture2D _tex)
    {
    }


    public void OnPostprocessAudio(AudioClip _clip)
    {

    }
}
