using UnityEditor;
using UnityEngine;

namespace _Project.Script.Editor
{
    public class Utility : MonoBehaviour
    {
        [MenuItem("Tools/MeshColliderConvexEnable")]
        public static void EnableMeshColliderConvex()
        {
            var objs = Selection.gameObjects;

            if (objs.Length <= 0) return;

            foreach (var obj in objs)
            {
                var cols = obj.GetComponentsInChildren<MeshCollider>(true);

                foreach (var col in cols)
                {
                    col.convex = true;
                }
            }
            
            AssetDatabase.SaveAssets();
        }
        
        
        [MenuItem("GameObject/Separator", false, -1)]
        public static void CreateSeparator(MenuCommand command)
        {
            var obj = new GameObject("===========");
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;
            
            var selectObject = command.context as GameObject ?? Selection.activeGameObject;

            if (selectObject != null)
            {
                Transform parent = selectObject.transform.parent;
                int index = selectObject.transform.GetSiblingIndex();
                
                if (parent != null) obj.transform.SetParent(parent, false);
                
                obj.transform.SetSiblingIndex(++index);
            }
        }
    }
}
