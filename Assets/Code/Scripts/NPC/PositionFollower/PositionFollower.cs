using UnityEngine;
using UnityEngine.Events;

public class PositionFollower : MonoBehaviour
{
    [SerializeField]
    private bool followParent;

    [SerializeField]
    private Transform targetTransform = null;
    [SerializeField] private bool rotateTowardsCamera = true;
    [SerializeField]
    private Vector3 offset = Vector3.zero;

    private void Start()
    {
        if (followParent)
        {
            targetTransform = transform.parent;
            offset = transform.localPosition;
            transform.parent = null;
            transform.rotation = Quaternion.Euler(-59, 135, 0);
        }
    }

    void LateUpdate()
    {
        if (!targetTransform)
            return;

        transform.position = targetTransform.TransformPoint(offset);
        if (rotateTowardsCamera)
            transform.LookAt(Camera.main.transform);
    }

    /*  public bool getTargetDeathEvent(out UnityEvent death)
    {
        death = targetTransform.GetComponent<Character>().characterStats.death;
        bool foundDeathevent = death != null;
        return foundDeathevent;
    }
   */
    /* #if UNITY_EDITOR
     void OnDrawGizmos()
     {
         DebugUtility.DrawDebugCross(targetTransform.TransformPoint(offset), Color.cyan, .1f);
     }
     #endif  */
}