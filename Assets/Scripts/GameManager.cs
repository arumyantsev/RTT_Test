using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager _instance;
    [SerializeField]
    private List<Unit> allUnits;
    [SerializeField]
    private List<Unit> selectedUnits;
    [SerializeField]
    private LayerMask groundLayer;
    [SerializeField]
    private GameObject destinationMarker;
    public bool isShiftDown = false;

    private void Start()
    {
        _instance = this;
        selectedUnits = new List<Unit>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            isShiftDown = true;
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            isShiftDown = false;
        }
        if(Input.GetMouseButtonUp(1))
        {
            for(int i = 0; i < selectedUnits.Count; i++)
            {
                Vector3 destination = GetMouseWorldPosition();
                destinationMarker.transform.position = destination;
                selectedUnits[i].MoveToDestination(destination);
            }
        }
    }

    private void HideUnitSelectionIndicators()
    {
        for(int i = 0; i < allUnits.Count; i++)
        {
            allUnits[i].selectableObject.HideSelectionIndicator();
        }
    }

    public void SelectUnit(Unit unit, bool addToSelection)
    {
        if(addToSelection)
        {
            if(!selectedUnits.Contains(unit))
            {
                selectedUnits.Add(unit);
                
            }
        }
        else
        {
            HideUnitSelectionIndicators();
            selectedUnits.Clear();
            selectedUnits.Add(unit);
        }
        unit.selectableObject.ShowSelectionIndicator();
        
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 newPosition = Vector3.zero;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            newPosition = hit.point;
        }

        return newPosition;
    }

    public void RegisterUnit(Unit newUnit)
    {
        allUnits.Add(newUnit);
    }

    public bool IsUnitSelected(Unit unit)
    {
        return selectedUnits.Contains(unit);
    }
}
