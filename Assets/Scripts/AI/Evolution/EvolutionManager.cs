#region Includes
using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
#endregion

/// Класс для взаимодействия с эволюционными процессами.
public class EvolutionManager : MonoBehaviour
{
    #region Members
    private static System.Random randomizer = new System.Random();

    public static EvolutionManager Instance
    {
        get;
        private set;
    }

    // Результаты будут записаны в файл
    [SerializeField]
    private bool SaveStatistics = false;
    private string statisticsFileName;

    // Количество генотипов прошедших трассу, чтобы сохранить их
    [SerializeField]
    private uint SaveFirstNGenotype = 0;
    private uint genotypesSaved = 0;

    // Численность популяции
    [SerializeField]
    private int PopulationSize = 30;

    // После какой популяции перезапускается алгоритм (0 = никогда)
    [SerializeField]
    private int RestartAfter = 100;

    // Использование элитарного отбора (true) или остаточной стохастической выборки (false)
    [SerializeField]
    private bool ElitistSelection = false;

    [SerializeField] private bool isAllMutatable = false;

    // Слои FNN (Нейронной сети с прямой связью)
    [SerializeField]
    private uint[] FNNTopology;

    // Нынешняя популяция агентов
    private List<Agent> agents = new List<Agent>();
    
    /// Количество агентов, которые живы в данный момент
    public int AgentsAliveCount
    {
        get;
        private set;
    }

    /// Событие, когда все агенты мертвы
    public event System.Action AllAgentsDied;

    private GeneticAlgorithm geneticAlgorithm;

    /// Возраст нынешнего поколения
    public uint GenerationCount
    {
        get { return geneticAlgorithm.GenerationCount; }
    }
    #endregion

