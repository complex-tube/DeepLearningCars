#region Includes
using UnityEngine;
using UnityEngine.UI;
#endregion

public class UIController : MonoBehaviour
{
    #region Members
    public Canvas Canvas
    {
        get;
        private set;
    }

    private UISimulationController simulationUI;
    #endregion

    #region Constructors
    void Awake()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.UIController = this;

        Canvas = GetComponent<Canvas>();
        simulationUI = GetComponentInChildren<UISimulationController>(true);

        simulationUI.Show();
    }
    #endregion

    #region Methods
    public void SetDisplayTarget(CarController target)
    {
        simulationUI.Target = target;
    }
    #endregion
}
