#region Includes
using UnityEngine;
using System.Collections;
#endregion

/// Класс представляет чекпойнты для трассы
public class Checkpoint : MonoBehaviour
{
    #region Members
    /// Радиус чекпойнта
    public float CaptureRadius = 3;
    private SpriteRenderer spriteRenderer;

    /// Награда, которая выдается за достижение чекпойнта
    public float RewardValue
    {
        get;
        set;
    }

    /// Расстояние до предыдущего чекпойнта на трассе
    public float DistanceToPrevious
    {
        get;
        set;
    }

    /// Рассчитанное расстояние от нынешнего чекпойнта, до самого первого
    public float AccumulatedDistance
    {
        get;
        set;
    }

    /// Рассчитанная награда за прохождение всех чекпойнтов до этого
    public float AccumulatedReward
    {
        get;
        set;
    }

    /// Отображение чекпойнта
    public bool IsVisible
    {
        get { return spriteRenderer.enabled; }
        set { spriteRenderer.enabled = value; }
    }
    #endregion

    #region Constructors
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    #endregion

    #region Methods
    /// Расчет награды за прохождение чекпойнта
    public float GetRewardValue(float currentDistance)
    {
        //Расчет того на сколько близко до прохождение чекпойнта по отношению к расстоянию до предыдущего чекпойнта
        float completePerc = (DistanceToPrevious - currentDistance) / DistanceToPrevious; 

        //Награда за прохождение чекпойнта
        if (completePerc < 0)
            return 0;
        else return completePerc * RewardValue;
    }
    #endregion
}
