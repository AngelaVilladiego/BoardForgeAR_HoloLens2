using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;

    [SerializeField]
    private GameObject MainSolverObject;

    [SerializeField]
    private GameObject BoardObject;

    [SerializeField]
    private GameObject BoardMenuObject;

    [SerializeField]
    private GameObject HintTextObject;

    [SerializeField]
    private GameObject BoardWrapper;

    [SerializeField]
    private GameObject boundsVisualsPrefab;

    [SerializeField]
    [Tooltip("This event is triggered once when the board begins to be manipulated (placed, rotated, and/or scaled).")]
    private UnityEvent onBoardManipulationBegun = new UnityEvent();

    public UnityEvent OnBoardManipulationBegun
    {
        get => onBoardManipulationBegun;
        set => onBoardManipulationBegun = value;
    }

    [SerializeField]
    [Tooltip("This event is triggered once when the game object to place is no longer being manipulated.")]
    private UnityEvent onBoardManipulationEnded = new UnityEvent();

    public UnityEvent OnBoardManipulationEnded
    {
        get => onBoardManipulationEnded;
        set => onBoardManipulationEnded = value;
    }

    [SerializeField]
    [Tooltip("This event is triggered once when the game object to place is no longer being manipulated.")]
    private UnityEvent onFinalizeBoardRequest = new UnityEvent();

    public UnityEvent OnFinalizeBoardRequest
    {
        get => onFinalizeBoardRequest;
        set => onFinalizeBoardRequest = value;
    }

    private bool isBeingManipulated = false;

    public enum GameStage
    {
        Board_Home,
        Board_Place,
        Board_Rotate,
        Board_Scale,
    }

    private TextMeshPro textMeshPro;
    private ObjectManipulator objectManipulator;

    private GameStage stage;

    private BoundsControl boundsControl;

    private void Awake()
    {
        // Enforcing Singleton pattern
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        textMeshPro = HintTextObject.GetComponent<TextMeshPro>();
        StartBoardPlaceStage();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetStage(GameStage gameStage)
    {
        if (boundsControl != null)
        {
            RemoveBoundsControl(BoardWrapper);

            // Delay further initialization to the end of the frame
            StartCoroutine(InitializeStageNextFrame(gameStage));
            return;
        }

        InitializeStage(gameStage); // Initial stage setup
    }

    private IEnumerator InitializeStageNextFrame(GameStage gameStage)
    {
        yield return new WaitForEndOfFrame();
        InitializeStage(gameStage);
    }

    private void InitializeStage(GameStage gameStage)
    {
        stage = gameStage;

        // If the board is in a state of manipulation and will switch to a non-manipulation stage
        if (isBeingManipulated && gameStage == GameStage.Board_Home)
        {
            OnBoardManipulationEnded?.Invoke();
            isBeingManipulated = false;
        }

        // If the board not in a state of manipulation and will switch to a manipulation stage
        if (!isBeingManipulated && gameStage != GameStage.Board_Home)
        {
            OnBoardManipulationBegun?.Invoke();
            isBeingManipulated = true;
        }

        switch (stage)
        {
            case GameStage.Board_Place:
                MainSolverObject.GetComponent<ObjectPlaceManager>().BeginPlacement();

                textMeshPro.SetText("Look around to move.\nClick fingers to set placement.");
                BoardMenuObject.SetActive(false);
                HintTextObject.SetActive(true);
                break;

            case GameStage.Board_Rotate:

                HintTextObject.SetActive(false);

                boundsControl = InitializeBoundsControl(BoardWrapper);
                boundsControl.FlattenMode = FlattenMode.Never;
                objectManipulator.AllowedManipulations = MixedReality.Toolkit.TransformFlags.Rotate;
                var rotateConstraint = BoardWrapper.AddComponent<RotationAxisConstraint>();
                rotateConstraint.ConstraintOnRotation = MixedReality.Toolkit.AxisFlags.XAxis | MixedReality.Toolkit.AxisFlags.ZAxis;
                rotateConstraint.UseLocalSpaceForConstraint = true;
                boundsControl.RotateAnchor = RotateAnchorType.BoundsCenter;
                boundsControl.EnabledHandles = HandleType.Rotation;
                break;

            case GameStage.Board_Scale:
                HintTextObject.SetActive(false);

                boundsControl = InitializeBoundsControl(BoardWrapper);
                boundsControl.FlattenMode = FlattenMode.Always;
                objectManipulator.AllowedManipulations = MixedReality.Toolkit.TransformFlags.Scale;
                var scaleConstraint = BoardWrapper.AddComponent<MinMaxScaleConstraint>();
                scaleConstraint.MaximumScale = new Vector3(2f, 1f, 2f);
                scaleConstraint.MinimumScale = new Vector3(0.2f, 1f, 0.2f);
                scaleConstraint.RelativeToInitialState = false;
                boundsControl.ScaleAnchor = ScaleAnchorType.OppositeCorner;
                boundsControl.ScaleBehavior = HandleScaleMode.NonUniform;
                boundsControl.EnabledHandles = HandleType.Scale;
                break;

            case GameStage.Board_Home:
                HintTextObject.SetActive(false);
                if (objectManipulator != null)
                    objectManipulator.enabled = false;
                break;

            default:
                break;
        }
    }

    public void StartBoardPlaceStage() => SetStage(GameStage.Board_Place);

    public void StartBoardRotateStage() => SetStage(GameStage.Board_Rotate);

    public void StartBoardScaleStage() => SetStage(GameStage.Board_Scale);

    public void StartBoardHomeStage() => SetStage(GameStage.Board_Home);

    public void RequestFinalizeBoard()
    {
        OnFinalizeBoardRequest?.Invoke();
    }

    public void FinalizeBoard()
    {
        if (boundsControl != null)
        {
            RemoveBoundsControl(BoardWrapper);
        }

        BoardMenuObject.SetActive(false);
    }

    public void OnBoardManipulationEnd()
    {
        SetStage(GameStage.Board_Home);
        BoardMenuObject.SetActive(true);
        HintTextObject.SetActive(false);
    }

    /// <summary>
    /// Set up a BoundsControl component on the target GameObject.
    /// </summary>
    /// <param name="target">The gameobject to set up the BoundsControl on.</param>
    /// <returns>The created <see cref="BoundsControl"/> component</returns>
    private BoundsControl InitializeBoundsControl(GameObject target)
    {
        target.AddComponent<ConstraintManager>();
        objectManipulator = target.AddComponent<ObjectManipulator>();
        var boundsControl = target.AddComponent<BoundsControl>();
        boundsControl.BoundsVisualsPrefab = boundsVisualsPrefab;
        boundsControl.HandlesActive = true;
        boundsControl.DragToggleThreshold = .02f;
        boundsControl.ToggleHandlesOnClick = false;
        var inputAdapter = target.AddComponent<UGUIInputAdapterDraggable>();
        inputAdapter.MovableAxes = MixedReality.Toolkit.AxisFlags.None;

        return boundsControl;
    }

    private void RemoveBoundsControl(GameObject target)
    { 
        Destroy(target.GetComponent<UGUIInputAdapterDraggable>());
        Destroy(target.GetComponent<BoundsControl>());
        Destroy(target.GetComponent<ObjectManipulator>());
        Destroy(target.GetComponent<ConstraintManager>());

        foreach (Transform child in target.transform)
        {
            if (child.gameObject.name.Contains("BoundingBox"))
            {
                Destroy(child.gameObject);
            }
        }

        boundsControl = null;
    }

    public void Reset()
    {
        BoardWrapper.transform.rotation = Quaternion.identity;
        BoardWrapper.transform.localScale = Vector3.one;

        Start();
    }
}
