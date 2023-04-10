#region Includes
using System;
using System.Collections.Generic;
#endregion

/// Класс, реализующий модифицированный генетический алгоритм
public class GeneticAlgorithm
{
    #region Members
    #region Default Parameters
    
    /// Стандартное минимальное значение для инициации параметров популяции
    public const float DefInitParamMin = -1.0f;
    
    /// Стандартное минимальное значение для инициации параметров популяции
    public const float DefInitParamMax = 1.0f;

    /// Стандартная вероятность перемешивания параметров
    public const float DefCrossSwapProb = 0.6f;

    /// Стандартная вероятность мутации параметров
    public const float DefMutationProb = 0.4f;
    
    /// Макимальное значение (+-) на которое может быть мутирован параметр
    public const float DefMutationAmount = 2.0f;
    
    /// Стандартный процент генотипов, которые могуть быть мутированы в новой популяции
    public const float DefMutationPerc = 1.0f;
    #endregion

    #region Operator Delegates
    
    /// Шаблон метода для методов, используемых для инициализации начальной популяции
    public delegate void InitialisationOperator(IEnumerable<Genotype> initialPopulation);
    
    /// Шаблон метода для методов, используемых для оценки (или запуска процесса оценки) текущей совокупности
    public delegate void EvaluationOperator(IEnumerable<Genotype> currentPopulation);
    
    /// Шаблон метода для методов, используемых для расчета значения пригодности каждого генотипа текущей популяции
    public delegate void FitnessCalculation(IEnumerable<Genotype> currentPopulation);
    
    /// Шаблон метода для методов, используемых для выбора генотипов текущей популяции и создания промежуточной популяции
    public delegate List<Genotype> SelectionOperator(List<Genotype> currentPopulation);
    
    /// Шаблон метода для методов, используемых для объединения промежуточной популяции для создания новой популяции
    public delegate List<Genotype> RecombinationOperator(List<Genotype> intermediatePopulation, uint newPopulationSize);
    
    /// Шаблон метода для методов, используемых для мутации новой популяции
    public delegate void MutationOperator(List<Genotype> newPopulation);
    
    /// Шаблон метода для метода, используемого для проверки выполнения какого-либо критерия завершения
    public delegate bool CheckTerminationCriterion(IEnumerable<Genotype> currentPopulation);
    #endregion

    #region Operator Methods
    
    /// Метод, используемый для инициализации начальной популяции
    public InitialisationOperator InitialisePopulation = DefaultPopulationInitialisation;
    
    /// Метод, используемый для оценки (или запуска процесса оценки) текущей популяции
    public EvaluationOperator Evaluation = AsyncEvaluation;
    
    /// Метод, используемый для расчета значения приспособленности каждого генотипа текущей популяции
    public FitnessCalculation FitnessCalculationMethod = DefaultFitnessCalculation;
    
    /// Метод, используемый для выбора генотипов текущей популяции и создания промежуточной популяции
    public SelectionOperator Selection = DefaultSelectionOperator;
    
    /// Метод, используемый для рекомбинации промежуточной популяции для создания новой популяции
    public RecombinationOperator Recombination = DefaultRecombinationOperator;
    
    /// Метод, используемый для мутации новой популяции
    public MutationOperator Mutation = DefaultMutationOperator;
    
    /// Метод, используемый для проверки выполнения какого-либо критерия завершения
    public CheckTerminationCriterion TerminationCriterion = null;
    #endregion

    private static Random randomizer = new Random();

    private List<Genotype> currentPopulation;

    /// Количество генотипов в популяции
    public uint PopulationSize
    {
        get;
        private set;
    }

    /// Количество поколений, которые уже прошли
    public uint GenerationCount
    {
        get;
        private set;
    }

    /// Должна ли текущая популяция сортироваться перед вызовом оператора критерия завершения
    public bool SortPopulation
    {
        get;
        private set;
    }

    /// Работает ли в настоящее время генетический алгоритм
    public bool Running
    {
        get;
        private set;
    }

    /// Событие, когда алгоритм в конечном итоге завершается
    public event System.Action<GeneticAlgorithm> AlgorithmTerminated;
    
    /// Событие, когда алгоритм завершил расчет пригодности.
    public event System.Action<IEnumerable<Genotype>> FitnessCalculationFinished;

    #endregion

    #region Constructors
    /// Инициализировать новый экземпляр генетического алгоритма
    public GeneticAlgorithm(uint genotypeParamCount, uint populationSize)
    {
        this.PopulationSize = populationSize;
        //Инициализировать новую популяцию
        currentPopulation = new List<Genotype>((int) populationSize);
        for (int i = 0; i < populationSize; i++)
            currentPopulation.Add(new Genotype(new float[genotypeParamCount]));

        GenerationCount = 1;
        SortPopulation = true;
        Running = false;
    }
    #endregion

    #region Methods
    public void Start()
    {
        Running = true;

        InitialisePopulation(currentPopulation);

        Evaluation(currentPopulation);
    }

