#region Includes
using System;
#endregion

/// Класс представляет слой нейронной сети
public class NeuralLayer
{
    #region Members
    private static Random randomizer = new Random();

    /// Представляет активационную функцию нейрона
    public delegate double ActivationFunction(double xValue);

    /// Активационная функция, которую используют нейроны этого слоя
    public ActivationFunction NeuronActivationFunction = MathHelper.SigmoidFunction;

    /// Количество нейронов в слое
    public uint NeuronCount
    {
        get;
        private set;
    }

    /// Количество выходных нейронов следующего слоя
    public uint OutputCount
    {
        get;
        private set;
    }

    /// Веса связей между нейронам этого и следующего слоя (i-го этого слоя и j-го следующего)
    public double[,] Weights
    {
        get;
        private set;
    }
    #endregion

    #region Constructors
    /// Создание нового слоя с данным количеством нейронов и количеством нейронов следующего слоя
    public NeuralLayer(uint nodeCount, uint outputCount)
    {
        this.NeuronCount = nodeCount;
        this.OutputCount = outputCount;

        Weights = new double[nodeCount + 1, outputCount]; // + 1 для нейрона смещения
    }
    #endregion

    #region Methods

    /// Обработка входных данных используя нынешние веса до следующего слоя
    public double[] ProcessInputs(double[] inputs)
    {
        //Проверка аргументов
        if (inputs.Length != NeuronCount)
            throw new ArgumentException("Given xValues do not match layer input count.");
        
        //Инициализация переменной для суммы каждого нейрона
        double[] sums = new double[OutputCount];
        //Добавление нейрона смещения
        double[] biasedInputs = new double[NeuronCount + 1];
        inputs.CopyTo(biasedInputs, 0);
        biasedInputs[inputs.Length] = 1.0;

        for (int j = 0; j < Weights.GetLength(1); j++)
            for (int i = 0; i < Weights.GetLength(0); i++)
                sums[j] += biasedInputs[i] * Weights[i, j];

        //Применение активационной функции к сумме
        if (NeuronActivationFunction != null)
        {
            for (int i = 0; i < sums.Length; i++)
                sums[i] = NeuronActivationFunction(sums[i]);
        }

        return sums;
    }

    /// Статистика весов данного слоя
    public override string ToString()
    {
        string output = "";

        for (int x = 0; x < Weights.GetLength(0); x++)
        {
            for (int y = 0; y < Weights.GetLength(1); y++)
                output += "[" + x + "," + y + "]: " + Weights[x, y];

            output += "\n";
        }

        return output;
    }
    #endregion
}
