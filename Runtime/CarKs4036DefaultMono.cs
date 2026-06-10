using System;
using UnityEngine;
using UnityEngine.Events;


public class CarKs4036DefaultMono : MonoBehaviour {


    [SerializeField] CharacterController m_characterToMove;
    [SerializeField] CarEvent m_events = new CarEvent();
    [Serializable]
    class CarEvent { 
		public UnityEvent<float> m_onLinearVelocityUpdated;
		public UnityEvent<float> m_onAngularVelocityUpdated;
		public UnityEvent<float> m_onLeftWheelPercentPowerUpdated;
		public UnityEvent<float> m_onRightWheelPercentPowerUpdated;
		public UnityEvent<float> m_onLeftWheelDegreePerSecondUpdated;
		public UnityEvent<float> m_onRightWheelDegreePerSecondUpdated;
		public UnityEvent<float> m_onLeftWheelCurrentRotationUpdated;
		public UnityEvent<float> m_onRightWheelCurrentRotationUpdated;
		public UnityEvent<bool[]> m_onScreenDisplay128x64SetRequest;
		public UnityEvent<Color> m_onColorRequestForTheCarStyle;
		public UnityEvent<Vector3> m_onPositionUpdated;
		public UnityEvent<Quaternion> m_onRotationInQuaternion;
		public UnityEvent<Vector3> m_onRotationInEuler;
        public UnityEvent<int> m_onCarIdUpdated;
        public UnityEvent m_onFireRequest;
    }

    public void Fire() {
		m_events.m_onFireRequest.Invoke();
	}
	public void SetCarColor(Color color) {
		var colorNoAlpha = new Color(color.r, color.g, color.b, 1);
		m_events.m_onColorRequestForTheCarStyle.Invoke(colorNoAlpha);
    }




    [Range(-1.0f, 1.0f)] [SerializeField] float m_leftWheelPercentPower = 0.0f;
    [Range(-1.0f, 1.0f)] [SerializeField] float m_rightWheelPercentPower = 0.0f;
    [SerializeField] bool m_useFakeGravity = true;
    [SerializeField] float m_fakeLinearGravity = 0.2f;
    [SerializeField] float m_rotationPerSecondInDegree = 720.0f;
    [SerializeField] Transform m_carCenterReferenceNode;
    [SerializeField] Transform m_leftWheelReferenceNode;
    [SerializeField] Transform m_rightWheelReferenceNode;
    [SerializeField] Transform m_rightWheelTopRadiusReferenceNode;

    [SerializeField] Ks4036Raycast3D m_raycastFrontLeftWheel;
    [SerializeField] Ks4036Raycast3D m_raycastFrontRightWheel;
    [SerializeField] Transform m_carCenterGroundReferenceNode;
    [SerializeField] bool m_useRandomColorStyleAtReady = true;

    [SerializeField] float m_distanceBetweenWheelsInMm = 70.0f;
    [SerializeField] float m_radiusOfWheelsInMm = 16.6f;
    [SerializeField] float m_diameterOfWheelsInMm = 33.2f;
    [SerializeField] float m_circumferenceOfWheelsInMm;
    [SerializeField] float m_maxWheelSpeedInMeterPerSec;
  
    [SerializeField] float m_leftRotationInDegreeTotal;
    [SerializeField] float m_rightRotationInDegreeTotal;
    [SerializeField] int m_carId;
    [SerializeField] Vector3 m_carPosition;
    [SerializeField] Quaternion m_carRotation;
    [SerializeField] Vector3 m_carEuler;

    [SerializeField] float m_distanceBetweenWheelsInMeter = 0.07f;


