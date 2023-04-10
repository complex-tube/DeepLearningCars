#region Includes
using System;
using UnityEngine;
using System.Collections.Generic;
#endregion

/// Класс управляющий нынешней трассой и всеми машинами на ней
public class TrackManager : MonoBehaviour
{
    #region Members
    public static TrackManager Instance
    {
        get;
        private set;
    }

    // Спрайты для визуализации лучшей машины и машины после нее
    [SerializeField]
    private Sprite BestCarSprite;
    [SerializeField]
    private Sprite SecondBestSprite;
    [SerializeField]
    private Sprite NormalCarSprite;

    private Checkpoint[] checkpoints;

    /// Прототип машины, используется для создания новых машин
    public CarController PrototypeCar;
    // Нальная позиция
    private Vector3 startPosition;
    private Quaternion startRotation;

    // Класс для хранения нынешних машин и их позиций на трассе
    private class RaceCar
    {
        public RaceCar(CarController car = null, uint checkpointIndex = 1)
        {
            this.Car = car;
            this.CheckpointIndex = checkpointIndex;
        }
        public CarController Car;
        public uint CheckpointIndex;
    }
    private List<RaceCar> cars = new List<RaceCar>();

    /// Количество машин на трассе
    public int CarCount
    {
        get { return cars.Count; }
    }

    #region Best and Second best
    private CarController bestCar = null;
    
    /// Нынешняя лучшая машина на трассе
    public CarController BestCar
    {
        get { return bestCar; }
        private set
        {
            if (bestCar != value)
            {
                //Обновление отображения
                if (BestCar != null)
                    BestCar.SpriteRenderer.sprite = NormalCarSprite;
                if (value != null)
                    value.SpriteRenderer.sprite = BestCarSprite;

                //Установить нынешнюю самую лучшу машину на место машины перед лучшей
                CarController previousBest = bestCar;
                bestCar = value;
                if (BestCarChanged != null)
                    BestCarChanged(bestCar);

                SecondBestCar = previousBest;
            }
        }
    }
    
    /// Событие изменения лучшей машины
    public event System.Action<CarController> BestCarChanged;

    private CarController secondBestCar = null;
    
    /// Нынешняя вторая лучшая машина
    public CarController SecondBestCar
    {
        get { return secondBestCar; }
        private set
        {
            if (SecondBestCar != value)
            {
                //Отображение машины
                if (SecondBestCar != null && SecondBestCar != BestCar)
                    SecondBestCar.SpriteRenderer.sprite = NormalCarSprite;
                if (value != null)
                    value.SpriteRenderer.sprite = SecondBestSprite;

                secondBestCar = value;
                if (SecondBestCarChanged != null)
                    SecondBestCarChanged(SecondBestCar);
            }
        }
    }
    
    /// Событие изменения воторой лучшей машины
    public event System.Action<CarController> SecondBestCarChanged;
    #endregion

    

    /// Длинна трассы в расстоянии между чекпойнтами
    public float TrackLength
    {
        get;
        private set;
    }
    #endregion

