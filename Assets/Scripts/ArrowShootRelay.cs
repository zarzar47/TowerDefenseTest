using UnityEngine;

public class ArrowShootRelay : MonoBehaviour
{
    public Tower towerParent;

    void Start()
    {
        // Auto-find the Tower component in the parent
        if (towerParent == null)
        {
            towerParent = GetComponentInParent<Tower>();
        }
    }

    // This method is called from the animation event
    public void TriggerFireArrow()
    {
        if (towerParent != null)
        {
            towerParent.Attack();
        }
    }
}
