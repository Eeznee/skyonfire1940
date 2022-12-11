using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class PersistentPath : MonoBehaviour
{
    void Start()
    {
        GetComponent<InputField>().text = Application.persistentDataPath;
    }
}
