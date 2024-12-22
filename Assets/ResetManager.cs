using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ResetManager : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onBoardClearRequested = new UnityEvent();

    public UnityEvent OnBoardClearRequested
    {
        get => onBoardClearRequested;
        set => onBoardClearRequested = value;
    }

    [SerializeField]
    private UnityEvent onRestartRequested = new UnityEvent();

    public UnityEvent OnRestartRequested
    {
        get => onRestartRequested;
        set => onRestartRequested = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RequestBoardClear()
    {
        OnBoardClearRequested?.Invoke();
    }

    public void RequestRestart()
    {
        OnRestartRequested?.Invoke();
    }

    public void Restart()
    {
        ItemPlaceManager.Instance.Clear();
        BoardManager.Instance.Reset();
    }

    public void ClearBoard()
    {
        ItemPlaceManager.Instance.Clear();
    }
}
