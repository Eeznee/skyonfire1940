using UnityEngine;
using System;
using UnityEngine.UI;

public class CreditsTextDisplay : MonoBehaviour
{
    [System.Serializable]
    public struct CreditsCategory
    {
        public string category;
        public string[] names;
    }

    public CreditsCategory[] categoriesAndNames;
    public Text categoriesText;
    public Text namesText;

    public int linesBetweenCategories = 2;


    private void Start()
    {
        namesText.text = NamesString();
        categoriesText.text = CategoryString();
    }

    public string CategoryString()
    {
        string txt = "";
        foreach (CreditsCategory categoryAndName in categoriesAndNames)
        {
            txt += categoryAndName.category;
            for (int i = 0; i < categoryAndName.names.Length; i++) txt += "\n";
            for(int i = 0; i < linesBetweenCategories; i++) txt += "\n";
        }
        return txt;
    }

    public string NamesString()
    {
        string txt = "";
        foreach (CreditsCategory categoryAndName in categoriesAndNames)
        {
            foreach (string name in categoryAndName.names) txt += name + "\n";
            for (int i = 0; i < linesBetweenCategories; i++) txt += "\n";
        }
        return txt;
    }
}