    public void EvaluationFinished()
    {
        //Рассчитать пригодность на основе оценки
        FitnessCalculationMethod(currentPopulation);

        //Сортировать популяцию, если установлен флаг
        if (SortPopulation)
            currentPopulation.Sort();

        //Событие, когда пригодность рассчитана
        if (FitnessCalculationFinished != null)
            FitnessCalculationFinished(currentPopulation);

        //Проверить критерий завершения
        if (TerminationCriterion != null && TerminationCriterion(currentPopulation))
        {
            Terminate();
            return;
        }

        //Применить выбор
        List<Genotype> intermediatePopulation = Selection(currentPopulation);

        //Применить рекомбинацию
        List<Genotype> newPopulation = Recombination(intermediatePopulation, PopulationSize);

        //Применить мутацию
        Mutation(newPopulation);

        
        //Установка текущей популяции на вновь сгенерированную и запуск оценки заново
        currentPopulation = newPopulation;
        GenerationCount++;

        Evaluation(currentPopulation);
    }

    private void Terminate()
    {
        Running = false;
        if (AlgorithmTerminated != null)
            AlgorithmTerminated(this);
    }

    #region Static Methods
    #region Default Operators
    
    /// Инициализия популяции, устанавливая для каждого параметра случайное значение в диапазоне по умолчанию.
    public static void DefaultPopulationInitialisation(IEnumerable<Genotype> population)
    {
        //Set parameters to random values in set range
        foreach (Genotype genotype in population)
            genotype.SetRandomParameters(DefInitParamMin, DefInitParamMax);
    }

    public static void AsyncEvaluation(IEnumerable<Genotype> currentPopulation)
    {
        //В этот момент должна быть запущена асинхронная оценка, и после ее завершения должна быть вызвана функция EvaluationFinished
    }

    
    /// Рассчитывает приспособленность каждого генотипа по формуле: fitness = evaluation / averageEvaluation
    public static void DefaultFitnessCalculation(IEnumerable<Genotype> currentPopulation)
    {
        //Сначала считается средняя оценка всего населения
        uint populationSize = 0;
        float overallEvaluation = 0;
        foreach (Genotype genotype in currentPopulation)
        {
            overallEvaluation += genotype.Evaluation;
            populationSize++;
        }

        float averageEvaluation = overallEvaluation / populationSize;

        //Теперь приспособленность считается формулой: fitness = evaluation / averageEvaluation
        foreach (Genotype genotype in currentPopulation)
            genotype.Fitness = genotype.Evaluation / averageEvaluation;
    }

    /// Выбор только трех лучших генотипа текущей популяции и копирует их в промежуточную популяцию
    public static List<Genotype> DefaultSelectionOperator(List<Genotype> currentPopulation)
    {
        List<Genotype> intermediatePopulation = new List<Genotype>();
        intermediatePopulation.Add(currentPopulation[0]);
        intermediatePopulation.Add(currentPopulation[1]);
        intermediatePopulation.Add(currentPopulation[2]);

        return intermediatePopulation;
    }

    /// Скрещивает первый со вторым генотипом промежуточной популяции до тех пор, пока новая популяция не достигнет желаемого размера
    public static List<Genotype> DefaultRecombinationOperator(List<Genotype> intermediatePopulation, uint newPopulationSize)
    {
        if (intermediatePopulation.Count < 2) throw new ArgumentException("Intermediate population size must be greater than 2 for this operator.");

        List<Genotype> newPopulation = new List<Genotype>();
        while (newPopulation.Count < newPopulationSize)
        {
            Genotype offspring1, offspring2;
            CompleteCrossover(intermediatePopulation[0], intermediatePopulation[1], DefCrossSwapProb, out offspring1, out offspring2);

            newPopulation.Add(offspring1);
            if (newPopulation.Count < newPopulationSize)
                newPopulation.Add(offspring2);
        }

        return newPopulation;
    }

    /// Мутирует каждый генотип с вероятностью и количеством мутаций по умолчанию
    public static void DefaultMutationOperator(List<Genotype> newPopulation)
    {
        foreach (Genotype genotype in newPopulation)
        {
            if (randomizer.NextDouble() < DefMutationPerc)
                MutateGenotype(genotype, DefMutationProb, DefMutationAmount);
        }
    }
    #endregion

    #region Recombination Operators
    public static void CompleteCrossover(Genotype parent1, Genotype parent2, float swapChance, out Genotype offspring1, out Genotype offspring2)
    {
        //Initialise new parameter vectors
        int parameterCount = parent1.ParameterCount;
        float[] off1Parameters = new float[parameterCount], off2Parameters = new float[parameterCount];

        //Iterate over all parameters randomly swapping
        for (int i = 0; i < parameterCount; i++)
        {
            if (randomizer.Next() < swapChance)
            {
                //Swap parameters
                off1Parameters[i] = parent2[i];
                off2Parameters[i] = parent1[i];
            }
            else
            {
                //Don't swap parameters
                off1Parameters[i] = parent1[i];
                off2Parameters[i] = parent2[i];
            }
        }

        offspring1 = new Genotype(off1Parameters);
        offspring2 = new Genotype(off2Parameters);
    }
    #endregion

    #region Mutation Operators
    
    /// Мутирует данный генотип, добавляя случайное значение в диапазоне [-mutationAmount, mutationAmount] к каждому параметру с вероятностью мутацииProb.
    public static void MutateGenotype(Genotype genotype, float mutationProb, float mutationAmount)
    {
        for (int i = 0; i < genotype.ParameterCount; i++)
        {
            if (randomizer.NextDouble() < mutationProb)
            {
                genotype[i] += (float)(randomizer.NextDouble() * (mutationAmount * 2) - mutationAmount);
            }    
        } 
    }
    #endregion
    #endregion
    #endregion

}
