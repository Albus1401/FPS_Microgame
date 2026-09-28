using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SimpleAmbienceTrigger : MonoBehaviour
{
    [Header("Manager")]
    [SerializeField]
    private SimpleAmbienceManager ambienceManager;

    [Header("Target State")]
    [Tooltip("The state number from the manager's list")]
    [SerializeField, Min(0)]
    private int targetState;

    [Header("What Should Change?")]
    [SerializeField]
    private bool changeSound = true;

    [SerializeField]
    private bool changeSnapshot = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        ambienceManager.SwitchTo(
            targetState,
            changeSound,
            changeSnapshot
        );
    }

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }
}