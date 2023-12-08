using UnityEngine;

public class AutoUnParent : MonoBehaviour
{
    private void Awake()
    {
        transform.DetachChildren();
    }
}