    void Start() 
{ 
    if (m_characterToMove == null)
    {
        Debug.LogError("m_characterToMove is not assigned!", this);
        return;
    }
    
    if (m_useRandomColorStyleAtReady)
    {
        SetCarColor(new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f)
        ));
    }
    
    RefreshWheelParameters();
}


    //## I am method that allows to update the screen of the mini car with a new texture, 
    //## where the texture is a 1D array of boolean values representing the pixels of the screen,
    //## Only works is designer did put a screen on the car.
    public void SetScreen128x64To(bool[] array1d128x64)
    {
        m_events.m_onScreenDisplay128x64SetRequest.Invoke(array1d128x64);
    }

    //## Allows to control the motor speed of the left wheel from -1.0 to 1.0, where 1.0 is full forward, -1.0 is full backward, and 0.0 is stopped.
    public void SetLeftWheelPercentPower(float percentPower)
    {
        m_leftWheelPercentPower = Mathf.Clamp(percentPower, -1.0f, 1.0f);
    }
    //## Allows to control the motor speed of the right wheel from -1.0 to 1.0, where 1.0 is full forward, -1.0 is full backward, and 0.0 is stopped.
    public void SetRightWheelPercentPower(float percentPower)
    {
        m_rightWheelPercentPower = Mathf.Clamp(percentPower, -1.0f, 1.0f);
    }

    //## allos to control the motor speed of both motor with a single function, where each percent power is from -1.0 to 1.0, where 1.0 is full forward, -1.0 is full backward, and 0.0 is stopped.
    public void SetBothWheelsPercentPower(float leftPercentPower, float rightPercentPower)
    {
        SetLeftWheelPercentPower(leftPercentPower);
        SetRightWheelPercentPower(rightPercentPower);
    }

    public void SetMaxWheelSpeedInMeterFromRotationAngleInDegreePerSeconds(float newRotationPerSecondInDegree)
    {
        m_rotationPerSecondInDegree = newRotationPerSecondInDegree;
        m_maxWheelSpeedInMeterPerSec = m_circumferenceOfWheelsInMm * 
                                       (m_rotationPerSecondInDegree / 360.0f) * 0.001f;
    }
    public void SetMaxWheelSpeedInMeterPerSeconds(float newMaxWheelSpeedInMeterPerSec)
    {
        m_maxWheelSpeedInMeterPerSec = newMaxWheelSpeedInMeterPerSec;
        m_rotationPerSecondInDegree = (m_maxWheelSpeedInMeterPerSec * 1000.0f * 360.0f) / m_circumferenceOfWheelsInMm;
    }







