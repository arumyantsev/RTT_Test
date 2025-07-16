using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Enums;
using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    [SerializeField]
    private GameObject hoverIndicator;
    [SerializeField]
    private GameObject selectionIndicator;
    [SerializeField]
    private SelectableType selectableType;
    [SerializeField]
    private Unit thisUnit;

    private void Start()
    {
        if(selectableType == SelectableType.Unit)
        {
            thisUnit = GetComponent<Unit>();
        }
        hoverIndicator.SetActive(false);
        selectionIndicator.SetActive(false);
    }

    private void OnMouseEnter()
    {
        if(selectableType == SelectableType.Unit)
        {
            if(!GameManager._instance.IsUnitSelected(thisUnit))
            {
                hoverIndicator.SetActive(true);
            }
        }
                
    }

    private void OnMouseExit()
    {
        hoverIndicator.SetActive(false);
    }

    private void OnMouseOver()
    {
        if(Input.GetMouseButtonUp(0))
        {
            if (selectableType == SelectableType.Unit)
            {
                GameManager._instance.SelectUnit(thisUnit, GameManager._instance.isShiftDown);
                hoverIndicator.SetActive(false);
            }
        }
    }

    public void ShowSelectionIndicator()
    {
        selectionIndicator.SetActive(true);
    }

    public void HideSelectionIndicator()
    {
        selectionIndicator.SetActive(false);
    }


}
