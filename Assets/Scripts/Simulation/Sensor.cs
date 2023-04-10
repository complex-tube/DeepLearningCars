#region Includes
using UnityEngine;
#endregion

/// Класс представляет сенсор, который считывает расстояние до первого препятствия
public class Sensor : MonoBehaviour
{
    #region Members
    
    // Слой в Unity на который будет влиять сенсор
    [SerializeField]
    private LayerMask LayerToSense;
    
    //Крестик сенсора, куда попадает его луч
    [SerializeField]
    private SpriteRenderer Cross;
    
    //Максимальная и минимальная дистанция взаимодействия
    private const float MAX_DIST = 10f;
    private const float MIN_DIST = 0.01f;

    /// Нынешнее выходное значение сенсора выраженное в процентном соотношении от максимальной дистанции
    public float Output
    {
        get;
        private set;
    }
    #endregion

    #region Constructors
    void Start ()
    {
        Cross.gameObject.SetActive(true);
	}
    #endregion

    #region Methods
    void FixedUpdate ()
    {
        //Расчет направления сенсора
        var position = this.transform.position;
        Vector2 direction = Cross.transform.position - position;
        direction.Normalize();

        //Отправить луч в сторону, в которую направлен сенсор
        RaycastHit2D hit =  Physics2D.Raycast(position, direction, MAX_DIST, LayerToSense);

        //Проверить дистанцию
        if (hit.collider == null)
            hit.distance = MAX_DIST;
        else if (hit.distance < MIN_DIST)
            hit.distance = MIN_DIST;

        this.Output = hit.distance;
        Cross.transform.position = (Vector2) this.transform.position + direction * hit.distance; //Установаить позицию нынешнего перекрестия
	}

    /// Скрыть крестики сенсора
    public void Hide()
    {
        Cross.gameObject.SetActive(false);
    }

    /// Показать крестики сенсора
    public void Show()
    {
        Cross.gameObject.SetActive(true);
    }
    #endregion
}