//## I am a method that allows to control the motor from four button inputs (FRONT LEFT, FRONT RIGHT, BACK LEFT, BACK RIGHT)
//## This simulates the controller of four buttons of KS4036
public void SetWithFourButtons(bool frontLeft, bool frontRight, bool backLeft, bool backRight)
{
    if (frontLeft && frontRight && backLeft && backRight)
    {
        SetBothWheelsPercentPower(0, 0);
    }
    else if (!frontLeft && frontRight && backLeft && backRight)
    {
        SetBothWheelsPercentPower(-1, 0);
    }
    else if (frontLeft && !frontRight && backLeft && backRight)
    {
        SetBothWheelsPercentPower(0, -1);
    }
    else if (!frontLeft && !frontRight && backLeft && backRight)
    {
        SetBothWheelsPercentPower(-1, -1);
    }
    else if (frontLeft && frontRight && !backLeft && backRight)
    {
        SetBothWheelsPercentPower(1, 0);
    }
    else if (!frontLeft && frontRight && !backLeft && backRight)
    {
        SetBothWheelsPercentPower(0, 0);
    }
    else if (frontLeft && !frontRight && !backLeft && backRight)
    {
        SetBothWheelsPercentPower(1, -1);
    }
    else if (!frontLeft && !frontRight && !backLeft && backRight)
    {
        SetBothWheelsPercentPower(0, -1);
    }
    else if (frontLeft && frontRight && backLeft && !backRight)
    {
        SetBothWheelsPercentPower(0, 1);
    }
    else if (!frontLeft && frontRight && backLeft && !backRight)
    {
        SetBothWheelsPercentPower(-1, 1);
    }
    else if (frontLeft && !frontRight && backLeft && !backRight)
    {
        SetBothWheelsPercentPower(0, 0);
    }
    else if (!frontLeft && !frontRight && backLeft && !backRight)
    {
        SetBothWheelsPercentPower(-1, 0);
    }
    else if (frontLeft && frontRight && !backLeft && !backRight)
    {
        SetBothWheelsPercentPower(1, 1);
    }
    else if (!frontLeft && frontRight && !backLeft && !backRight)
    {
        SetBothWheelsPercentPower(0, 1);
    }
    else if (frontLeft && !frontRight && !backLeft && !backRight)
    {
        SetBothWheelsPercentPower(1, 0);
    }
    else if (!frontLeft && !frontRight && !backLeft && !backRight)
    {
        SetBothWheelsPercentPower(0, 0);
    }
}



    //## I am a methode that allows to control the motor from a single joystick input in Vector 2.

    public void SetWithOneJoystick(Vector2 joystickInput)
    {
        SetWithOneJoystickUsingBooleanThreshold(joystickInput, 0.5f);

    }



    //## I am a methode that allows to control the motor from a single joystick input in Vector 2.
    //## But using a threshold to determine the direction of the movement, where the threshold is a value between 0.0 and 1.0, and the joystick input is a Vector2 where X axis is right for positive and left for negative, and Y axis is up for positive and down for negative.
    //## Y axis is up for positive and down for negative, X axis is right for positive and left for negative.
    public void SetWithOneJoystickUsingBooleanThreshold(Vector2 joystickInput, float threshold)
    {
        bool isLeft = joystickInput.x < -threshold;
        bool isRight = joystickInput.x > threshold;
        bool isForward = joystickInput.y > threshold;
        bool isBackward = joystickInput.y < -threshold;
        if (isLeft && isForward)
        {
            SetBothWheelsPercentPower(0.5f, 1.0f);
        }
        else if (isRight && isForward)
        {
            SetBothWheelsPercentPower(1.0f, 0.5f);
        }
        else if (isLeft && isBackward)
        {
            SetBothWheelsPercentPower(-0.5f, -1.0f);
        }
        else if (isRight && isBackward)
        {
            SetBothWheelsPercentPower(-1.0f, -0.5f);
        }
        else if (isLeft && !isRight)
        {
            SetBothWheelsPercentPower(0.0f, 1.0f);
        }
        else if (isRight && !isLeft)
        {
            SetBothWheelsPercentPower(1.0f, 0.0f);
        }
        else if (isForward && !isBackward)
        {
            SetBothWheelsPercentPower(1.0f, 1.0f);
        }
        else if (isBackward && !isForward)
        {
            SetBothWheelsPercentPower(-1.0f, -1.0f);
        }
        else
        {
            SetBothWheelsPercentPower(0.0f, 0.0f);
        }

    }


    public void SetWithOneJoystickUsingAnalog(Vector2 joystickInput)
    {
        float forward = joystickInput.y;
        float turn = -joystickInput.x;
        if (joystickInput.magnitude < 0.1f)
        {
            SetBothWheelsPercentPower(0.0f, 0.0f);
            return;
        }
        float leftWheel = forward - turn;
        float rightWheel = forward + turn;
        float maxPower = Mathf.Max(Mathf.Abs(leftWheel), Mathf.Abs(rightWheel));
        if (maxPower > 1.0f)
        {
            leftWheel /= maxPower;
            rightWheel /= maxPower;
        }
        SetBothWheelsPercentPower(leftWheel, rightWheel);
    }


    public void SetWheels(float leftJoystick, float rightJoystick)
    {
        SetBothWheelsPercentPower(leftJoystick, rightJoystick);
    }

    //## I am a methode that allows to control the motor from two joystick input in Vector 2
    //## With only the y axis used for the movement, where the left joystick controls the left wheel and the right joystick controls the right wheel. Y axis is up for positive and down for negative, X axis is right for positive and left for negative.
    public void SetWheelsWithDoubeJoystick2D(Vector2 leftJoystick, Vector2 rightJoystick)
    {
        SetBothWheelsPercentPower(leftJoystick.y, rightJoystick.y);
    }




    void RefreshWheelParameters()
    {
        if (m_leftWheelReferenceNode == null || m_rightWheelReferenceNode == null || m_rightWheelTopRadiusReferenceNode == null)
        {
            Debug.LogWarning("Wheel reference nodes are not assigned!", this);
            return;
        }

        Vector3 leftWheel = m_leftWheelReferenceNode.position;
        Vector3 rightWheel = m_rightWheelReferenceNode.position;
        Vector3 radiusPoint = m_rightWheelTopRadiusReferenceNode.position;
        m_distanceBetweenWheelsInMm = Mathf.Abs(Vector3.Distance(leftWheel, rightWheel) * 1000.0f);
        m_radiusOfWheelsInMm = Vector3.Distance(rightWheel, radiusPoint) * 1000.0f;
        m_diameterOfWheelsInMm = m_radiusOfWheelsInMm * 2.0f;
        m_circumferenceOfWheelsInMm = m_diameterOfWheelsInMm * Mathf.PI;
        m_maxWheelSpeedInMeterPerSec = m_circumferenceOfWheelsInMm *
                                       (m_rotationPerSecondInDegree / 360.0f) * 0.001f;
        m_distanceBetweenWheelsInMeter = m_distanceBetweenWheelsInMm / 1000.0f;

        /*
         
## Recompute the distance between the anchor point to move in real worlds units.
func refresh_wheel_parameters() -> void:
		
	var left_wheel = _left_wheel_reference_node.global_position
	var right_wheel = _right_wheel_reference_node.global_position
	var radius_point = _right_wheel_top_radius_reference_node.global_position
	
	_distance_between_wheels_in_mm = abs(left_wheel.distance_to(right_wheel) * 1000.0)
	_radius_of_wheels_in_mm = (right_wheel.distance_to(radius_point)) * 1000.0
	_diameter_of_wheels_in_mm = _radius_of_wheels_in_mm * 2.0
	_circumference_of_wheels_in_mm = _diameter_of_wheels_in_mm * PI
	
	_max_wheel_speed_in_meter_per_sec = _circumference_of_wheels_in_mm * \
									   (_rotation_per_second_in_degree / 360.0) * 0.001
	
	_distance_between_wheels_in_meter = _distance_between_wheels_in_mm / 1000.0
	
         * */
    }
    private float m_verticalVelocity;
    void Update()
{
        //# ROBOT CONTROL AND ODOMETRY CALCULATIONS
        //# DIFFERENTIAL DRIVE KINEMATIC CALCULATIONS
        //# SOURCE https://youtu.be/LrsTBWf6Wsc?t=1098
        if (m_characterToMove == null)
            return;

        // Clamp wheel inputs
        float leftInput = Mathf.Clamp(m_leftWheelPercentPower, -1f, 1f);
        float rightInput = Mathf.Clamp(m_rightWheelPercentPower, -1f, 1f);

        // Convert to wheel speeds (m/s)
        float leftSpeed = leftInput * m_maxWheelSpeedInMeterPerSec;
        float rightSpeed = rightInput * m_maxWheelSpeedInMeterPerSec;

        // Differential drive kinematics
        float linearVelocity = (leftSpeed + rightSpeed) * 0.5f;
        float angularVelocity = -(rightSpeed - leftSpeed) / m_distanceBetweenWheelsInMeter;

        // Use the SAME transform for rotation and movement
        Transform robotTransform = m_characterToMove.transform;

        // Rotate robot
        float angularDisplacementDeg =
            angularVelocity * Time.deltaTime * Mathf.Rad2Deg;

        robotTransform.Rotate(
            Vector3.up,
            angularDisplacementDeg,
            Space.Self
        );

        // Forward movement
        Vector3 displacement =
            robotTransform.forward *
            linearVelocity *
            Time.deltaTime;

        // Gravity
        if (m_useFakeGravity)
        {
            if (m_characterToMove.isGrounded)
            {
                m_verticalVelocity = -1f;
            }
            else
            {
                m_verticalVelocity -=
                    m_fakeLinearGravity * Time.deltaTime;
            }   

            displacement.y = m_verticalVelocity * Time.deltaTime;
        }

        m_characterToMove.Move(displacement);
        // Emit events
    m_events.m_onLeftWheelPercentPowerUpdated.Invoke(m_leftWheelPercentPower);
    m_events.m_onRightWheelPercentPowerUpdated.Invoke(m_rightWheelPercentPower);
    m_events.m_onLeftWheelDegreePerSecondUpdated.Invoke(leftInput * m_rotationPerSecondInDegree);
    m_events.m_onRightWheelDegreePerSecondUpdated.Invoke(rightInput * m_rotationPerSecondInDegree);

    m_leftRotationInDegreeTotal += leftInput * m_rotationPerSecondInDegree * Time.deltaTime;
    m_rightRotationInDegreeTotal += rightInput * m_rotationPerSecondInDegree * Time.deltaTime;
    m_leftRotationInDegreeTotal = m_leftRotationInDegreeTotal % 360.0f;
    m_rightRotationInDegreeTotal = m_rightRotationInDegreeTotal % 360.0f;

    m_events.m_onLeftWheelCurrentRotationUpdated.Invoke(m_leftRotationInDegreeTotal);
    m_events.m_onRightWheelCurrentRotationUpdated.Invoke(m_rightRotationInDegreeTotal);

    // Update car state
    m_carId = GetCarId();
    m_carPosition = GetCarPosition();
    m_carRotation = GetCarRotation();
    m_carEuler = GetCarEuler();
    
    m_events.m_onPositionUpdated.Invoke(m_carPosition);
    m_events.m_onRotationInQuaternion.Invoke(m_carRotation);
    m_events.m_onRotationInEuler.Invoke(m_carEuler);
    m_events.m_onCarIdUpdated.Invoke(m_carId);



    _frontWheelLeftDistance = GetFrontWheelLeftDistance();
    _frontWheelRightDistance = GetFrontWheelRightDistance();

    }




    public void SetMotorLeftForwardOn()
{
    SetLeftWheelPercentPower(1.0f);
}

