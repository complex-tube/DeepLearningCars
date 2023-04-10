#region Includes
using System;
#endregion

/// Класс представляет полностью связанную нейронную сеть прямого распределения
public class NeuralNetwork
{
    #region Members
    /// Слои нейронной сети
    public NeuralLayer[] Layers
    {
        get;
        private set;
    }

    /// Массив представляющий количество нейронов в каждом слое
    public uint[] Topology
    {
        get;
        private set;
    }

    /// Количество весов во всех связях между нейронами
    public int WeightCount
    {
        get;
        private set;
    }
    #endregion

    #region Constructors
    /// Инициализация новой нейронной сети
    public NeuralNetwork(params uint[] topology)
    {
        this.Topology = topology;

        //Вычисление количества всех весов
        WeightCount = 0;
        for (int i = 0; i < topology.Length - 1; i++)
            WeightCount += (int) ((topology[i] + 1) * topology[i + 1]); // + 1 для нейрона смещения

        //Инициализация слоев
        Layers = new NeuralLayer[topology.Length - 1];
        for (int i = 0; i<Layers.Length; i++)
            Layers[i] = new NeuralLayer(topology[i], topology[i + 1]);
    }
    #endregion

    #region Methods
    /// Обработка входных данных
    public double[] ProcessInputs(double[] inputs)
    {
        //Проверка входных данных
        if (inputs.Length != Layers[0].NeuronCount)
            throw new ArgumentException("Given inputs do not match network input amount.");

        //Process inputs by propagating values through all layers
        //Обработка входных данных путем распространения через всю сеть
        double[] outputs = inputs;
        foreach (NeuralLayer layer in Layers)
            outputs = layer.ProcessInputs(outputs);

        return outputs;
        
    }

    /// Возвращает строку представляющую нейронную сеть
    public override string ToString()
    {
        string output = "";

        for (int i = 0; i<Layers.Length; i++)
            output += "Layer " + i + ":\n" + Layers[i].ToString();

        return output;
    }
    #endregion
}