    #region Constructors
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Mulitple instance of TrackManager are not allowed in one Scene.");
            return;
        }

        Instance = this;

        //Получить все чекпойнты
        checkpoints = GetComponentsInChildren<Checkpoint>();

        //Установить начальную позицию и спрятать машину-шаблон
        startPosition = PrototypeCar.transform.position;
        startRotation = PrototypeCar.transform.rotation;
        PrototypeCar.gameObject.SetActive(false);

        CalculateCheckpointPercentages();
    }

    void Start()
    {
        //Спрятать чекпойнты
        foreach (Checkpoint check in checkpoints)
            check.IsVisible = false;
    }
    #endregion

    #region Methods
    void Update()
    {
        //Обовить награду каждой доступной машины на трассе
        for (int i = 0; i < cars.Count; i++)
        {
            RaceCar car = cars[i];
            if (car.Car.enabled)
            {
                car.Car.CurrentCompletionReward = GetCompletePerc(car.Car, ref car.CheckpointIndex);

                //Обновить лучшую машину
                if (BestCar == null || car.Car.CurrentCompletionReward >= BestCar.CurrentCompletionReward)
                    BestCar = car.Car;
                else if (SecondBestCar == null || car.Car.CurrentCompletionReward >= SecondBestCar.CurrentCompletionReward)
                    SecondBestCar = car.Car;
            }
        }
    }

    public void SetCarAmount(int amount)
    {
        if (amount < 0) throw new ArgumentException("Amount may not be less than zero.");

        if (amount == CarCount) return;

        if (amount > cars.Count)
        {
            for (int toBeAdded = amount - cars.Count; toBeAdded > 0; toBeAdded--)
            {
                GameObject carCopy = Instantiate(PrototypeCar.gameObject);
                carCopy.transform.position = startPosition;
                carCopy.transform.rotation = startRotation;
                CarController controllerCopy = carCopy.GetComponent<CarController>();
                cars.Add(new RaceCar(controllerCopy, 1));
                carCopy.SetActive(true);
            }
        }
        else if (amount < cars.Count)
        {
            for (int toBeRemoved = cars.Count - amount; toBeRemoved > 0; toBeRemoved--)
            {
                RaceCar last = cars[cars.Count - 1];
                cars.RemoveAt(cars.Count - 1);

                Destroy(last.Car.gameObject);
            }
        }
    }

    /// Обонвить все машины и запустить их на трассе
    public void Restart()
    {
        foreach (RaceCar car in cars)
        {
            car.Car.transform.position = startPosition;
            car.Car.transform.rotation = startRotation;
            car.Car.Restart();
            car.CheckpointIndex = 1;
        }

        BestCar = null;
        SecondBestCar = null;
    }

    public IEnumerator<CarController> GetCarEnumerator()
    {
        for (int i = 0; i < cars.Count; i++)
            yield return cars[i].Car;
    }

    /// Вычисляет процент полного пути, на который приходится чекпойнт
    private void CalculateCheckpointPercentages()
    {
        checkpoints[0].AccumulatedDistance = 0; //Первый чекпойнт - это старт
        //Проход по оставшимся чекпойнтам и установление дистанции к предыдущим и накопленной дистанции трассы
        for (int i = 1; i < checkpoints.Length; i++)
        {
            checkpoints[i].DistanceToPrevious = Vector2.Distance(checkpoints[i].transform.position, checkpoints[i - 1].transform.position);
            checkpoints[i].AccumulatedDistance = checkpoints[i - 1].AccumulatedDistance + checkpoints[i].DistanceToPrevious;
        }

        //Установка длины пути на накопленном расстоянии от последнего чекпойнта
        TrackLength = checkpoints[checkpoints.Length - 1].AccumulatedDistance;
        
        //Рассчитать вознаграждение для каждого чекпойнта
        for (int i = 1; i < checkpoints.Length; i++)
        {
            checkpoints[i].RewardValue = (checkpoints[i].AccumulatedDistance / TrackLength) - checkpoints[i-1].AccumulatedReward;
            checkpoints[i].AccumulatedReward = checkpoints[i - 1].AccumulatedReward + checkpoints[i].RewardValue;
        }
    }

    // Вычисляет процент завершения данного автомобиля с учетом завершенной последней контрольной точки.
    private float GetCompletePerc(CarController car, ref uint curCheckpointIndex)
    {
        //Проверка прохождения всех чекпойнтов
        if (curCheckpointIndex >= checkpoints.Length)
            return 1;

        //Рассчитать расстояние до следующей контрольной точки
        float checkPointDistance = Vector2.Distance(car.transform.position, checkpoints[curCheckpointIndex].transform.position);

        //Проверка на то можно ли пройти точку
        if (checkPointDistance <= checkpoints[curCheckpointIndex].CaptureRadius)
        {
            curCheckpointIndex++;
            car.CheckpointCaptured(); //Уведомить машину, что она прошла чекпойнт
            return GetCompletePerc(car, ref curCheckpointIndex); //Проверить следующий чекпойнт
        }
        else
        {
            //Получить награду за последнюю контрольную точку, получить награду за расстояние до следующей контрольной точки
            return checkpoints[curCheckpointIndex - 1].AccumulatedReward + checkpoints[curCheckpointIndex].GetRewardValue(checkPointDistance);
        }
    }
    #endregion

}
