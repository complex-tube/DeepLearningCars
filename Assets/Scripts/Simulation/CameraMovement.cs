#region Includes
using UnityEngine;
#endregion


/// Класс для движения камеры
public class CameraMovement : MonoBehaviour
{
    #region Members
    // Расстояние от камеры до объекта в z координате
    [SerializeField]
    private int CamZ = -10;
    // Цель наблюдения камеры
    [SerializeField]
    private GameObject Target;
    // Скорость движения камеры
    [SerializeField]
    private float CamSpeed = 5f;
    // Скорость камеры во время пользовательского управления
    [SerializeField]
    private float UserInputSpeed = 50f;
    // Возможность управлять камерой пользователем
    [SerializeField]
    private bool AllowUserInput;

    /// Рамки в которых камера может двигаться
    public RectTransform MovementBounds
    {
        get;
        set;
    }

    private Vector3 targetCamPos;
    #endregion

    #region Methods
    /// Установка цели для слежки
    public void SetTarget(GameObject target)
    {
        // Установка позиции мгновенно, если предыдущая цель null
        if (Target == null && !AllowUserInput && target != null)
            SetCamPosInstant(target.transform.position);

        this.Target = target;
    }
    
	void FixedUpdate ()
    {
        //Проверка направления движения
        if (AllowUserInput)
            CheckUserInput();
        else if (Target != null)
            targetCamPos = Target.transform.position;

        targetCamPos.z = CamZ;
        this.transform.position = Vector3.Lerp(this.transform.position, targetCamPos, CamSpeed * Time.deltaTime); //Движение камеры со сглаживаением

        //Проверка на выход из рамок
        if (MovementBounds != null)
        {
            float vertExtent = Camera.main.orthographicSize;
            float horzExtent = vertExtent * Screen.width / Screen.height;

            float rightDiff = (this.transform.position.x + horzExtent) - (MovementBounds.position.x + MovementBounds.rect.width / 2);
            float leftDiff = (this.transform.position.x - horzExtent) - (MovementBounds.position.x - MovementBounds.rect.width / 2);
            float upDiff = (this.transform.position.y + vertExtent) - (MovementBounds.position.y + MovementBounds.rect.height / 2);
            float downDiff = (this.transform.position.y - vertExtent) - (MovementBounds.position.y - MovementBounds.rect.height / 2);

            if (rightDiff > 0)
            {
                this.transform.position = new Vector3(this.transform.position.x - rightDiff, this.transform.position.y, this.transform.position.z);
                targetCamPos.x = this.transform.position.x;
            }
            else if (leftDiff < 0)
            {
                this.transform.position = new Vector3(this.transform.position.x - leftDiff, this.transform.position.y, this.transform.position.z);
                targetCamPos.x = this.transform.position.x;
            }

            if (upDiff > 0)
            {
                this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - upDiff, this.transform.position.z);
                targetCamPos.y = this.transform.position.y;
            }
            else if (downDiff < 0)
            {
                this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - downDiff, this.transform.position.z);
                targetCamPos.y = this.transform.position.y;
            }
        }
    }

    /// Моментальная постановка камеры в нужную позицию без сглаживания
    public void SetCamPosInstant(Vector3 camPos)
    {
        camPos.z = CamZ;
        this.transform.position = camPos;
        targetCamPos = this.transform.position;
    }

    private void CheckUserInput()
    {
        float horizontalInput, verticalInput;

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        targetCamPos += new Vector3(horizontalInput * UserInputSpeed * Time.deltaTime, verticalInput * UserInputSpeed * Time.deltaTime);
    }
    #endregion
}
