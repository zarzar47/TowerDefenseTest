using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// This script handles the dragging and placement of tower prefabs in a tower defense game.
// It allows players to drag a tower from the UI and place it on a buildable tile in the game world.
// Can be easily extended to include more features like tower rotation, validation checks, etc.
public class TowerDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject towerPrefab;
    private GameObject draggingTower;

    void Start()
    {
        TextMeshProUGUI cost = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        cost.text = towerPrefab.GetComponent<Tower>().GetCost().ToString();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        draggingTower = Instantiate(towerPrefab);
        draggingTower.GetComponent<Collider>().enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask buildableMask = LayerMask.GetMask("Buildable");
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, buildableMask))
        {
            Vector3 snappedPos = new Vector3(Mathf.Round(hit.point.x), 0.5f, Mathf.Round(hit.point.z));
            draggingTower.transform.position = snappedPos;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        LayerMask buildableMask = LayerMask.GetMask("Buildable");

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, buildableMask))
        {
            Vector3 pos = hit.collider.transform.position;
            Vector2Int gridPos = new Vector2Int((int)pos.x, (int)pos.z);

            // Call GameManager to place tower
            GameManager.Instance.PlaceTowerAt(gridPos, towerPrefab);
        }

        Destroy(draggingTower); // Always destroy preview
    }
}
