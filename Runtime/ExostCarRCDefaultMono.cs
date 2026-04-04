using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using UnityEngine;
using UnityEngine.Events;


public class ExostCarRCDefaultMono : MonoBehaviour
{

    public bool m_buttonLeftFrontOn;
    public bool m_buttonRightFrontOn;
    public bool m_buttonLeftBackOn;
    public bool m_buttonRightBackOn;


    public float m_timeToDoAFullRotationOnPivot = 1.5f;
    public float m_turnHardAmplification = 1.3f;
    public float m_turnRatioControl = 1;
    public float m_forwardDistancePerSecondInMeter = 1;

    public float m_wheelTurnPerSecond=1;

    public Transform m_whatToMove;
    public Transform m_exostCarToMoveDirection;
    public Transform m_pivotLeftFront;
    public Transform m_pivotRightFront;
    public Transform m_pivotLeftBack;
    public Transform m_pivotRightBack;
    public Transform m_pivotRightBackWheelTopAnchor;

    public CarRCWheelSpeedMono m_motorLeftFrontOn;
    public CarRCWheelSpeedMono m_motorRightFrontOn;
    public CarRCWheelSpeedMono m_motorLeftBackOn;
    public CarRCWheelSpeedMono m_motorRightBackOn;

    public void SetTimeToMakeFullTurnAroundPivot(float timeToMakeFullTurn)
    {
        m_timeToDoAFullRotationOnPivot = timeToMakeFullTurn;
    }

    public float m_deductedMoveSpeedDuringRotationPerSecond = 1;


    public float m_frontWheelDistance = 0;
    public float m_wheelHeight = 0;
    public float m_wheelDistancePerTurn = 0;
    public float m_pivotCircleDistance;
    public float m_distancePerSecond;
    public float m_wheelRotationPerSecond;
    public float m_wheelRotationAnglePerSecond;
    public float m_pivotAnglePerSecond;
    public float m_pivotAnglePerSecondAmplified;

    [ContextMenu("Compute Deducted Info")]
    public void ComputeDeductedInfo()
    {

        m_pivotAnglePerSecond = 360 / m_timeToDoAFullRotationOnPivot;
        m_pivotAnglePerSecondAmplified = m_turnHardAmplification* m_pivotAnglePerSecond;
        m_frontWheelDistance = Vector3.Distance(m_pivotLeftFront.position, m_pivotRightFront.position);
        m_wheelHeight = Vector3.Distance(m_pivotRightBack.position, m_pivotRightBackWheelTopAnchor.position);
        m_wheelDistancePerTurn = m_wheelHeight * 2 * (float)Math.PI;
        m_pivotCircleDistance = m_frontWheelDistance * 2 * (float)Math.PI;
        m_distancePerSecond = m_pivotCircleDistance / m_timeToDoAFullRotationOnPivot;
        m_wheelRotationPerSecond = m_distancePerSecond / m_wheelDistancePerTurn;
        m_wheelRotationAnglePerSecond = m_wheelRotationPerSecond * 360f;

        
    }



    [System.Serializable]
    public class UnityEventBool : UnityEvent<bool> { }
    //public AnimationCurve m_motorStartResistance;

    public void SetWheelRotationSpeed(float rotationSpeed) {

        m_motorLeftFrontOn.m_rotationSpeedWhenOn = rotationSpeed;
        m_motorRightFrontOn.m_rotationSpeedWhenOn = rotationSpeed;
        m_motorLeftBackOn.m_rotationSpeedWhenOn = rotationSpeed;
        m_motorRightBackOn.m_rotationSpeedWhenOn = rotationSpeed;


    }

    public void SetWheelFront() => SetWheelState(1, 1, 1, 1);
    public void SetWheelBack() => SetWheelState(-1, -1, -1, -1);
    public void SetWheelStop() => SetWheelState(0,0,0,0);


    public void SetMotorLeftFront(bool isMotorOn)
    {
        m_buttonLeftFrontOn = isMotorOn;
    }
    public void SetMotorRightFront(bool isMotorOn)
    {
        m_buttonRightFrontOn = isMotorOn;
    }
    public void SetMotorLeftBack(bool isMotorOn)
    {
        m_buttonLeftBackOn = isMotorOn;
    }
    public void SetMotorRightBack(bool isMotorOn)
    {
        m_buttonRightBackOn = isMotorOn;
    }


    public void SetTwoMotorRightTo(bool isMotorOn)
    {
        throw new NotImplementedException();
    }
    public void SetTwoMotorLeftTo(bool isMotorOn)
    {
        throw new NotImplementedException();
    }
    public void SetTwoMotorFrontTo(bool isMotorOn)
    {
        throw new NotImplementedException();
    }
    public void SetTwoMotorBackTo(bool isMotorOn)
    {
        throw new NotImplementedException();
    }

    public void SetCarAsTurningRight() {

        throw new NotImplementedException();
    }

    public void SetCarAsTurningLeft() {
        throw new NotImplementedException();
    }

