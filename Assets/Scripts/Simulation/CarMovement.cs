#region Includes
using UnityEngine;
using System.Collections;
#endregion


public class CarMovement : MonoBehaviour
{
    #region Members
    
    public event System.Action HitWall;

    
    private const float MAX_VEL = 20f;
    private const float ACCELERATION = 8f;
    private const float VEL_FRICT = 2f;
    private const float TURN_SPEED = 100;

    private CarController controller;

    
    public float Velocity
    {
        get;
        private set;
    }

    
    public Quaternion Rotation
    {
        get;
        private set;
    }

    private double horizontalInput, verticalInput; 
    
    public double[] CurrentInputs
    {
        get { return new double[] { horizontalInput, verticalInput }; }
    }
    #endregion

    #region Constructors
    void Start()
    {
        controller = GetComponent<CarController>();
    }
    #endregion

    #region Methods
	void FixedUpdate ()
    {
        if (controller != null && controller.UseUserInput)
            CheckInput();

        ApplyInput();

        ApplyVelocity();

        ApplyFriction();
	}

    private void CheckInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
    }

    private void ApplyInput()
    {
        if (verticalInput > 1)
            verticalInput = 1;
        else if (verticalInput < -1)
            verticalInput = -1;

        if (horizontalInput > 1)
            horizontalInput = 1;
        else if (horizontalInput < -1)
            horizontalInput = -1;

        bool canAccelerate = false;
        if (verticalInput < 0)
            canAccelerate = Velocity > verticalInput * MAX_VEL;
        else if (verticalInput > 0)
            canAccelerate = Velocity < verticalInput * MAX_VEL;
        
        if (canAccelerate)
        {
            Velocity += (float)verticalInput * ACCELERATION * Time.deltaTime;

            //Cap velocity
            if (Velocity > MAX_VEL)
                Velocity = MAX_VEL;
            else if (Velocity < -MAX_VEL)
                Velocity = -MAX_VEL;
        }
        
        Rotation = transform.rotation;
        Rotation *= Quaternion.AngleAxis((float)-horizontalInput * TURN_SPEED * Time.deltaTime, new Vector3(0, 0, 1));
    }

    public void SetInputs(double[] input)
    {
        horizontalInput = input[0];
        verticalInput = input[1];
    }

    private void ApplyVelocity()
    {
        Vector3 direction = new Vector3(0, 1, 0);
        transform.rotation = Rotation;
        direction = Rotation * direction;

        this.transform.position += direction * Velocity * Time.deltaTime;
    }

    private void ApplyFriction()
    {
        if (verticalInput == 0)
        {
            if (Velocity > 0)
            {
                Velocity -= VEL_FRICT * Time.deltaTime;
                if (Velocity < 0)
                    Velocity = 0;
            }
            else if (Velocity < 0)
            {
                Velocity += VEL_FRICT * Time.deltaTime;
                if (Velocity > 0)
                    Velocity = 0;            
            }
        }
    }

    void OnCollisionEnter2D()
    {
        if (HitWall != null)
            HitWall();
    }

    public void Stop()
    {
        Velocity = 0;
        Rotation = Quaternion.AngleAxis(0, new Vector3(0, 0, 1));
    }
    #endregion
}
