using UnityEngine;

public abstract class CustomBehaviour : MonoBehaviour
{
    public void SetPosition(Vector3 position)
    {
        this.transform.position = position;
    }

    public void SetLocalPosition(Vector3 localPosition)
    {
        this.transform.localPosition = localPosition;
    }

    public Vector3 GetPosition()
    {
        return this.transform.position;
    }

    public Vector3 GetLocalPosition()
    {
        return this.transform.localPosition;
    }

    public void SetScale(Vector3 scale)
    {
        this.transform.localScale = scale;
    }

    public void SetScale(float scale)
    {
        this.transform.localScale = Vector3.one * scale;
    }

    public Vector3 GetScale()
    {
        return this.transform.lossyScale;
    }

    public Vector3 GetLocalScale()
    {
        return this.transform.localScale;
    }

    public Vector3 GetRotationEulerAngle()
    {
        return this.transform.rotation.eulerAngles;
    }

    public Vector3 GetLocalRotationEulerAngle()
    {
        return this.transform.localRotation.eulerAngles;
    }

    public void SetRotation(float rotation)
    {
        SetRotation(new Vector3(0, 0, rotation));
    }

    public void SetRotation(Vector3 eulerAngle)
    {
        this.transform.rotation = Quaternion.Euler(eulerAngle);
    }

    public void SetLocalRotation(Vector3 eulerAngle)
    {
        this.transform.localRotation = Quaternion.Euler(eulerAngle);
    }

    public void SetActive(bool active)
    {
        this.gameObject.SetActive(active);
    }

    public bool IsActive()
    {
        return this.gameObject.activeInHierarchy;
    }

    public void SetParent(Transform parent)
    {
        this.transform.SetParent(parent);
    }
}