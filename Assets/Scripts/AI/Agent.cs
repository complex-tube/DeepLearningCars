#region Includes
using System;
using System.Collections.Generic;
#endregion

/// Класс, сочетающий генотип и нейронную сеть с прямой связью (FNN)
public class Agent : IComparable<Agent>
{
    #region Members
    /// Генотип этого агента
    public Genotype Genotype
    {
        get;
        private set;
    }

    /// Нейронная сеть прямого распространения, построенная по генотипу этого агента
    public NeuralNetwork FNN
    {
        get;
        private set;
    }

    private bool isAlive = false;
    
    /// Жив ли этот агент в данный момент (активно участвует в симуляции)
    public bool IsAlive
    {
        get { return isAlive; }
        private set
        {
            if (isAlive != value)
            {
                isAlive = value;

                if (!isAlive && AgentDied != null)
                    AgentDied(this);
            }
        }
    }
    
    /// Событие, когда агент умер (перестал участвовать в симуляции)
    public event Action<Agent> AgentDied;
    #endregion

    #region Constructors
    
    /// Инициализирует нового агента из заданного генотипа, создавая новую нейронную сеть с прямой связью из параметров генотипа
    public Agent(Genotype genotype, NeuralLayer.ActivationFunction defaultActivation, params uint[] topology)
    {
        IsAlive = false;
        this.Genotype = genotype;
        FNN = new NeuralNetwork(topology);
        foreach (NeuralLayer layer in FNN.Layers)
            layer.NeuronActivationFunction = defaultActivation;

        //Проверить правильность топологии
        if (FNN.WeightCount != genotype.ParameterCount)
            throw new ArgumentException("The given genotype's parameter count must match the neural network topology's weight count.");

        //Construct FNN from genotype
        IEnumerator<float> parameters = genotype.GetEnumerator();
        foreach (NeuralLayer layer in FNN.Layers) //Loop over all layers
        {
            for (int i = 0; i < layer.Weights.GetLength(0); i++) //Loop over all nodes of current layer
            {
                for (int j = 0; j < layer.Weights.GetLength(1); j++) //Loop over all nodes of next layer
                {
                    layer.Weights[i,j] = parameters.Current;
                    parameters.MoveNext();
                }
            }
        }
    }
    #endregion

    #region Methods
    /// Сбрасывает этого агента в изначальное состояние
    public void Reset()
    {
        Genotype.Evaluation = 0;
        Genotype.Fitness = 0;
        IsAlive = true;
    }

    /// Убивает этого агента (устанавливает для IsAlive значение false)
    public void Kill()
    {
        IsAlive = false;
    }

    #region IComparable
    /// Сравнивает этот агент с другим агентом, сравнивая их основные генотипы
    public int CompareTo(Agent other)
    {
        return this.Genotype.CompareTo(other.Genotype);
    }
    #endregion
    #endregion
}

