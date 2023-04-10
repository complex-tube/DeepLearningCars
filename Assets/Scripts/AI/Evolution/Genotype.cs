#region Includes
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
#endregion

/// Класс, представляющий одного члена популяции
public class Genotype : IComparable<Genotype>, IEnumerable<float>
{
    #region Members
    private static Random randomizer = new Random();

    /// Текущая оценка этого генотипа
    public float Evaluation
    {
        get;
        set;
    }
    
    /// Текущая пригодность этого генотипа
    public float Fitness
    {
        get;
        set;
    }

    // Массив параметров этого генотипа.
    private float[] parameters;

    /// Количество параметров, хранящихся в массиве параметров данного генотипа
    public int ParameterCount
    {
        get
        {
            if (parameters == null) return 0;
            return parameters.Length;
        }
    }

    // Перегруженный индексатор для удобного доступа к параметрам
    public float this[int index]
    {
        get { return parameters[index]; }
        set { parameters[index] = value; }
    }
    #endregion

    #region Constructors
    /// Экземпляр нового генотипа с заданным вектором параметров и начальной приспособленностью 0
    public Genotype(float[] parameters)
    {
        this.parameters = parameters;
        Fitness = 0;
    }
    #endregion

    #region Methods
    #region IComparable
    /// Сравнение этого генотипа с другим генотипом в зависимости от значений их приспособленности
    public int CompareTo(Genotype other)
    {
        return other.Fitness.CompareTo(this.Fitness); //в обратном порядке, чтобы большая пригодность была первой в списке
    }
    #endregion

    #region IEnumerable
    /// Получает Enumerator для перебора всех параметров этого генотипа по числам параметра
    public IEnumerator<float> GetEnumerator()
    {
        for (int i = 0; i < parameters.Length; i++)
            yield return parameters[i];
    }

    /// Получает Enumerator для перебора всех параметров этого генотипа по генотипу
    IEnumerator IEnumerable.GetEnumerator()
    {
        for (int i = 0; i < parameters.Length; i++)
            yield return parameters[i];
    }
    #endregion

    /// Устанавливает параметры этого генотипа в случайные значения в заданном диапазоне
    public void SetRandomParameters(float minValue, float maxValue)
    {
        //Проверка аргументов
        if (minValue > maxValue) throw new ArgumentException("Minimum value may not exceed maximum value.");

        //Сгенерируйте вектор случайных параметров
        float range = maxValue - minValue;
        for (int i = 0; i < parameters.Length; i++)
            parameters[i] = (float)((randomizer.NextDouble() * range) + minValue); //Create a random float between minValue and maxValue
    }

    /// Возвращает копию вектора параметров
    public float[] GetParameterCopy()
    {
        float[] copy = new float[ParameterCount];
        for (int i = 0; i < ParameterCount; i++)
            copy[i] = parameters[i];

        return copy;
    }

    /// Сохраняет параметры этого генотипа в файл по заданному пути к файлу
    public void SaveToFile(string filePath)
    {
        StringBuilder builder = new StringBuilder();
        foreach (float param in parameters)
            builder.Append(param.ToString()).Append(";");

        builder.Remove(builder.Length - 1, 1);

        File.WriteAllText(filePath, builder.ToString());
    }
    #endregion
}
