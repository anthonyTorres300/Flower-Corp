using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Add this script to your WeaponSelection GameObject temporarily to diagnose button issues.
/// It will log detailed information about what's happening with your UI.
/// </summary>
public class WeaponSelectionDebugger : MonoBehaviour
{
    public Button buttonLeft;
    public Button buttonRight;
    public GameObject selectionPanel;

    void Start()
    {
        Debug.Log("====== WEAPON SELECTION DEBUGGER ======");
        
        // Check EventSystem
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogError("❌ NO EVENT SYSTEM FOUND! You need an EventSystem for UI to work!");
            Debug.LogError("Fix: Right-click in Hierarchy → UI → Event System");
        }
        else
        {
            Debug.Log("✅ EventSystem found: " + eventSystem.name);
        }

        // Check Panel
        if (selectionPanel == null)
        {
            Debug.LogError("❌ Selection Panel is NULL!");
        }
        else
        {
            Debug.Log("✅ Selection Panel assigned: " + selectionPanel.name);
            Debug.Log("   Panel active: " + selectionPanel.activeSelf);
            
            Canvas canvas = selectionPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Debug.Log("   Canvas found: " + canvas.name);
                Debug.Log("   Canvas render mode: " + canvas.renderMode);
            }
            else
            {
                Debug.LogError("❌ Panel is not inside a Canvas!");
            }
        }

        // Check Buttons
        CheckButton("LEFT", buttonLeft);
        CheckButton("RIGHT", buttonRight);

        Debug.Log("======================================");
    }

    void CheckButton(string name, Button button)
    {
        if (button == null)
        {
            Debug.LogError($"❌ {name} Button is NULL!");
            return;
        }

        Debug.Log($"✅ {name} Button assigned: {button.name}");
        Debug.Log($"   GameObject active: {button.gameObject.activeSelf}");
        Debug.Log($"   Enabled: {button.enabled}");
        Debug.Log($"   Interactable: {button.interactable}");
        
        // Check if button has text
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            Debug.Log($"   Text: '{text.text}'");
        }

        // Check collider/raycast target
        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            Debug.Log($"   Image raycastTarget: {image.raycastTarget}");
        }

        // Check listeners
        int listenerCount = button.onClick.GetPersistentEventCount();
        Debug.Log($"   Persistent listeners: {listenerCount}");
    }

    void Update()
    {
        // Log when mouse is clicked
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("MOUSE CLICKED!");
            
            // Check what's under the mouse
            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem != null)
            {
                var pointerData = new PointerEventData(eventSystem);
                pointerData.position = Input.mousePosition;
                
                var results = new System.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(pointerData, results);
                
                if (results.Count > 0)
                {
                    Debug.Log($"   Raycast hit {results.Count} UI elements:");
                    foreach (var result in results)
                    {
                        Debug.Log($"   - {result.gameObject.name}");
                    }
                }
                else
                {
                    Debug.LogWarning("   Raycast hit NOTHING! Your buttons might be behind something.");
                }
            }
        }
    }
}