using Structures;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Temporary behaviour to give a player character building functionality.
/// </summary>
public class BuildPreviewer : MonoBehaviour, IBuildPreviewer
{
    public Action OnSnapToGrid { get; set; }
    public Action OnConfirm { get; set; }
    public Action OnCancel { get; set; }

    // Reference to the build manager for this player.
    // Needs to be refactored later.
    [SerializeField] private PlayerBuildManager buildManager;

    // Temporary inventory for the player
    [SerializeField] private List<Item> items = new List<Item>();

    // The active item.
    private Item m_item;
    public Item item
    {
        get { return m_item; }
        set
        {
            // Call OnCancel if the item has changed.
            if (value != m_item)
                OnCancel?.Invoke();

            m_item = value;
            
            if (item != null)
            {
                // Create a preview object for the new item.
                var preview = StructurePreview.Create(item.preview);
                preview.previewer = this;
                preview.SetBuildManager(buildManager);
            }
                
        }
    }

    #region Cleanup

    private void OnDisable()
    {
        item = null;
        OnSnapToGrid = null;
        OnConfirm = null;
        OnCancel = null;
    }

    #endregion


    private void Update()
    {
        // Create a preview object
        int itemIndex = 0;
        for (int i = (int)KeyCode.Alpha1; i < (int)KeyCode.AltGr; i++)
        {
            if (Input.GetKeyDown((KeyCode)i))
            {
                if (itemIndex < items.Count)
                {
                    item = items[itemIndex];
                    break;
                }
            }
            itemIndex++;
        }


        if (item != null)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                // Cancel the preview
                item = null;
            }
            else if (Input.GetMouseButtonDown(0))
            {
                // Confirm the placement
                OnConfirm?.Invoke();
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                // Trigger snapping to the grid
                OnSnapToGrid?.Invoke();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (item != null)
            buildManager.DrawGizmos();
    }
}
