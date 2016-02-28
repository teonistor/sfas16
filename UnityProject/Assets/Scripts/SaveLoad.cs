using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveLoad : MonoBehaviour {

    private string FilePathScores;
    private string FilePathTutorials;
    public List<float> HighScores { get; private set; }
    public int TutorialPattern { get; private set; }


    void Awake () {
        FilePathScores = Path.Combine(Application.persistentDataPath, "hscore");
        FilePathTutorials = Path.Combine(Application.persistentDataPath, "tutorial");

        if (File.Exists(FilePathScores)) {
            FileStream file = File.Open(FilePathScores, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            HighScores = (List<float>)bf.Deserialize(file);
            file.Close();
        }
        else {
            HighScores = new List<float>();
        }

        if (File.Exists(FilePathTutorials)) {
            FileStream file = File.Open(FilePathTutorials, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            TutorialPattern = (int)bf.Deserialize(file);
            file.Close();
        }
        else {
            TutorialPattern = 0;//or some 111111 bit pattern?
            //TutorialPattern = 131071; //2^17-2, or 16 1s followed by a 0. 
        }
    }
	
    public void addCompletedTutorial (int i) {
        TutorialPattern |= 1 << i;
        /*FileStream file = File.Create(FilePathTutorials);
        BinaryFormatter bf = new BinaryFormatter();
        //TutorialPattern = (int)bf.Deserialize(file);
        file.Close();*/ //obscured for now
    }
	
    public bool addHighScore (float score) {
        HighScores.Add(score);
        HighScores.Sort();
        if (HighScores.Count > 5) {
            HighScores.RemoveAt(0);
        }
        FileStream file = File.Create(FilePathScores);
        BinaryFormatter bf = new BinaryFormatter();
        //TutorialPattern = (int)bf.Deserialize(file); //What to do??
        file.Close();
        if (HighScores[HighScores.Count - 1] == score)
            return true;
        return false;
    }
}
