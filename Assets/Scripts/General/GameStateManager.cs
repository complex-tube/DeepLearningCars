#region Includes
using UnityEngine;
using UnityEngine.SceneManagement;
#endregion

/// Класс представляющий управление всей симуляцией
public class GameStateManager : MonoBehaviour
{
    #region Members
    // Объект камеры
    [SerializeField]
    private CameraMovement Camera;

    // Имя трассы, которая будет загружена
    [SerializeField]
    public string TrackName;

    /// Объект управления пользовательским интерфейсом
    public UIController UIController
    {
        get;
        set;
    }

    public static GameStateManager Instance
    {
        get;
        private set;
    }

    private CarController prevBest, prevSecondBest;
    #endregion

    #region Constructors
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Multiple GameStateManagers in the Scene.");
            return;
        }
        Instance = this;

        //Загрузка сцены пользовательского интерфейса
        SceneManager.LoadScene("GUI", LoadSceneMode.Additive);

        //Загрузка сцены трека
        SceneManager.LoadScene(TrackName, LoadSceneMode.Additive);
    }

    void Start ()
    {
        TrackManager.Instance.BestCarChanged += OnBestCarChanged;
        EvolutionManager.Instance.StartEvolution();
	}
    #endregion

    #region Methods
    // Callback метод, вызывается после изменения лучшей машины
    private void OnBestCarChanged(CarController bestCar)
    {
        if (bestCar == null)
            Camera.SetTarget(null);
        else
            Camera.SetTarget(bestCar.gameObject);
            
        if (UIController != null)
            UIController.SetDisplayTarget(bestCar);
    }
    #endregion
}