    public void SetCarAsTurningRightFast()
    {

        throw new NotImplementedException();
    }
    public void SetCarAsTurningLeftFast()
    {

        throw new NotImplementedException();
    }


    public void SetWheelState(
        short leftFront,
        short rightFront,
        short leftBack,
        short rightBack
        )
    {
        SetWheel(leftFront, m_motorLeftFrontOn);
        SetWheel(rightFront, m_motorRightFrontOn);
        SetWheel(leftBack, m_motorLeftBackOn);
        SetWheel(rightBack, m_motorRightBackOn);

    }

    public void SetWheel(short state, CarRCWheelSpeedMono wheel) {
        if (state == -1) {
            wheel.SetMotorInverse(true); wheel.SetMotorOn(true);
        }
        else if (state == 0) { 
        wheel.SetMotorInverse(false); wheel.SetMotorOn(false);
        } 
        else if (state == 1) {
        wheel.SetMotorInverse(false); wheel.SetMotorOn(true);
        }
    }
    bool isCarGoodSide = false ;
    void Update()
    {
        if (!enabled || !gameObject.activeInHierarchy)
            return;
        if (ImpossibleSituationOfButtons())
            return;
        ComputeDeductedInfo();
        isCarGoodSide = m_exostCarToMoveDirection.up.y > 0f;

        SetWheelRotationSpeed(m_wheelRotationAnglePerSecond);
        m_deductedMoveSpeedDuringRotationPerSecond = Vector3.Distance(m_pivotLeftFront.position, m_pivotRightFront.position)*2*(float)Math.PI ;

        SetWheelState(0,0,0,0);
        if (MoveForward()) {
            SetWheelState(1, 1, 1, 1);
            
            if(isCarGoodSide)
                m_whatToMove.Translate(m_exostCarToMoveDirection.forward * Time.deltaTime * m_distancePerSecond, Space.World);
            else
                m_whatToMove.Translate(-m_exostCarToMoveDirection.forward * Time.deltaTime * m_distancePerSecond, Space.World);
        }
        else if (MoveBackward()) {

            SetWheelState(-1, -1, -1, -1);
            if (isCarGoodSide)
                m_whatToMove.Translate(m_exostCarToMoveDirection.forward * Time.deltaTime * -m_distancePerSecond, Space.World);
            else
                m_whatToMove.Translate(-m_exostCarToMoveDirection.forward * Time.deltaTime * -m_distancePerSecond, Space.World);
        }
        else if (TurnLeftHard())
        {
            SetWheelState(-1, 1, -1, 1);
            if (isCarGoodSide)
                m_whatToMove.Rotate(Vector3.up, Time.deltaTime * -m_pivotAnglePerSecondAmplified * m_turnRatioControl, Space.Self);
            else
                m_whatToMove.Rotate(Vector3.up, Time.deltaTime * m_pivotAnglePerSecondAmplified * m_turnRatioControl, Space.Self);
        }
        else if (TurnRightHard())
        {
            SetWheelState(1, -1, 1, -1);
            if (isCarGoodSide)
                m_whatToMove.Rotate(Vector3.up, Time.deltaTime * m_pivotAnglePerSecondAmplified * m_turnRatioControl, Space.Self);
            else
                m_whatToMove.Rotate(Vector3.up, Time.deltaTime * -m_pivotAnglePerSecondAmplified * m_turnRatioControl, Space.Self);
        }
        
        else if (TurnLeftLight()) {

            SetWheelState(0, 1, 0, 1);
            if (isCarGoodSide)
                RotateAround(RotateType.LeftFront);
            else
                RotateAround(RotateType.LeftBack,true);

        }
        else if (TurnRightLight()) {

            SetWheelState(1, 0, 1, 0);
            if (isCarGoodSide)
                RotateAround(RotateType.RightFront);
            else
                RotateAround(RotateType.RightBack, true);

        }

        else if (TurnLeftLightBackward()) {

            SetWheelState(0, -1, 0, -1);
            if (isCarGoodSide)
                RotateAround(RotateType.LeftBack);
            else
                RotateAround(RotateType.LeftFront, true);

        }
        else if (TurnRightLightBackward()) {

            SetWheelState(-1, 0, -1, 0);
            if (isCarGoodSide)
                RotateAround(RotateType.RightBack);
            else
                RotateAround(RotateType.RightFront, true);

        }

    }


    [ContextMenu("Set Car As Move Backward")]
    public void SetCarAsMoveBackward()
    {


        m_buttonLeftBackOn = true;
        m_buttonRightBackOn = true;
        m_buttonLeftFrontOn = false;
        m_buttonRightFrontOn = false;
    }

