#region Includes
using UnityEngine;
#endregion

/// Класс представляющий управление на объектом 2D машины с 5 сенсорами
public class CarController : MonoBehaviour
{
    #region Members
    #region IDGenerator
    // Уникальный id
    private static int idGenerator = 0;
    
    /// Возвращает следующий уникальный id
    private static int NextID
    {
        get { return idGenerator++; }
    }
    #endregion

    // Максимальная задержка, во время которой машина не собирает чекпойнты
    private const float MAX_CHECKPOINT_DELAY = 7;

    /// Агент машины
    public Agent Agent
    {
        get;
        set;
    }

    public float CurrentCompletionReward
    {
        get { return Agent.Genotype.Evaluation; }
        set { Agent.Genotype.Evaluation = value; }
    }

    /// Возможность управлять машиной мануально
    public bool UseUserInput = false;

    /// Компонент управления машиной
    public CarMovement Movement
    {
        get;
        private set;
    }

    /// Нынешние входные данные для управления
    public double[] CurrentControlInputs
    {
        get { return Movement.CurrentInputs; }
    }

    /// Отрисовка машины
    public SpriteRenderer SpriteRenderer
    {
        get;
        private set;
    }

    private Sensor[] sensors;
    private float timeSinceLastCheckpoint;
    #endregion

    #region Constructors
    void Awake()
    {
        //Использование компонентов движения, спрайтов и сенсоров в коде 
        Movement = GetComponent<CarMovement>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        sensors = GetComponentsInChildren<Sensor>();
    }
    void Start()
    {
        //Предоставление уникального имени машине
        this.name = "Car (" + NextID + ")";
        
        Movement.HitWall += Die;
    }
    #endregion

    #region Methods
    
    /// Перезапуск машины
    public void Restart()
    {
        Movement.enabled = true;
        timeSinceLastCheckpoint = 0;

        foreach (Sensor s in sensors)
            s.Show();

        Agent.Reset();
        this.enabled = true;
    }
    
    void Update()
    {
        timeSinceLastCheckpoint += Time.deltaTime;
    }

    
    void FixedUpdate()
    {
        //Использовать управление от агента
        if (!UseUserInput)
        {
            //Считать сенсоры
            double[] sensorOutput = new double[sensors.Length];
            for (int i = 0; i < sensors.Length; i++)
                sensorOutput[i] = sensors[i].Output;

            double[] controlInputs = Agent.FNN.ProcessInputs(sensorOutput);
            Movement.SetInputs(controlInputs);
        }

        if (timeSinceLastCheckpoint > MAX_CHECKPOINT_DELAY)
        {
            Die();
        }
    }

    // Makes this car die (making it unmovable and stops the Agent from calculating the controls for the car).
    // Машина не двигается, также агент машины останавливает свои вычисления
    private void Die()
    {
        this.enabled = false;
        Movement.Stop();
        Movement.enabled = false;

        foreach (Sensor s in sensors)
            s.Hide();

        Agent.Kill();
    }

    public void CheckpointCaptured()
    {
        timeSinceLastCheckpoint = 0;
    }
    #endregion
}
