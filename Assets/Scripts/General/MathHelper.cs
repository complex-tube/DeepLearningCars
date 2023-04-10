#region Includes
using System;
#endregion

/// Статический класс для математических операций
public static class MathHelper
{
    #region Methods
    #region Activation Functions
    /// Стандартная сигмоидальная функция 
    public static double SigmoidFunction(double xValue)
    {
        if (xValue > 10) return 1.0;
        else if (xValue < -10) return 0.0;
        else return 1.0 / (1.0 + Math.Exp(-xValue));
    }

    /// Стандартная функция гиперболического тангенса
    public static double TanHFunction(double xValue)
    {
        if (xValue > 10) return 1.0;
        else if (xValue < -10) return -1.0;
        else return Math.Tanh(xValue);
    }

    /// SoftSign функция
    public static double SoftSignFunction(double xValue)
    {
        return xValue / (1 + Math.Abs(xValue));
    }
    #endregion
    #endregion
}

