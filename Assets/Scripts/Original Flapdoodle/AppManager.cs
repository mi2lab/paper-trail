using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AppManager : MonoBehaviour
{
    // ***** Singleton *****
    public static AppManager Instance { get; private set; }

    public enum AppState { Preview, Capture, SelectDefault, Selected, AnimateDefault, AnimateTranslation, AnimateRotation, ClippingMasks }
    static AppState appState;

    public GameObject MainToolbar;
    public GameObject CaptureToolbar;
    public GameObject SelectToolbar;
    public GameObject AnimateFunctionsToolbar;
    public GameObject AnimateOptionsToolbar;
    public GameObject ClippingMasksToolbar;

    public GameObject SelectButton;
    public GameObject EndPreviewButton;
    public GameObject CaptureGuides, CaptureButton;

    public GameObject InstructionBackground;    // makes instructions more readable
    public GameObject SelectInstructions;
    public GameObject AnimateSelectInstructions;
    public GameObject AnimateRecordInstructions;
    public GameObject AddClippingMaskInstructions;
    public GameObject DeleteClippingMaskInstructions;

    private Button selectButtonScript;

    private void Awake()
    {   
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        selectButtonScript = SelectButton.GetComponent<Button>();

        appState = AppState.SelectDefault;
        selectButtonScript.Select();
    }

    public AppState GetAppState() { return appState; }

    /// <summary> Need to overload these bc Unity editor is stupid and doesn't accept enum parameters </summary>
    public void SetAppState(AppState newState) 
    {
        UpdateUI(appState, newState);
        UpdateDoodleParameters(appState, newState);
        appState = newState;
        Debug.Log("AppState: " + appState);
    }
    public void SetAppState(string stateString)
    {
        if (System.Enum.TryParse(stateString, out AppState newState))
        {
            if (newState != appState)
            {
                SetAppState(newState);
            }
            else { Debug.Log("AppState is already " + stateString); }
        }
        else { Debug.LogError("AppState " + stateString + " doesn't exist"); }
    }

    /// <summary> What is sure to be a horrendous state machine :D </summary>
    private void UpdateUI(AppState oldState, AppState newState)
    {
        switch (oldState)
        {
            case AppState.Preview:
                MainToolbar.SetActive(true);
                EndPreviewButton.SetActive(false);
                break;
            case AppState.Capture:
                CaptureToolbar.SetActive(false);
                ToggleCaptureButton(false);
                ToggleCaptureGuides(false);
                break;
            case AppState.SelectDefault:
                SelectInstructions.SetActive(false);
                InstructionBackground.SetActive(false);
                break;
            case AppState.Selected:
                SelectToolbar.SetActive(false);
                MainToolbar.SetActive(true);
                break;
            case AppState.AnimateDefault:
                AnimateOptionsToolbar.SetActive(false);
                MainToolbar.SetActive(true);
                AnimateSelectInstructions.SetActive(false);
                InstructionBackground.SetActive(false);
                DoodleManager.Instance.ToggleAllPivots(false);  // if we were in the Rotation animation mode, need to hide the pivots
                break;
            case AppState.AnimateTranslation:
            case AppState.AnimateRotation:
                AnimateRecordInstructions.SetActive(false);
                InstructionBackground.SetActive(false);
                break;
            case AppState.ClippingMasks:
                ClippingMasksToolbar.SetActive(false);
                break;
        }

        switch (newState)
        {
            case AppState.Preview:
                MainToolbar.SetActive(false);
                EndPreviewButton.SetActive(true);
                break;
            case AppState.Capture:
                CaptureToolbar.SetActive(true);
                ImageCapture.Instance.SetCaptureMode("Photo");
                ToggleCaptureButton(true);
                break;
            case AppState.SelectDefault:
                selectButtonScript.Select();
                SelectInstructions.SetActive(true);
                InstructionBackground.SetActive(true);
                break;
            case AppState.Selected:
                MainToolbar.SetActive(false);
                SelectToolbar.SetActive(true);
                break;
            case AppState.AnimateDefault:
                MainToolbar.SetActive(false);
                AnimateFunctionsToolbar.SetActive(false);
                AnimateOptionsToolbar.SetActive(true);
                AnimateSelectInstructions.SetActive(true);
                InstructionBackground.SetActive(true);
                break;
            case AppState.AnimateTranslation:
            case AppState.AnimateRotation:
                AnimateOptionsToolbar.SetActive(false);
                AnimateFunctionsToolbar.SetActive(true);
                AnimateRecordInstructions.SetActive(true);
                InstructionBackground.SetActive(true);
                DoodleManager.Instance.UpdateAnimateFunctionsToolbar(newState);
                break;
            case AppState.ClippingMasks:
                ClippingMasksToolbar.SetActive(true);
                break;
        }
    }

    /// <summary> Handles state-based changes for non-UI things </summary>
    private void UpdateDoodleParameters(AppState oldState, AppState newState)
    {
        Debug.Log("Updating doodle parameters");
        switch (oldState)
        {
            case AppState.AnimateDefault:
                if (newState != AppState.AnimateTranslation && oldState != AppState.AnimateRotation)
                {
                    DoodleManager.Instance.ToggleAllPivots(false);  // keeping this in case something goes wrong :P
                    DoodleManager.Instance.PlayAllAnimations();
                }
                break;
            case AppState.AnimateRotation:
                DoodleManager.Instance.ToggleAllPivots(false);
                DoodleManager.Instance.ToggleComponentsForRotationAnimation();
                break;
        }

        switch (newState)
        {
            case AppState.AnimateDefault:
                DoodleManager.Instance.StopAllAnimations();
                break;
            case AppState.AnimateRotation:
                DoodleManager.Instance.ToggleAllPivots(true);
                DoodleManager.Instance.ToggleComponentsForRotationAnimation();
                break;
        }
    }

    public void ToggleCaptureButton(bool show) { CaptureButton.SetActive(show); }

    public void ToggleCaptureGuides(bool show) { CaptureGuides.SetActive(show); }

    public void ToggleMainToolbar(bool show) { MainToolbar.SetActive(show); }

    public void ToggleCaptureToolbar(bool show) { CaptureToolbar.SetActive(show); }

    #region Clipping masks
    public IEnumerator FlashAddClippingMaskInstructions() {
        InstructionBackground.SetActive(true);
        AddClippingMaskInstructions.SetActive(true);
        yield return new WaitForSeconds(2f);
        AddClippingMaskInstructions.SetActive(false);
        InstructionBackground.SetActive(false);
    }

    public void ToggleDeleteClippingMaskInstructions(bool show)
    {
        DeleteClippingMaskInstructions.SetActive(show);
        InstructionBackground.SetActive(show);
    }
    #endregion
}