    [ContextMenu("Set Car As Move Forward")]
    public void SetCarAsMoveForward()
    {
        m_buttonLeftBackOn = false;
        m_buttonRightBackOn = false;
        m_buttonLeftFrontOn = true;
        m_buttonRightFrontOn = true;
    }
    [ContextMenu("Set Car As Turn Right Hard")]
    public void SetCarAsTurnRightHard()
    {


        m_buttonLeftBackOn = false;
            m_buttonRightBackOn = true;
        m_buttonLeftFrontOn = true;
       m_buttonRightFrontOn = false;
    }
    [ContextMenu("Set Car As Turn Left Hard")]
    public void SetCarAsTurnLeftHard()
    {
         m_buttonLeftBackOn = true;
        m_buttonRightBackOn = false;
        m_buttonLeftFrontOn = false;
        m_buttonRightFrontOn = true;
    }

    [ContextMenu("Set Car As Turn Left Light")]
    public void SetCarAsTurnLeftLight()
    {

         m_buttonLeftBackOn = false;
        m_buttonRightBackOn = false;
        m_buttonLeftFrontOn = false;
        m_buttonRightFrontOn = true;
    }

    [ContextMenu("Set Car As Turn Right Light")]
    public void SetCarAsTurnRightLight()
    {
        m_buttonLeftBackOn = false;
        m_buttonRightBackOn = false;
        m_buttonLeftFrontOn = true;
        m_buttonRightFrontOn= false;
    }

    [ContextMenu("Set Car As Turn Right Light Backward")]
    public void SetCarAsTurnRightLightBackward()
    {

         m_buttonLeftBackOn = true;
        m_buttonRightBackOn = false;
        m_buttonLeftFrontOn = false;
        m_buttonRightFrontOn = false;
    }

    [ContextMenu("Set Car As Turn Left Light Backward")]
    public void SetCarAsTurnLeftLightBackward()
    {
        m_buttonLeftBackOn = false;
        m_buttonRightBackOn = true;
        m_buttonLeftFrontOn = false;
        m_buttonRightFrontOn= false;
    }

    private bool MoveBackward()
    {
        return m_buttonLeftBackOn && m_buttonRightBackOn && !m_buttonLeftFrontOn && !m_buttonRightFrontOn;
    }

    private bool MoveForward()
    {
        return !m_buttonLeftBackOn && !m_buttonRightBackOn && m_buttonLeftFrontOn && m_buttonRightFrontOn;
    }
    private bool TurnRightHard()
    {

        return !m_buttonLeftBackOn && m_buttonRightBackOn && m_buttonLeftFrontOn && !m_buttonRightFrontOn;
    }
    private bool TurnLeftHard()
    {
        return m_buttonLeftBackOn && !m_buttonRightBackOn && !m_buttonLeftFrontOn && m_buttonRightFrontOn;
    }


    private bool TurnLeftLight()
    {

        return !m_buttonLeftBackOn && !m_buttonRightBackOn && !m_buttonLeftFrontOn && m_buttonRightFrontOn;
    }

    private bool TurnRightLight()
    {

        return !m_buttonLeftBackOn && !m_buttonRightBackOn && m_buttonLeftFrontOn && !m_buttonRightFrontOn;
    }

    private bool TurnRightLightBackward()
    {

        return m_buttonLeftBackOn && !m_buttonRightBackOn && !m_buttonLeftFrontOn && !m_buttonRightFrontOn;
    }

    private bool TurnLeftLightBackward()
    {
        return !m_buttonLeftBackOn && m_buttonRightBackOn && !m_buttonLeftFrontOn && !m_buttonRightFrontOn;
    }

    public enum RotateType { LeftFront, RightFront, LeftBack, RightBack}
    private void RotateAround(RotateType rotateType, bool inverseRotation=false)
    {

        Transform whatToMove = m_whatToMove;
        Transform rotationPoint = null;
        bool inverseAngle = false;
        bool isBack = false;
        switch (rotateType)
        {
            case RotateType.LeftFront:
                rotationPoint = m_pivotLeftFront;
                break;
            case RotateType.RightFront:
                rotationPoint = m_pivotRightFront;
                inverseAngle = true;


                break;
            case RotateType.LeftBack:
                rotationPoint = m_pivotLeftBack;
                isBack = true;
                break;
            case RotateType.RightBack:
                rotationPoint = m_pivotRightBack;
                inverseAngle = true;
                isBack = true;
                break;
            default:
                break;
        }

        if (rotationPoint!=null) {

            // Calculate the relative position of the object to the rotation point.
            Vector3 relativePosition = m_whatToMove.position - rotationPoint.position;

            // Perform the rotation using Unity's built-in functions.
            m_whatToMove.RotateAround(rotationPoint.position, Vector3.up, (isBack?1:-1)* m_pivotAnglePerSecond* m_turnRatioControl * Time.deltaTime * (inverseAngle?-1:1) * (inverseRotation ? -1 : 1) );

            // Translate the object back to its original position (if necessary).
            //m_whatToMove.position = rotationPoint.position + relativePosition;

        }
    }




    private bool ImpossibleSituationOfButtons()
    {
        return (m_buttonRightBackOn && m_buttonRightFrontOn) || (m_buttonLeftBackOn && m_buttonLeftFrontOn);
    }

   
}
