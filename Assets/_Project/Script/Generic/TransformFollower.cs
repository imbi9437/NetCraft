using UnityEngine;

namespace _Project.Script.Generic
{
    public class TransformFollower : MonoBehaviour
    {
        [SerializeField] private bool followPosition;
        [SerializeField] private bool followRotation;
        [SerializeField] private bool followScale;
        
        [SerializeField] private Transform target;

        private void Update()
        {
            if (followPosition)
                transform.position = target ? target.position : Vector3.zero;
            
            if (followRotation)
                transform.rotation = target ? target.rotation : Quaternion.identity;
            
            if (followScale)
                transform.localScale = target ? target.localScale : Vector3.one;
        }
    }
}
