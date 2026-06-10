using UnityEngine;

public class CarKs4036SetWheelRotationFromAngleMono : MonoBehaviour
{
    [SerializeField] private Transform m_pivotToRotateLocally;
    [SerializeField] private float m_currentAngle;
    [SerializeField] private bool m_inverse;

    public void SetCurrentAngleAndUpdate(float rotationAngleInDegree)
    {
        m_currentAngle = rotationAngleInDegree;

        float multiplier = m_inverse ? -1f : 1f;

        m_pivotToRotateLocally.localEulerAngles =
            new Vector3(-m_currentAngle * multiplier, 0f, 0f);
    }
}