using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoad : MonoBehaviour {

    private string FilePathScores;
    private string FilePathTutorials;
    public List<float> HighScores { get; private set; }
    public List<bool> TutorialsLeft { get; private set; }


    void Awake () {
        FilePathScores = Path.Combine(Application.persistentDataPath, "hscore");
        FilePathTutorials = Path.Combine(Application.persistentDataPath, "tutorial");

        HighScores = new List<float>();
        TutorialsLeft = new List<bool>();
        for (int i = 0; i < 16; i++) {
            TutorialsLeft.Add(true);
        }

        if (File.Exists(FilePathScores)) {
            FileStream file = File.Open(FilePathScores, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            HighScores = (List<float>)bf.Deserialize(file);
            file.Close();
        }

        if (File.Exists(FilePathTutorials)) {
            FileStream file = File.Open(FilePathTutorials, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            TutorialsLeft = (List<bool>)bf.Deserialize(file);
            file.Close();
        }
    }
	
    public void addCompletedTutorial (int code) {
        for (int i=0;i< code; i++) {
            TutorialsLeft[i] = false;
        }
        FileStream file = File.Create(FilePathTutorials);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, TutorialsLeft);
        file.Close();
    }
	
    public bool addHighScore (float score) {
        HighScores.Add(score);
        HighScores.Sort();
        if (HighScores.Count > 5) {
            HighScores.RemoveAt(0);
        }
        FileStream file = File.Create(FilePathScores);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, HighScores);
        file.Close();
        if (HighScores[HighScores.Count - 1] == score)
            return true;
        return false;
    }
}