    #region Constructors
    void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one EvolutionManager in the Scene.");
            return;
        }
        Instance = this;
    }
    #endregion

    #region Methods
    // Запуск эволюционного процесса
    public void StartEvolution()
    {
        //Создание нейронной сети
        NeuralNetwork nn = new NeuralNetwork(FNNTopology);

        //Установка генетического алгоритма
        geneticAlgorithm = new GeneticAlgorithm((uint) nn.WeightCount, (uint) PopulationSize);
        genotypesSaved = 0;

        geneticAlgorithm.Evaluation = StartEvaluation;

        if (ElitistSelection)
        {
            //Конфигурация, когда отбор элитарный
            geneticAlgorithm.Selection = GeneticAlgorithm.DefaultSelectionOperator;
            geneticAlgorithm.Recombination = RandomRecombination;
        }
        else
        {   
            //Конфигурация, когда отбор стохастический
            geneticAlgorithm.Selection = RemainderStochasticSampling;
            geneticAlgorithm.Recombination = RandomRecombination;
        }

        if (isAllMutatable)
        {
            geneticAlgorithm.Mutation = MutateAll;
        }
        else
        {
            geneticAlgorithm.Mutation = MutateAllButBestTwo;
        }
        
        AllAgentsDied += geneticAlgorithm.EvaluationFinished;

        //Statistics
        if (SaveStatistics)
        {
            statisticsFileName = "Evaluation - " + GameStateManager.Instance.TrackName + " " + DateTime.Now.ToString("yyyy_MM_dd_HH-mm-ss");
            WriteStatisticsFileStart();
            geneticAlgorithm.FitnessCalculationFinished += WriteStatisticsToFile;
        }
        geneticAlgorithm.FitnessCalculationFinished += CheckForTrackFinished;
        
        //Перезапуск логики
        if (RestartAfter > 0)
        {
            geneticAlgorithm.TerminationCriterion += CheckGenerationTermination;
            geneticAlgorithm.AlgorithmTerminated += OnGATermination;
        }

        geneticAlgorithm.Start();
    }

    // Запись параметров статистики в файл
    private void WriteStatisticsFileStart()
    {
        File.WriteAllText(statisticsFileName + ".txt", "Evaluation of a Population with size " + PopulationSize + 
                ", on Track \"" + GameStateManager.Instance.TrackName + "\", using the following GA operators: " + Environment.NewLine +
                "Selection: " + geneticAlgorithm.Selection.Method.Name + Environment.NewLine +
                "Recombination: " + geneticAlgorithm.Recombination.Method.Name + Environment.NewLine +
                "Mutation: " + geneticAlgorithm.Mutation.Method.Name + Environment.NewLine + 
                "FitnessCalculation: " + geneticAlgorithm.FitnessCalculationMethod.Method.Name + Environment.NewLine + Environment.NewLine);
    }

    // Добавление в файл информации про нынешнюю популяцию и лучший результат генотипа
    private void WriteStatisticsToFile(IEnumerable<Genotype> currentPopulation)
    {
        foreach (Genotype genotype in currentPopulation)
        {
            File.AppendAllText(statisticsFileName + ".txt", geneticAlgorithm.GenerationCount + "\t" + genotype.Evaluation + Environment.NewLine);
            break; //Запись только первого
        }
    }

    // Проверка нынешней популяции и запись статистики генотипа в файл, если его результат достиг 1
    private void CheckForTrackFinished(IEnumerable<Genotype> currentPopulation)
    {
        if (genotypesSaved >= SaveFirstNGenotype) return;

        string saveFolder = statisticsFileName + "/";

        foreach (Genotype genotype in currentPopulation)
        {
            if (genotype.Evaluation >= 1)
            {
                if (!Directory.Exists(saveFolder))
                    Directory.CreateDirectory(saveFolder);

                genotype.SaveToFile(saveFolder + "Genotype - Finished as " + (++genotypesSaved) + ".txt");

                if (genotypesSaved >= SaveFirstNGenotype) return;
            }
            else
                return;
        }
    }

    // Проверка на критерий завершения эволюции, по достижению максимального номера популяции
    private bool CheckGenerationTermination(IEnumerable<Genotype> currentPopulation)
    {
        return geneticAlgorithm.GenerationCount >= RestartAfter;
    }

    // Вызов, когда генетический алгоритм был завершен
    private void OnGATermination(GeneticAlgorithm ga)
    {
        AllAgentsDied -= ga.EvaluationFinished;

        RestartAlgorithm(5.0f);
    }

    // Перезапуск, когда закончилось время ожидания
    private void RestartAlgorithm(float wait)
    {
        Invoke("StartEvolution", wait);
    }

    // Запуск оценки, создание новых агентов, перезапуск менеджера трассы
    private void StartEvaluation(IEnumerable<Genotype> currentPopulation)
    {
        //Создание новых агентов для нынешней популяции
        agents.Clear();
        AgentsAliveCount = 0;

        foreach (Genotype genotype in currentPopulation)
            agents.Add(new Agent(genotype, MathHelper.SoftSignFunction, FNNTopology));

        TrackManager.Instance.SetCarAmount(agents.Count);
        IEnumerator<CarController> carsEnum = TrackManager.Instance.GetCarEnumerator();
        for (int i = 0; i < agents.Count; i++)
        {
            if (!carsEnum.MoveNext())
            {
                Debug.LogError("Cars enum ended before agents.");
                break;
            }

            carsEnum.Current.Agent = agents[i];
            AgentsAliveCount++;
            agents[i].AgentDied += OnAgentDied;
        }

        TrackManager.Instance.Restart();
    }

    // Callback проверки на умерших агентов
    private void OnAgentDied(Agent agent)
    {
        AgentsAliveCount--;

        if (AgentsAliveCount == 0 && AllAgentsDied != null)
            AllAgentsDied();
    }

    #region GA Operators
    // Выбор оператора для генетического алгоритма, используя метод остаточной стохастической выборки
    private List<Genotype> RemainderStochasticSampling(List<Genotype> currentPopulation)
    {
        List<Genotype> intermediatePopulation = new List<Genotype>();
        // Добавление в промежуточную популяцию машин, столько раз, сколько у их приспособленности единиц
        foreach (Genotype genotype in currentPopulation)
        {
            if (genotype.Fitness < 1)
                break;
            else
            {
                for (int i = 0; i < (int) genotype.Fitness; i++)
                    intermediatePopulation.Add(new Genotype(genotype.GetParameterCopy()));
            }
        }

        //Добавление оставшейся части генотипов в промежуточную популяцию
        foreach (Genotype genotype in currentPopulation)
        {
            float remainder = genotype.Fitness - (int)genotype.Fitness;
            if (randomizer.NextDouble() < remainder)
                intermediatePopulation.Add(new Genotype(genotype.GetParameterCopy()));
        }

        return intermediatePopulation;
    }

    // Рекомбинация операторов для генетического алгоритма, рекомбинация случайных генотипов для промежуточной популяции
    private List<Genotype> RandomRecombination(List<Genotype> intermediatePopulation, uint newPopulationSize)
    {
        //Проверка аргументов
        if (intermediatePopulation.Count < 2)
            throw new System.ArgumentException("The intermediate population has to be at least of size 2 for this operator.");

        List<Genotype> newPopulation = new List<Genotype>();
        //Всегда добавляем лучшие два
        newPopulation.Add(intermediatePopulation[0]);
        newPopulation.Add(intermediatePopulation[1]);


        while (newPopulation.Count < newPopulationSize)
        {
            //Берем два случайных индекса, которые не одинаковые
            int randomIndex1 = randomizer.Next(0, intermediatePopulation.Count), randomIndex2;
            do
            {
                randomIndex2 = randomizer.Next(0, intermediatePopulation.Count);
            } while (randomIndex2 == randomIndex1);

            Genotype offspring1, offspring2;
            GeneticAlgorithm.CompleteCrossover(intermediatePopulation[randomIndex1], intermediatePopulation[randomIndex2], 
                GeneticAlgorithm.DefCrossSwapProb, out offspring1, out offspring2);

            newPopulation.Add(offspring1);
            if (newPopulation.Count < newPopulationSize)
                newPopulation.Add(offspring2);
        }

        return newPopulation;
    }
    
    // Мутирование всех членов новой популяции с стандартной вероянтостью, кроме крайних двух членов
    private void MutateAllButBestTwo(List<Genotype> newPopulation)
    {
        for (int i = 2; i < newPopulation.Count; i++)
        {
            if (randomizer.NextDouble() < GeneticAlgorithm.DefMutationPerc)
                GeneticAlgorithm.MutateGenotype(newPopulation[i], GeneticAlgorithm.DefMutationProb, GeneticAlgorithm.DefMutationAmount);
        }
    }
    
    // Мутирование всех членов популции
    private void MutateAll(List<Genotype> newPopulation)
    {
        foreach (Genotype genotype in newPopulation)
        {
            if (randomizer.NextDouble() < GeneticAlgorithm.DefMutationPerc)
                GeneticAlgorithm.MutateGenotype(genotype, GeneticAlgorithm.DefMutationProb, GeneticAlgorithm.DefMutationAmount);
        }
    }
    #endregion
    #endregion

    }
