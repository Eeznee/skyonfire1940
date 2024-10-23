using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using UnityEditor;
#endif

[System.Serializable]
public class ProjectileProperties
{
    public string name;
    public float diameter;
    public float mass;

    public float baseVelocity;
    public float basePenetration;

    public BulletHits bulletHits;

    public bool ap;
    public bool explosive;
    public bool incendiary;
    public bool tracer;
    public ExplosiveFiller filler;
    public float fuze = 0f;

    //Editor
    public bool displayAsBullet = false;
    public bool approximated;

    public float Energy => baseVelocity * baseVelocity * mass * 0.5f;


    public ProjectileProperties(string n, float m, float d, float bv, float bp)
    {
        name = n;
        mass = m;
        diameter = d;
        baseVelocity = bv;
        basePenetration = bp;
        ap = explosive = incendiary = tracer = false;
    }
    public static ProjectileProperties Fragment()
    {
        return new ProjectileProperties("Shrapnel", 0.05f, 10f, 50f, 10f);
    }

    public string AutoName()
    {
        string n = "";
        if (ap) n += "-AP";
        if (explosive) n += "-HE";
        if (incendiary) n += "-I";
        if (tracer) n += "-T";
        if (n.StartsWith("-")) n = n.Remove(0, 1);
        return n == "" ? "Ball" : n;
    }
    public float ApproxPenetration()
    {
        if (explosive && !ap) return 0.5f;

        float pen = Ballistics.ApproximatePenetration(mass, baseVelocity, diameter);
        if (ap) pen *= 1.25f;
        if (tracer) pen *= 0.9f;
        if (incendiary) pen *= 0.9f;
        return pen;
    }

    public float FireChance()
    {
        if (explosive) return 0f;

        if (incendiary) return ap ? 0.7f : 1f;

        return tracer ? 0.3f : 0f;
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(ProjectileProperties))]
    public class ProjectilePropertiesDrawer : PropertyDrawer
    {
        SerializedProperty explosive;
        SerializedProperty approximated;

        const int lines = 9;
        const float space = 1f;

        private Rect baseRect;
        private bool displayAsBullet;


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (base.GetPropertyHeight(property, label) + space) * lines;
        }
        public Rect Rec(int line, int column, int width)
        {
            float x = baseRect.position.x + baseRect.width * column / 100;
            float y = baseRect.position.y + baseRect.height * line / lines;
            return new Rect(x, y, width * baseRect.width / 100 - space, baseRect.height / lines - space);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            displayAsBullet = property.FindPropertyRelative("displayAsBullet").boolValue;
            explosive = property.FindPropertyRelative("explosive");
            approximated = property.FindPropertyRelative("approximated");
            baseRect = position;

            EditorGUI.BeginProperty(position, label, property);
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            //EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);
            int l = 0;

            if (displayAsBullet)
            {
                EditorGUI.LabelField(Rec(l, 0, 50), "Name : ");
                EditorGUI.PropertyField(Rec(l++, 50, 50), property.FindPropertyRelative("name"), GUIContent.none);

                EditorGUI.LabelField(Rec(l, 0, 40), "Armor Piercing : ");
                EditorGUI.PropertyField(Rec(l++, 40, 10), property.FindPropertyRelative("ap"), GUIContent.none);
                EditorGUI.LabelField(Rec(--l, 50, 90), "Incendiary : ");
                EditorGUI.PropertyField(Rec(l++, 90, 10), property.FindPropertyRelative("incendiary"), GUIContent.none);

                EditorGUI.LabelField(Rec(l, 0, 40), "Explosive : ");
                EditorGUI.PropertyField(Rec(l, 40, 10), explosive, GUIContent.none);
                EditorGUI.LabelField(Rec(l, 50, 90), "Tracer : ");
                EditorGUI.PropertyField(Rec(l++, 90, 10), property.FindPropertyRelative("tracer"), GUIContent.none);

                if (explosive.boolValue)
                {
                    EditorGUI.PropertyField(Rec(l++, 0, 100), property.FindPropertyRelative("filler"), GUIContent.none);

                    EditorGUI.LabelField(Rec(l, 0, 50), "Fuze (seconds) : ");
                    EditorGUI.PropertyField(Rec(l++, 50, 50), property.FindPropertyRelative("fuze"), GUIContent.none);
                }


                EditorGUI.LabelField(Rec(l, 0, 50), "Approximate Values : ");
                EditorGUI.PropertyField(Rec(l++, 50, 10), property.FindPropertyRelative("approximated"), GUIContent.none);
            }
            else
            {
                EditorGUI.LabelField(Rec(l, 0, 50), "Diameter in mm : ");
                EditorGUI.PropertyField(Rec(l++, 50, 50), property.FindPropertyRelative("diameter"), GUIContent.none);

                EditorGUI.LabelField(Rec(l, 0, 50), "Explosive : ");
                EditorGUI.PropertyField(Rec(l++, 50, 10), explosive, GUIContent.none);

                if (explosive.boolValue)
                {
                    EditorGUI.PropertyField(Rec(l++, 0, 100), property.FindPropertyRelative("filler"), GUIContent.none);

                    EditorGUI.LabelField(Rec(l, 0, 50), "Fuze (seconds) : ");
                    EditorGUI.PropertyField(Rec(l++, 50, 50), property.FindPropertyRelative("fuze"), GUIContent.none);
                }


                EditorGUI.LabelField(Rec(l, 0, 50), "Hits Effects : ");
                EditorGUI.PropertyField(Rec(l++, 50, 40), property.FindPropertyRelative("bulletHits"), GUIContent.none);
                property.FindPropertyRelative("approximated").boolValue = false;
            }


            if (!approximated.boolValue)
            {
                EditorGUI.LabelField(Rec(l, 0, 50), "Mass in kg : ");
                EditorGUI.PropertyField(Rec(l++, 50, 50), property.FindPropertyRelative("mass"), GUIContent.none);
                EditorGUI.LabelField(Rec(l, 0, 50), "Base Velocity m/s : ");
                EditorGUI.PropertyField(Rec(l++, 50, 50), property.FindPropertyRelative("baseVelocity"), GUIContent.none);
                EditorGUI.LabelField(Rec(l, 0, 50), "Penetration in mm : ");
                EditorGUI.PropertyField(Rec(l++, 50, 50), property.FindPropertyRelative("basePenetration"), GUIContent.none);
            }
            else
            {
                EditorGUI.LabelField(Rec(l, 0, 50), "Mass : ");
                EditorGUI.LabelField(Rec(l++, 50, 50), property.FindPropertyRelative("mass").floatValue + " kg");
                EditorGUI.LabelField(Rec(l, 0, 50), "Base Velocity : ");
                EditorGUI.LabelField(Rec(l++, 50, 50), property.FindPropertyRelative("baseVelocity").floatValue + " m/s");
                EditorGUI.LabelField(Rec(l, 0, 50), "Penetration : ");
                EditorGUI.LabelField(Rec(l++, 50, 50), property.FindPropertyRelative("basePenetration").floatValue.ToString("0.0") + " mm");
            }

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }
    }
#endif
}