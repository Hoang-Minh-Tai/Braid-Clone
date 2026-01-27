using UnityEngine;

[CreateAssetMenu(fileName = "CloudTextData_SO", menuName = "Scriptable Objects/CloudTextData_SO")]
public class CloudTextData_SO : ScriptableObject
{
    public string headerText;
    [TextArea(3, 10)]
    public string[] bodyTexts;

}
