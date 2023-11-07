using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class writtableObject : MonoBehaviour
{
    public TMP_InputField bookInputField;
    public string BookFileName = "book.txt";

    public void SendBookMessage()
    {
        string bookText = bookInputField.text;

        if (!string.IsNullOrEmpty(bookText))
        {
            WriteToBookLog(bookText);
            bookInputField.text = "";
        }
    }

    public void WriteToBookLog(string message)
    {
        string path = Application.dataPath + "/" + BookFileName;
        //StreamWriter writer = new StreamWriter(filePath, true);
        //writer.WriteLine(message);
        //writer.Close();
    }
}
