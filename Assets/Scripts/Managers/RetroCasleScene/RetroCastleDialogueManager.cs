using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ExcelDataReader;

[Serializable]
public class DialogueElement
{
    public List<string> characters;
    public List<string> dialogue;
    
    public DialogueElement()
    {
        characters = new List<string>();
        dialogue = new List<string>();
    }
}

public class RetroCastleDialogueManager : MonoBehaviour
{
    public DialogueController dialogueController;
    public List<string> dialoguesTextAssetsPaths;

    public List<DialogueElement> dialogueElements;

    // Start is called before the first frame update
    void Start()
    {
        dialogueElements = new List<DialogueElement>();
        LoadData();
    }


    void LoadData()
    {
        foreach (var excelFile in dialoguesTextAssetsPaths)
        {
            DialogueElement newDialogueElement = new DialogueElement();
            using (var stream = File.Open(excelFile, FileMode.Open, FileAccess.Read))
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    while (reader.Read())
                    {
                        newDialogueElement.characters.Add(reader.GetString(0)); 
                        newDialogueElement.dialogue.Add(reader.GetString(1)); 
                    }
                }
            }

            dialogueElements.Add(newDialogueElement); 
        }
    }
}