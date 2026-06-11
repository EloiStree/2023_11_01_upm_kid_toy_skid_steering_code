using UnityEngine;

public class CarKs4036SetMeshWithColor:MonoBehaviour
{
    public bool m_duplicateMaterialAtStart = true;
    [SerializeField] MeshRenderer m_meshRenderer;

    private void Reset()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
    }
    private void Start()
    {
        if (m_meshRenderer == null)
        {
            m_meshRenderer = GetComponent<MeshRenderer>();
        }
        if (m_duplicateMaterialAtStart && m_meshRenderer != null)
        {
            m_meshRenderer.material = new Material(m_meshRenderer.material);
        }
    }

    public void SetMeshColor(Color color)
    {
        if (m_meshRenderer != null)
        {
            m_meshRenderer.material.color = color;
        }
    }
}

