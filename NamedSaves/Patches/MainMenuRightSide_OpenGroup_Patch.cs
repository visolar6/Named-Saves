using HarmonyLib;
using UnityEngine;
using NamedSaves.Utilities;
using System;

namespace NamedSaves.Patches
{
    [HarmonyPatch(typeof(MainMenuRightSide), nameof(MainMenuRightSide.OpenGroup))]
    public static class MainMenuRightSide_OpenGroup_Patch
    {
        static void Postfix(MainMenuRightSide __instance, string target)
        {
            if (target != "SavedGames") return;

            foreach (var group in __instance.groups)
            {
                if (group.gameObject.name == "SavedGames")
                {
                    var tmpType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
                    if (tmpType != null)
                    {
                        var tmps = group.gameObject.GetComponentsInChildren(tmpType, true);
                        foreach (var tmp in tmps)
                        {
                            var tmpTextProp = tmpType.GetProperty("text");
                            var tmpNameProp = tmpType.GetProperty("name");
                            string name = tmpNameProp?.GetValue(tmp)?.ToString() ?? "(unknown)";
                            string value = tmpTextProp?.GetValue(tmp)?.ToString() ?? "(null)";
                            if (name == "SaveGameMode")
                            {
                                var comp = tmp;
                                string? saveId = null;
                                if (comp != null)
                                {
                                    // Traverse up to ancestor level 2 to get the Save ID
                                    var t = comp.transform;
                                    int ancestorLevel = 0;
                                    while (t != null && ancestorLevel < 3)
                                    {
                                        if (ancestorLevel == 2)
                                            saveId = t.gameObject.name;
                                        t = t.parent;
                                        ancestorLevel++;
                                    }
                                }
                                // Look up custom name from config, and if not found, set empty entry
                                string? customName = null;
                                if (!string.IsNullOrEmpty(saveId))
                                {
                                    customName = NamedSavesConfig.GetCustomName(saveId!);
                                    if (customName == null)
                                    {
                                        NamedSavesConfig.SetCustomName(saveId!, "");
                                    }
                                }
                                // Replace game mode text with custom name, or use game mode as fallback
                                // Wrap in red color only if showing a custom name
                                string displayText;
                                if (!string.IsNullOrEmpty(customName))
                                {
                                    displayText = GetCustomNameDisplayText(customName!);
                                }
                                else
                                {
                                    displayText = GetGameModeDisplayText(value);
                                }
                                tmpTextProp?.SetValue(tmp, displayText);
                                // Shift text down by 1 pixel
                                var textRect = comp?.GetComponent<RectTransform>();
                                if (textRect != null)
                                {
                                    var pos = textRect.anchoredPosition;
                                    textRect.anchoredPosition = new Vector2(pos.x, pos.y - 1f);
                                }
                                // --- Add edit button for custom name ---
                                if (!string.IsNullOrEmpty(saveId) && comp != null)
                                {
                                    var parent = comp.transform.parent;
                                    if (parent != null && parent.Find("NamedSavesEditButton") == null)
                                    {
                                        // Create a new GameObject for the button
                                        var buttonGO = new GameObject("NamedSavesEditButton");
                                        buttonGO.transform.SetParent(parent, false);
                                        // Add or get RectTransform
                                        var buttonRect = buttonGO.GetComponent<RectTransform>();
                                        if (buttonRect == null)
                                            buttonRect = buttonGO.AddComponent<RectTransform>();
                                        buttonRect.sizeDelta = new Vector2(48, 24);
                                        // Position button at the far right of the save file box
                                        buttonRect.anchorMin = new Vector2(1, 0.5f);
                                        buttonRect.anchorMax = new Vector2(1, 0.5f);
                                        buttonRect.pivot = new Vector2(1, 0.5f);
                                        buttonRect.anchoredPosition = new Vector2(-80, 0); // 32px from the right edge, vertically centered
                                        // Only add Button (no Image)
                                        var buttonType = typeof(UnityEngine.UI.Button);
                                        var button = buttonGO.GetComponent(buttonType) ?? buttonGO.AddComponent(buttonType);
                                        var interactableProp = buttonType.GetProperty("interactable");
                                        if (interactableProp != null) interactableProp.SetValue(button, true);
                                        // Set button text (using TextMeshProUGUI if available)
                                        var btnTextGO = new GameObject("EditText");
                                        btnTextGO.transform.SetParent(buttonGO.transform, false);
                                        var btnTmpType = Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
                                        if (btnTmpType != null)
                                        {
                                            var btnTmp = btnTextGO.AddComponent(btnTmpType);
                                            // Localize the edit button text
                                            string editButtonText = Language.main.Get("NamedSaves_EditButton") ?? "Edit name";
                                            btnTmpType.GetProperty("text")?.SetValue(btnTmp, editButtonText);
                                            btnTmpType.GetProperty("fontSize")?.SetValue(btnTmp, 10f);
                                            btnTmpType.GetProperty("alignment")?.SetValue(btnTmp, (object)514); // Center
                                            // Make the text receive raycasts so the button is clickable
                                            var raycastProp = btnTmpType.GetProperty("raycastTarget");
                                            if (raycastProp != null) raycastProp.SetValue(btnTmp, true);
                                            var btnTextRect = btnTextGO.GetComponent<RectTransform>();
                                            btnTextRect.anchorMin = Vector2.zero;
                                            btnTextRect.anchorMax = Vector2.one;
                                            btnTextRect.offsetMin = Vector2.zero;
                                            btnTextRect.offsetMax = Vector2.zero;
                                        }
                                        // Add click event to show input field
                                        var onClickProp = buttonType.GetProperty("onClick");
                                        var onClick = onClickProp?.GetValue(button);
                                        if (onClick != null)
                                        {
                                            var inputType = System.Type.GetType("TMPro.TMP_InputField, Unity.TextMeshPro");
                                            var tmpTextType = System.Type.GetType("TMPro.TextMeshProUGUI, Unity.TextMeshPro");
                                            var action = new UnityEngine.Events.UnityAction(() =>
                                            {
                                                // Create input field overlay directly on top of the custom name text
                                                var inputGO = new GameObject("NamedSavesInputField");
                                                inputGO.transform.SetParent(parent, false);
                                                var inputRect = inputGO.AddComponent<RectTransform>();
                                                var compRect = comp.GetComponent<RectTransform>();
                                                if (compRect != null)
                                                {
                                                    inputRect.anchorMin = compRect.anchorMin;
                                                    inputRect.anchorMax = compRect.anchorMax;
                                                    inputRect.pivot = compRect.pivot;
                                                    inputRect.sizeDelta = compRect.sizeDelta;
                                                    inputRect.anchoredPosition = compRect.anchoredPosition;
                                                }
                                                else
                                                {
                                                    inputRect.anchorMin = new Vector2(0, 1);
                                                    inputRect.anchorMax = new Vector2(0, 1);
                                                    inputRect.pivot = new Vector2(0, 1);
                                                    inputRect.anchoredPosition = new Vector2(0, 0);
                                                    inputRect.sizeDelta = new Vector2(84, 17);
                                                }
                                                // Add a visible background as the first child
                                                var bgGO = new GameObject("BG");
                                                bgGO.transform.SetParent(inputGO.transform, false);
                                                bgGO.transform.SetSiblingIndex(0);
                                                var bgRect = bgGO.AddComponent<RectTransform>();
                                                bgRect.anchorMin = Vector2.zero;
                                                bgRect.anchorMax = Vector2.one;
                                                bgRect.offsetMin = Vector2.zero;
                                                bgRect.offsetMax = Vector2.zero;
                                                var bgImageType = typeof(UnityEngine.UI.Image);
                                                var bgImage = bgGO.AddComponent(bgImageType);
                                                if (bgImage != null)
                                                {
                                                    // Assign a sprite so the background is visible
                                                    var spriteProp = bgImageType.GetProperty("sprite");
                                                    if (spriteProp != null)
                                                    {
                                                        var tex = new Texture2D(1, 1);
                                                        tex.SetPixel(0, 0, Color.white);
                                                        tex.Apply();
                                                        var defaultSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
                                                        spriteProp.SetValue(bgImage, defaultSprite);
                                                    }
                                                    var colorProp = bgImageType.GetProperty("color");
                                                    if (colorProp != null) colorProp.SetValue(bgImage, new Color(0f, 0f, 0f, 1f));
                                                }
                                                // Add TMP_InputField
                                                if (inputType == null || tmpTextType == null) return;
                                                var inputField = inputGO.AddComponent(inputType);
                                                // Set TMP_InputField.targetGraphic to the background image
                                                var targetGraphicProp = inputType.GetProperty("targetGraphic");
                                                if (targetGraphicProp != null && bgImage != null)
                                                    targetGraphicProp.SetValue(inputField, bgImage);
                                                // Set caret (cursor) color and width for visibility
                                                var caretColorProp = inputType.GetProperty("caretColor");
                                                if (caretColorProp != null)
                                                    caretColorProp.SetValue(inputField, Color.white);
                                                var caretWidthProp = inputType.GetProperty("caretWidth");
                                                if (caretWidthProp != null)
                                                    caretWidthProp.SetValue(inputField, 2); // 2px wide caret for visibility
                                                // Ensure the input field is enabled and interactable
                                                var enabledProp = inputType.GetProperty("enabled");
                                                if (enabledProp != null)
                                                    enabledProp.SetValue(inputField, true);
                                                var interactableProp2 = inputType.GetProperty("interactable");
                                                if (interactableProp2 != null)
                                                    interactableProp2.SetValue(inputField, true);
                                                inputGO.transform.localScale = Vector3.one;
                                                var cg = inputGO.GetComponent<UnityEngine.CanvasGroup>() ?? inputGO.AddComponent<UnityEngine.CanvasGroup>();
                                                cg.alpha = 1f;
                                                cg.interactable = true;
                                                cg.blocksRaycasts = true;
                                                // Add text component
                                                var textGO = new GameObject("Text");
                                                textGO.transform.SetParent(inputGO.transform, false);
                                                var textRect = textGO.AddComponent<RectTransform>();
                                                textRect.anchorMin = Vector2.zero;
                                                textRect.anchorMax = Vector2.one;
                                                textRect.offsetMin = new Vector2(0, 1); // 1px top padding
                                                textRect.offsetMax = new Vector2(0, -1); // 1px bottom padding
                                                var textComp = textGO.AddComponent(tmpTextType);
                                                if (textComp == null) return;
                                                tmpTextType.GetProperty("fontSize")?.SetValue(textComp, 8f);
                                                tmpTextType.GetProperty("alignment")?.SetValue(textComp, (object)514);
                                                tmpTextType.GetProperty("raycastTarget")?.SetValue(textComp, true);
                                                // Add placeholder component
                                                var placeholderGO = new GameObject("Placeholder");
                                                placeholderGO.transform.SetParent(inputGO.transform, false);
                                                var placeholderRect = placeholderGO.AddComponent<RectTransform>();
                                                placeholderRect.anchorMin = Vector2.zero;
                                                placeholderRect.anchorMax = Vector2.one;
                                                placeholderRect.offsetMin = new Vector2(0, 1); // 1px top padding
                                                placeholderRect.offsetMax = new Vector2(0, -1); // 1px bottom padding
                                                var placeholderComp = placeholderGO.AddComponent(tmpTextType);
                                                if (placeholderComp == null) return;
                                                tmpTextType.GetProperty("fontSize")?.SetValue(placeholderComp, 8f);
                                                tmpTextType.GetProperty("alignment")?.SetValue(placeholderComp, (object)514);
                                                // Localize the input placeholder text
                                                string placeholderText = Language.main.Get("NamedSaves_InputPlaceholder") ?? "Enter custom name...";
                                                tmpTextType.GetProperty("text")?.SetValue(placeholderComp, placeholderText);
                                                tmpTextType.GetProperty("color")?.SetValue(placeholderComp, new Color(1, 1, 1, 0.5f));
                                                tmpTextType.GetProperty("raycastTarget")?.SetValue(placeholderComp, false);
                                                // Assign textComponent and placeholder
                                                var textComponentProp = inputType.GetProperty("textComponent");
                                                var placeholderProp = inputType.GetProperty("placeholder");
                                                if (textComponentProp == null || placeholderProp == null) return;
                                                textComponentProp.SetValue(inputField, textComp);
                                                placeholderProp.SetValue(inputField, placeholderComp);
                                                // Set initial value
                                                // Always fetch the latest custom name from config in case it was just updated
                                                string latestCustomName = string.Empty;
                                                if (!string.IsNullOrEmpty(saveId))
                                                    latestCustomName = NamedSavesConfig.GetCustomName(saveId!) ?? string.Empty;
                                                inputType.GetProperty("text")?.SetValue(inputField, latestCustomName);
                                                // Activate input field for typing and show caret
                                                var activateMethod = inputType.GetMethod("ActivateInputField");
                                                activateMethod?.Invoke(inputField, null);
                                                // Try to select/focus the input field
                                                var selectMethod = inputType.GetMethod("Select");
                                                if (selectMethod != null) selectMethod.Invoke(inputField, null);
                                                // Force-select the input field using EventSystem
                                                var eventSystemType = typeof(UnityEngine.EventSystems.EventSystem);
                                                var currentEventSystemProp = eventSystemType.GetProperty("current");
                                                var currentEventSystem = currentEventSystemProp?.GetValue(null);
                                                if (currentEventSystem != null)
                                                {
                                                    var setSelectedGO = eventSystemType.GetMethod("SetSelectedGameObject", [typeof(GameObject)]);
                                                    if (setSelectedGO != null)
                                                        setSelectedGO.Invoke(currentEventSystem, [inputGO]);
                                                }
                                                // Disable the edit button while editing
                                                if (interactableProp != null) interactableProp.SetValue(button, false);
                                                // On submit or end edit, save and destroy input, re-enable button
                                                var saveAndClose = new UnityEngine.Events.UnityAction<string>((newValue) =>
                                                {
                                                    var safeSaveId = saveId ?? string.Empty;
                                                    var safeValue = value ?? string.Empty;
                                                    var safeNewValue = newValue ?? string.Empty;
                                                    NamedSavesConfig.SetCustomName(safeSaveId, safeNewValue);
                                                    // Update the displayed text to show custom name or fall back to game mode
                                                    string updatedDisplayText = !string.IsNullOrEmpty(safeNewValue) ? GetCustomNameDisplayText(safeNewValue) : GetGameModeDisplayText(safeValue);
                                                    tmpTextType.GetProperty("text")?.SetValue(comp, updatedDisplayText);
                                                    UnityEngine.Object.Destroy(inputGO);
                                                    if (interactableProp != null) interactableProp.SetValue(button, true);
                                                });
                                                // Try to add a direct listener to TMP_InputField.onEndEdit if possible
                                                var onEndEditProp = inputType.GetProperty("onEndEdit");
                                                var tmpInputField = inputGO.GetComponent(inputType);
                                                bool listenerAttached = false;
                                                if (tmpInputField != null && onEndEditProp != null)
                                                {
                                                    var onEndEditEvent = onEndEditProp.GetValue(tmpInputField, null);
                                                    if (onEndEditEvent != null)
                                                    {
                                                        var addListenerMethod_OnEndEdit = onEndEditEvent.GetType().GetMethod("AddListener");
                                                        if (addListenerMethod_OnEndEdit != null)
                                                        {
                                                            addListenerMethod_OnEndEdit.Invoke(onEndEditEvent, [saveAndClose]);
                                                            listenerAttached = true;
                                                        }
                                                    }
                                                }
                                                if (!listenerAttached)
                                                {
                                                    // Add OnDeselect event via EventTrigger as fallback
                                                    var eventTriggerType = typeof(UnityEngine.EventSystems.EventTrigger);
                                                    var eventTrigger = inputGO.GetComponent(eventTriggerType) ?? inputGO.AddComponent(eventTriggerType);
                                                    var entryType = typeof(UnityEngine.EventSystems.EventTrigger.Entry);
                                                    var eventTypeField = entryType.GetField("eventID");
                                                    var callbackField = entryType.GetField("callback");
                                                    var eventTriggerTypeEnum = typeof(UnityEngine.EventSystems.EventTriggerType);
                                                    var onDeselectValue = System.Enum.Parse(eventTriggerTypeEnum, "Deselect");
                                                    var entry = System.Activator.CreateInstance(entryType);
                                                    if (entry == null || eventTypeField == null || callbackField == null) return;
                                                    eventTypeField.SetValue(entry, onDeselectValue);
                                                    UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData> deselectAction = (eventData) =>
                                                    {
                                                        var textProp = inputType.GetProperty("text");
                                                        string textValue = textProp?.GetValue(inputField)?.ToString() ?? "";
                                                        saveAndClose.Invoke(textValue);
                                                    };
                                                    var callback = System.Activator.CreateInstance(typeof(UnityEngine.EventSystems.EventTrigger.TriggerEvent));
                                                    var addListenerMethod = callback.GetType().GetMethod("AddListener");
                                                    if (addListenerMethod == null) return;
                                                    addListenerMethod.Invoke(callback, [deselectAction]);
                                                    callbackField.SetValue(entry, callback);
                                                    var triggersListField = eventTriggerType.GetField("triggers");
                                                    var triggersListProp = eventTriggerType.GetProperty("triggers");
                                                    object? triggersObj = null;
                                                    if (triggersListField != null)
                                                        triggersObj = triggersListField.GetValue(eventTrigger);
                                                    else if (triggersListProp != null)
                                                        triggersObj = triggersListProp.GetValue(eventTrigger);
                                                    else
                                                        return;
                                                    var list = triggersObj as System.Collections.IList;
                                                    if (list == null) return;
                                                    list.Add(entry);
                                                }
                                            });
                                            var addListener = onClick.GetType().GetMethod("AddListener");
                                            if (addListener != null)
                                                addListener.Invoke(onClick, [action]);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Plugin.Log?.LogInfo("[NamedSaves] TextMeshProUGUI type not found.");
                    }
                }
            }
        }

        static string GetGameModeDisplayText(string gameMode)
        {
            return GetDisplayText(gameMode, "#ffffff", 10);
        }

        static string GetCustomNameDisplayText(string customName)
        {
            return GetDisplayText(customName, "#4487ff", 10);
        }

        static string GetDisplayText(string value, string color, int size = 10)
        {
            return $"<size={size}><color={color}>{value}</color></size>";
        }
    }
}
