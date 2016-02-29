using System.IO;
using UnityEngine;
using System.Collections.Generic;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters.Binary;

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
            using (BinaryReader br = new BinaryReader(file)) {
                while (file.Length > br.BaseStream.Position) {
                    HighScores.Add((float)br.ReadDouble());
                }
            }
        }

        if (File.Exists(FilePathTutorials)) {
            FileStream file = File.Open(FilePathTutorials, FileMode.Open);
            using (BinaryReader br = new BinaryReader(file)) {
                for (int i = 0; i < 16; i++) {
                    TutorialsLeft[i] = br.ReadBoolean();
                }
            }
        }
    }
	
    public void addCompletedTutorial (int code) {
        for (int i=0;i< code; i++) {
            TutorialsLeft[i] = false;
        }
        FileStream file = File.Create(FilePathTutorials);
        using (BinaryWriter bw = new BinaryWriter(file)) {
            for (int i = 0; i < 16; i++) {
                bw.Write(TutorialsLeft[i]);
            }
        }
    }
	
    public bool addHighScore (float score) {
        HighScores.Add(score);
        HighScores.Sort();
        if (HighScores.Count > 5) {
            HighScores.RemoveAt(0);
        }
        FileStream file = File.Create(FilePathScores);
        using (BinaryWriter bw = new BinaryWriter(file)) {
            for (int i = 0; i < HighScores.Count; i++) {
                bw.Write((double)HighScores[i]);
            }
        }
        if (HighScores[HighScores.Count - 1] == score)
            return true;
        return false;
    }
}
