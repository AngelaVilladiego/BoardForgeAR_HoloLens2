using MixedReality.Toolkit;
using MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public class ObjectPlaceManager2 : Solver
{
    [SerializeField]
    [Tooltip("If true, TapToPlace behaviour will start immediately; otherwise, the behaviour will need to be started intentionally.")]
    private bool startPlaceable = true;

    [SerializeField]
    private Material defaultMaterial;

    [SerializeField]
    private Material materialWhilePlacing;

    [SerializeField]
    [Tooltip("The Game Object with visible mesh this object may be wrapping")]
    private GameObject controlledGameObject;

    [SerializeField]
    private GameObject ARPlaneManager;

    [SerializeField]
    private GameObject ARMeshManager;

    [SerializeField]
    [Tooltip("This event is triggered once when the game object to place is being placed.")]
    private UnityEvent onPlacingStarted = new UnityEvent();
       

    /// <summary>
    /// This event is triggered once when the game object to place is selected.
    /// </summary>
    public UnityEvent OnPlacingStarted
    {
        get => onPlacingStarted;
        set => onPlacingStarted = value;
    }

    [SerializeField]
    [Tooltip("This event is triggered once when the game object to place is finished being placed.")]
    private UnityEvent onPlacingStopped = new UnityEvent();

    /// <summary>
    /// This event is triggered once when the game object to place is unselected, placed.
    /// </summary>
    public UnityEvent OnPlacingStopped
    {
        get => onPlacingStopped;
        set => onPlacingStopped = value;
    }

    private bool isPlacing = false;

    /// <summary>
    /// If true, the game object to place will start out selected.  The object will immediately start
    /// following the TrackedTargetType (Head or Controller Ray) and then a tap is required to place the object.  
    /// This value must be modified before Start() is invoked in order to have any effect.
    /// </summary>
    public bool StartPlaceable
    {
        get => startPlaceable;
        set => startPlaceable = value;
    }

    private SurfaceMagnetism surfaceMagnetism;

    // Used to obtain list of known interactors
    private XRInteractionManager interactionManager;

    // Used to cache a known set of interactors
    private List<IXRInteractor> interactorsCache;

    // Used to cache a known set of input interactor select button readers, and used to query their performed actions.
    private List<XRInputButtonReader> interactorSelectButtonReaders;

    /// <summary>
    /// Get or set the last time a clicked occurred.
    /// </summary>
    /// <remarks>
    /// This is used to record the time, in seconds, between `OnPointerClicked` calls 
    /// so to avoid two calls in a row.
    /// </remarks>
    private float lastTimeClicked = 0;

    /// <summary>
    /// Get or set the max time, in seconds, between clicks used when recognizing double clicks.
    /// </summary>
    protected float DoubleClickTimeout { get; set; } = 0.5f;

    // Start is called before the first frame update
    /// <inheritdoc/>
    protected override void Start()
    {
        base.Start();

        surfaceMagnetism = GetComponent<SurfaceMagnetism>();

        if (gameObject == null)
        {
            controlledGameObject = gameObject;
        }

        if (startPlaceable)
        {
            BeginPlacement();
        }
    }


    void BeginPlacement()
    {
        // Checking the amount of time passed between when StartPlacement or StopPlacementViaPerformedAction is called twice in
        // succession. If these methods are called twice very rapidly, the object will be
        // selected and then immediately unselected. If two calls occur within the
        // double click timeout, then return to prevent an immediate object state switch.
        // Also, check that time is no 0 to allow for auto start functionality.
        if (Time.time != 0 && (Time.time - lastTimeClicked) < DoubleClickTimeout)
        {
            return;
        }

        // Added for code configurability to avoid multiple calls to StartPlacement in a row
        if (!isPlacing)
        {
            ARPlaneManager.SetActive(true);
            //ARMeshManager.GetComponent<ARMeshManager>().meshPrefab.GetComponent<Renderer>().material = null;

            controlledGameObject.GetComponent<Renderer>().material = materialWhilePlacing;

            // Get the time of this click action
            lastTimeClicked = Time.time;

            //// Store the initial game object layer
            //GameObjectLayer = gameObject.layer;

            //// Temporarily change the game object layer to IgnoreRaycastLayer to enable a surface hit beyond the game object
            //gameObject.layer = ignoreRaycastLayer;

            SolverHandler.UpdateSolvers = true;

            isPlacing = true;
            surfaceMagnetism.enabled = true;

            OnPlacingStarted?.Invoke();

            RegisterPlacementAction();
        }
        
    }

    private static readonly ProfilerMarker StopPlacementPerfMarker =
          new ProfilerMarker("[CustomMRTK] ObjectPlaceManager.StopPlacement");

    void EndPlacement()
    {

        // Checking the amount of time passed between when StartPlacement or StopPlacementViaPerformedAction is called twice in
        // succession. If these methods are called twice very rapidly, the object will be
        // selected and then immediately unselected. If two calls occur within the
        // double click timeout, then return to prevent an immediate object state switch.
        if ((Time.time - lastTimeClicked) < DoubleClickTimeout)
        {
            return;
        }

        using (StopPlacementPerfMarker.Auto())
        {
            // Added for code configurability to avoid multiple calls to StopPlacementViaPerformedAction in a row
            if (isPlacing)
            {
                ARPlaneManager.SetActive(false);
                ARMeshManager.GetComponent<ARMeshManager>().meshPrefab.GetComponent<MeshRenderer>().material = null;

                controlledGameObject.GetComponent<Renderer>().material = defaultMaterial;

                // Get the time of this click action
                lastTimeClicked = Time.time;

                // Change the game object layer back to the game object's layer on start
                //gameObject.layer = GameObjectLayer;

                //SolverHandler.UpdateSolvers = false;

                isPlacing = false;
                surfaceMagnetism.enabled = false;

                OnPlacingStopped?.Invoke();

                UnregisterPlacementAction();
            }
        }
    }


    /// <summary>
    /// Get if an interactor's select button was performed this frame.
    /// </summary>
    private bool InteractorSelectPerformedThisFrame()
    {
        if (interactorSelectButtonReaders == null || interactorSelectButtonReaders.Count == 0)
        {
            return false;
        }

        foreach (XRInputButtonReader reader in interactorSelectButtonReaders)
        {
            if (reader.ReadWasPerformedThisFrame())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// A Unity event function that is called when the script component has been disabled.
    /// </summary>
    protected override void OnDisable()
    {
        EndPlacement();
        base.OnDisable();
    }

    private static readonly ProfilerMarker SolverUpdatePerfMarker =
            new ProfilerMarker("[CustomMRTK] ObjectPlaceManager.SolverUpdate");

    /// <inheritdoc/>
    public override void SolverUpdate()
    {
        using (SolverUpdatePerfMarker.Auto())
        {
            // Stop placement if a select action is performed this frame
            if (InteractorSelectPerformedThisFrame())
            {
                Debug.Log("+++++ CLICK ACTION PERFORMED +++++ ");
                if (isPlacing)
                {
                    Debug.Log("+++++ ATTEMPTING TO END PLACEMENT +++++ ");
                    EndPlacement();
                }
                else
                {
                    Debug.Log("+++++ ATTEMPTING TO RE-BEGIN PLACEMENT +++++ ");
                    BeginPlacement();
                }
            }
        }
    }

    /// <summary>
    /// Registers the input action which performs placement.
    /// </summary>
    private void RegisterPlacementAction()
    {
        // Refresh the registeration if they already exist
        UnregisterPlacementAction();

        if (interactionManager == null)
        {
            interactionManager = ComponentCache<XRInteractionManager>.FindFirstActiveInstance();
            if (interactionManager == null)
            {
                Debug.LogError("No interaction manager found in scene. Please add an interaction manager to the scene.");
            }
        }

        if (interactorsCache == null)
        {
            interactorsCache = new List<IXRInteractor>();
        }

        if (interactorSelectButtonReaders == null)
        {
            interactorSelectButtonReaders = new List<XRInputButtonReader>();
        }

        // Try registering for the controller's "action" so object selection isn't required for placement.
        // If no controller, then fallback to using object selections for placement.
        interactionManager.GetRegisteredInteractors(interactorsCache);
        foreach (IXRInteractor interactor in interactorsCache)
        {
#pragma warning disable CS0618 // ActionBasedController and XRBaseInputInteractor.forceDeprecatedInput are obsolete
            if (interactor is XRBaseInputInteractor controllerInteractor &&
                controllerInteractor.forceDeprecatedInput &&
                controllerInteractor.xrController is ActionBasedController actionController)
            {
                actionController.selectAction.action.performed += StopPlacementViaPerformedAction;
            }
#pragma warning restore CS0618 // ActionBasedController and XRBaseInputInteractor.forceDeprecatedInput are obsolete
            else
            {
                if (interactor is XRBaseInputInteractor inputInteractor &&
                    inputInteractor.selectInput != null)
                {
                    interactorSelectButtonReaders.Add(inputInteractor.selectInput);
                }

                if (interactor is IXRSelectInteractor selectInteractor)
                {
                    selectInteractor.selectEntered.AddListener(StopPlacementViaSelect);
                }
            }
        }
    }

    /// <summary>
    /// Unregisters the input action which performs placement.
    /// </summary>
    private void UnregisterPlacementAction()
    {
        if (interactorsCache != null)
        {
            foreach (IXRInteractor interactor in interactorsCache)
            {
#pragma warning disable CS0618 // ActionBasedController and XRBaseInputInteractor.forceDeprecatedInput are obsolete
                if (interactor is XRBaseInputInteractor controllerInteractor &&
                    controllerInteractor.forceDeprecatedInput &&
                    controllerInteractor.xrController is ActionBasedController actionController)
                {
                    actionController.selectAction.action.performed -= StopPlacementViaPerformedAction;
                }
#pragma warning restore CS0618 // ActionBasedController and XRBaseInputInteractor.forceDeprecatedInput are obsolete
                else if (interactor is IXRSelectInteractor selectInteractor)
                {
                    selectInteractor.selectEntered.RemoveListener(StopPlacementViaSelect);
                }
            }
            interactorsCache.Clear();
            interactorSelectButtonReaders.Clear();
        }
    }

    /// <summary>
    /// Stop the placement of a game object via an action's performance.
    /// </summary>
    private void StopPlacementViaPerformedAction(InputAction.CallbackContext context)
    {
        EndPlacement();
    }

    /// <summary>
    /// Stop the placement of a game object via an interactor's select event.
    /// </summary>
    private void StopPlacementViaSelect(SelectEnterEventArgs args)
    {
        EndPlacement();
    }

}
