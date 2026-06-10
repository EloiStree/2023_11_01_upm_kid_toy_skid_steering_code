using UnityEngine;

public class Ks4036Raycast3D :MonoBehaviour
{
    [SerializeField] Transform m_origin;
    [SerializeField] float m_maxDistance=20;
    [SerializeField] LayerMask m_layerMask = ~0; // Default to all layers
    [SerializeField] bool m_drawDebugRay = false;

    [Header("Debug")]
    [SerializeField] float m_currentDistance;
    [SerializeField] bool m_isHitting;

    public void Reset()
    {
        m_origin = transform;
    }

    public void Update()
    {
        RaycastHit hit;
        m_isHitting =  Physics.Raycast(m_origin.position, m_origin.forward, out hit, m_maxDistance, m_layerMask);
        if(m_isHitting)
        {
            m_currentDistance = hit.distance;
        }
        else
        {
            m_currentDistance = -0.001f;
        }
    }

    public float GetDistanceToGround()
    {
        return m_currentDistance;
    }
    public bool IsHittingGround()
    {
        return m_isHitting;
    }
}

