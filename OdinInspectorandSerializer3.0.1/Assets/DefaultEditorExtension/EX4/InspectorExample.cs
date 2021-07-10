using UnityEngine;

public enum Course
{
    Chinese,
    Mathematics,
    English
}

public class InspectorExample : MonoBehaviour
{
    public int intValue;
    public float floatValue;
    public string stringValue;
    public bool boolValue;
    public Vector3 vector3Value;
    public Course enumValue = Course.Chinese;
    public Color colorValue = Color.white;
    public Texture textureValue;
}