public void SetMotorLeftBackwardOn()
{
    SetLeftWheelPercentPower(-1.0f);
}

public void SetMotorRightForwardOn()
{
    SetRightWheelPercentPower(1.0f);
}

public void SetMotorRightBackwardOn()
{
    SetRightWheelPercentPower(-1.0f);
}

public void SetMotorLeftForward(bool isOn)
{
    SetLeftWheelPercentPower(isOn ? 1.0f : 0.0f);
}

public void SetMotorLeftBackward(bool isOn)
{
    SetLeftWheelPercentPower(isOn ? -1.0f : 0.0f);
}

public void SetMotorRightForward(bool isOn)
{
    SetRightWheelPercentPower(isOn ? 1.0f : 0.0f);
}

public void SetMotorRightBackward(bool isOn)
{
    SetRightWheelPercentPower(isOn ? -1.0f : 0.0f);
}

    public void SetMotorsOff() { 
        SetBothWheelsPercentPower(0.0f, 0.0f);
    }


    [SerializeField] float _frontWheelLeftDistance = 0.0f;
    [SerializeField] float _frontWheelRightDistance = 0.0f;

    

    public float GetFrontWheelLeftDistance() { 
        _frontWheelLeftDistance = GetDistanceFromRaycast(m_raycastFrontLeftWheel);
        return _frontWheelLeftDistance;
    }
    public float GetFrontWheelRightDistance() { 
        _frontWheelRightDistance = GetDistanceFromRaycast(m_raycastFrontRightWheel);
        return _frontWheelRightDistance;
    }


    public Transform GetCenterWheelForward() { 
        return m_carCenterReferenceNode;
    }

    public Transform GetCenterWheelGroundForward() { 
        return m_carCenterGroundReferenceNode;
    }


    public float GetDistanceFromRaycast(Ks4036Raycast3D raycast)
    {
        if (raycast == null)
        {
            return 0.0f;
        }
        return raycast.GetDistanceToGround();
    }


    public int GetCarId()
    {
        return gameObject.GetInstanceID();
    }
    public Vector3 GetCarPosition()
    {
        return m_carCenterGroundReferenceNode.position;
    }
    public Quaternion GetCarRotation()
    {
        return m_carCenterGroundReferenceNode.rotation;
    }
    public Vector3 GetCarEuler()
    {
        return m_carCenterGroundReferenceNode.rotation.eulerAngles;
    }


}

