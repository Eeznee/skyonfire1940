using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class Log : MonoBehaviour
{
    private class Message
    {
        const float messageTime = 5f;

        public string content;
        public string type;

        private float spawnTime;
        public Message(string _content, string _type)
        {
            content = _content;
            type = _type;
            spawnTime = Time.time;
        }
        public float Fade()
        {
            return Mathf.InverseLerp(messageTime, 0f, Time.time - spawnTime);
        }
        public bool Expired()
        {
            return spawnTime + messageTime < Time.time;
        }
    }
    public bool newToBottom;
    private Text text;

    private static List<Message> messages = new List<Message>(0);
    public static List<SofAircraft[]> squadrons = new List<SofAircraft[]>(0);
    public static void Print(string message, string type)
    {
        for (int i = 0; i < messages.Count; i++)
        {
            if (messages[i].type == type)
                messages.RemoveAt(i);
        }
        messages.Add(new Message(message, type));
    }


    private void ResetLogs()
    {
        messages = new List<Message>(0);
        squadrons = new List<SofAircraft[]>(0);
    }
    void Awake()
    {
        text = GetComponent<Text>();
        ResetLogs();
    }

    void Update()
    {
        float maxFade = 0f;
        text.text = "";
        for (int i = 0; i < messages.Count; i++)
        {
            int index = newToBottom ? i : messages.Count - 1 - i;
            if (messages[index].Expired()) messages.RemoveAt(index);
            else
            {
                text.text += messages[index].content + "\n";
                if (messages[i].Fade() > maxFade) maxFade = messages[i].Fade();
            }
        }
        Color col = Color.white;
        col.a = maxFade > 0.3f ? 1f : Mathf.InverseLerp(0f, 0.3f, maxFade);
        text.color = col;
    }
}
