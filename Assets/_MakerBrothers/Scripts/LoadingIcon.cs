using UnityEngine;

public class LoadingIcon : MonoBehaviour
{
    [SerializeField]
    float speed;
    RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
        rect.Rotate(0, 0, speed);
    }
}
