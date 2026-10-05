using System;
using System.Collections;
using System.Globalization;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Sensitivity")]
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private TMP_InputField sensitivityInput;

    [Header("Movement Key Rebinding")]
    [SerializeField] private InputActionReference playerMovementReference;
    [SerializeField] private Button moveForwardKeyInput;
    [SerializeField] private TMP_Text moveForwardKeyLabel;
    [SerializeField] private Button moveLeftKeyInput;
    [SerializeField] private TMP_Text moveLeftKeyLabel;
    [SerializeField] private Button moveBackwardKeyInput;
    [SerializeField] private TMP_Text moveBackwardKeyLabel;
    [SerializeField] private Button moveRightKeyInput;
    [SerializeField] private TMP_Text moveRightKeyLabel;

    [Header("Jump Rebinding")]
    [SerializeField] private InputActionReference jumpActionReference;
    [SerializeField] private Button jumpKeyInput;
    [SerializeField] private TMP_Text jumpKeyLabel;

    [Header("Sprint Rebinding")]
    [SerializeField] private InputActionReference sprintActionReference;
    [SerializeField] private Button sprintKeyInput;
    [SerializeField] private TMP_Text sprintKeyLabel;

    [Header("Action Rebinding")]
    [SerializeField] private InputActionReference Action1Reference;
    [SerializeField] private Button Action1KeyInput;
    [SerializeField] private TMP_Text Action1KeyLabel;
    [SerializeField] private InputActionReference Action2Reference;
    [SerializeField] private Button Action2KeyInput;
    [SerializeField] private TMP_Text Action2KeyLabel;

    private struct KeybindEntry
    {
        public InputActionReference actionReference;
        public string compositePartName;
        public Button inputField;
        public TMP_Text labelText;
        public int bindingIndex;
        public bool excludeMouse;
        public int resolvedBindingIndex;
    }

    private KeybindEntry[] keybindEntries;
    private InputActionRebindingExtensions.RebindingOperation activeRebindOperation;

    private SettingsManager settingsManager;

    // ---------------- Unity Lifecycle ----------------

    private void Awake()
    {
        settingsManager = UnityEngine.Object.FindAnyObjectByType<SettingsManager>();

        SetupSensitivityUI();
        SetupKeybindUI();
    }

    private void OnEnable()
    {
        if (settingsManager == null) return;

        float percentage = Mathf.InverseLerp(10f, 500f, settingsManager.MouseSensitivity) * 100f;
        UpdateUI(percentage);

        RefreshAllKeybindDisplays();
    }

    private void OnDisable()
    {
        CancelActiveRebind();
    }

    private void OnDestroy()
    {
        sensitivitySlider.onValueChanged.RemoveListener(OnSensitivitySliderChanged);
        sensitivityInput.onEndEdit.RemoveListener(OnSensitivityInputChanged);

        CancelActiveRebind();
    }

    // ---------------- Setup ----------------

    private void SetupSensitivityUI()
    {
        sensitivitySlider.minValue = 0f;
        sensitivitySlider.maxValue = 100f;
        sensitivitySlider.wholeNumbers = false;

        sensitivityInput.contentType = TMP_InputField.ContentType.DecimalNumber;
        sensitivitySlider.onValueChanged.AddListener(OnSensitivitySliderChanged);
        sensitivityInput.onEndEdit.AddListener(OnSensitivityInputChanged);
    }

    private void SetupKeybindUI()
    {
        keybindEntries = new KeybindEntry[]
        {
            // Movement 
            new KeybindEntry { actionReference = playerMovementReference, compositePartName = "up",    inputField = moveForwardKeyInput,  labelText = moveForwardKeyLabel,  excludeMouse = false },
            new KeybindEntry { actionReference = playerMovementReference, compositePartName = "left",  inputField = moveLeftKeyInput,     labelText = moveLeftKeyLabel,     excludeMouse = false },
            new KeybindEntry { actionReference = playerMovementReference, compositePartName = "down",  inputField = moveBackwardKeyInput, labelText = moveBackwardKeyLabel, excludeMouse = false },
            new KeybindEntry { actionReference = playerMovementReference, compositePartName = "right", inputField = moveRightKeyInput,    labelText = moveRightKeyLabel,    excludeMouse = false },
            // Jump
            new KeybindEntry { actionReference = jumpActionReference, bindingIndex = 0, inputField = jumpKeyInput, labelText = jumpKeyLabel, excludeMouse = false },
            // Sprint
            new KeybindEntry { actionReference = sprintActionReference, bindingIndex = 0, inputField = sprintKeyInput, labelText = sprintKeyLabel, excludeMouse = false },
            // Action 1
            new KeybindEntry { actionReference = Action1Reference, bindingIndex = 0, inputField = Action1KeyInput, labelText = Action1KeyLabel, excludeMouse = false },
            // Action 2
            new KeybindEntry { actionReference = Action2Reference, bindingIndex = 0, inputField = Action2KeyInput, labelText = Action2KeyLabel, excludeMouse = false },
        };

        for (int i = 0; i < keybindEntries.Length; i++)
        {
            ref KeybindEntry entry = ref keybindEntries[i];

            if (entry.actionReference == null || entry.actionReference.action == null)
            {
                Debug.LogWarning($"SettingsMenu: keybind entry {i} has no assigned action reference; skipping.");
                entry.resolvedBindingIndex = -1;
                continue;
            }

            InputAction action = entry.actionReference.action;

            entry.resolvedBindingIndex = entry.compositePartName != null
                ? FindCompositePartBindingIndex(action, entry.compositePartName)
                : ValidateBindingIndex(action, entry.bindingIndex);

            Button field = entry.inputField;
            if (field == null) continue;

            int capturedIndex = i; // avoid closure-over-loop-variable bug
            field.onClick.AddListener(() => StartRebind(capturedIndex));
        }

        RefreshAllKeybindDisplays();
    }

    // ---------------- Sensitivity ----------------

    private void OnSensitivitySliderChanged(float percentage)
    {
        float sensitivity = Mathf.Lerp(10f, 500f, percentage / 100f);
        settingsManager.SetSensitivity(sensitivity);

        UpdateUI(percentage);
    }

    private void OnSensitivityInputChanged(string input)
    {
        if (!float.TryParse(input, NumberStyles.Float, CultureInfo.CurrentCulture, out float percentage))
        {
            UpdateUI(sensitivitySlider.value);
            return;
        }
        percentage = Mathf.Clamp(percentage, 0f, 100f);
        OnSensitivitySliderChanged(percentage);
    }

    private void UpdateUI(float percentage)
    {
        sensitivitySlider.SetValueWithoutNotify(percentage);
        sensitivityInput.SetTextWithoutNotify(percentage.ToString("0.##") + "%");
    }

    // ---------------- Keybind Resolution Helpers ----------------

    private int FindCompositePartBindingIndex(InputAction action, string partName)
    {
        var bindings = action.bindings;
        for (int i = 0; i < bindings.Count; i++)
        {
            if (bindings[i].isPartOfComposite &&
                string.Equals(bindings[i].name, partName, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        Debug.LogWarning($"SettingsMenu: could not find composite part '{partName}' on action '{action.name}'.");
        return -1;
    }

    private int ValidateBindingIndex(InputAction action, int index)
    {
        if (index < 0 || index >= action.bindings.Count)
        {
            Debug.LogWarning($"SettingsMenu: binding index {index} out of range on action '{action.name}' (has {action.bindings.Count} bindings).");
            return -1;
        }

        if (action.bindings[index].isComposite)
        {
            Debug.LogWarning($"SettingsMenu: binding index {index} on action '{action.name}' is a composite header, not a bindable control.");
            return -1;
        }

        return index;
    }

    // ---------------- Conflict Checking ----------------

    private bool IsPathAlreadyUsed(int excludingEntryIndex, string path)
    {
        if (string.IsNullOrEmpty(path)) return false;

        for (int i = 0; i < keybindEntries.Length; i++)
        {
            if (i == excludingEntryIndex) continue;

            KeybindEntry other = keybindEntries[i];
            if (other.actionReference == null || other.resolvedBindingIndex < 0) continue;

            string otherPath = other.actionReference.action.bindings[other.resolvedBindingIndex].effectivePath;
            if (string.Equals(otherPath, path, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private void RestoreBinding(InputAction action, int bindingIndex, string previousPath)
    {
        // If the previous path was just the binding's original default, clear the override
        // entirely rather than re-applying it as an explicit override.
        if (string.Equals(previousPath, action.bindings[bindingIndex].path, StringComparison.OrdinalIgnoreCase))
        {
            action.RemoveBindingOverride(bindingIndex);
        }
        else
        {
            action.ApplyBindingOverride(bindingIndex, previousPath);
        }
    }

    private IEnumerator FlashConflictMessage(int entryIndex)
    {
        TMP_Text label = keybindEntries[entryIndex].labelText;
        if (label != null)
        {
            label.text = "Already in use!";
        }

        yield return new WaitForSecondsRealtime(1f);

        RefreshKeybindDisplay(entryIndex);
    }

    // ---------------- Rebind Execution ----------------

    private void StartRebind(int entryIndex)
    {
        if (activeRebindOperation != null) return; // already rebinding something

        KeybindEntry entry = keybindEntries[entryIndex];
        if (entry.actionReference == null || entry.resolvedBindingIndex < 0 || entry.inputField == null) return;

        InputAction action = entry.actionReference.action;
        string previousPath = action.bindings[entry.resolvedBindingIndex].effectivePath;

        if (entry.labelText != null)
        {
            entry.labelText.text = "Press any key...";
        }

        action.Disable();

        var rebind = action.PerformInteractiveRebinding(entry.resolvedBindingIndex)
            .WithCancelingThrough("<Keyboard>/escape")
            .OnMatchWaitForAnother(0.1f);

        if (entry.excludeMouse)
        {
            rebind = rebind.WithControlsExcluding("Mouse");
        }

        activeRebindOperation = rebind
            .OnComplete(operation => FinishRebind(entryIndex, operation, cancelled: false, previousPath))
            .OnCancel(operation => FinishRebind(entryIndex, operation, cancelled: true, previousPath))
            .Start();
    }

    private void FinishRebind(int entryIndex, InputActionRebindingExtensions.RebindingOperation operation, bool cancelled, string previousPath)
    {
        InputAction action = keybindEntries[entryIndex].actionReference.action;
        action.Enable();

        operation.Dispose();
        activeRebindOperation = null;

        if (!cancelled)
        {
            int bindingIndex = keybindEntries[entryIndex].resolvedBindingIndex;
            string newPath = action.bindings[bindingIndex].effectivePath;

            if (IsPathAlreadyUsed(entryIndex, newPath))
            {
                Debug.LogWarning($"SettingsMenu: '{newPath}' is already bound to another action; reverting.");
                RestoreBinding(action, bindingIndex, previousPath);
                StartCoroutine(FlashConflictMessage(entryIndex));
                return;
            }
        }

        RefreshKeybindDisplay(entryIndex);
    }

    private void CancelActiveRebind()
    {
        if (activeRebindOperation == null) return;

        activeRebindOperation.Cancel(); // triggers OnCancel -> FinishRebind
    }

    // ---------------- Display Refresh ----------------

    private void RefreshAllKeybindDisplays()
    {
        if (keybindEntries == null) return;

        for (int i = 0; i < keybindEntries.Length; i++)
        {
            RefreshKeybindDisplay(i);
        }
    }

    private void RefreshKeybindDisplay(int entryIndex)
    {
        KeybindEntry entry = keybindEntries[entryIndex];
        if (entry.labelText == null || entry.resolvedBindingIndex < 0 || entry.actionReference == null) return;

        string displayString = entry.actionReference.action.GetBindingDisplayString(entry.resolvedBindingIndex);
        entry.labelText.text = displayString;
    }
}