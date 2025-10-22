using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SongManager : MonoBehaviour
{
    public AudioSource musicSource;
    public GameObject notePrefab;
    public Transform[] lanes; // 0 - левый, 3 - правый
    public string notesFileName = "notes.txt";
    public float noteSpeed = 5f;

    private List<NoteData> notes = new List<NoteData>();
    private float songStartTime;
    private bool isPlaying = false;
    private int nextNoteIndex = 0;

    [System.Serializable]
    public class NoteData
    {
        public float time;
        public int lane;
        public NoteData(float t, int l)
        {
            time = t;
            lane = l;
        }
    }

    void Start()
    {
        LoadNotes();
    }

    void Update()
    {
        if (!isPlaying) return;

        float songTime = Time.time - songStartTime;

        while (nextNoteIndex < notes.Count && songTime >= notes[nextNoteIndex].time - 2f)
        {
            SpawnNote(notes[nextNoteIndex]);
            nextNoteIndex++;
        }
    }

    public void PlaySong()
    {
        songStartTime = Time.time;
        musicSource.Play();
        isPlaying = true;
    }

    void LoadNotes()
    {
        string path = Path.Combine(Application.streamingAssetsPath, notesFileName);
        if (!File.Exists(path))
        {
            Debug.LogError("Notes file not found: " + path);
            return;
        }

        string[] lines = File.ReadAllLines(path);
        foreach (string line in lines)
        {
            string[] parts = line.Split(' ');
            if (parts.Length >= 2)
            {
                float time = float.Parse(parts[0]);
                int lane = int.Parse(parts[1]);
                notes.Add(new NoteData(time, lane));
            }
        }

        notes.Sort((a, b) => a.time.CompareTo(b.time));
        Debug.Log("Loaded " + notes.Count + " notes.");

        GameManager.instance.SetTotalNotes(notes.Count);
    }

    void SpawnNote(NoteData note)
{
    if (note.lane < 0 || note.lane >= lanes.Length) return;

    Transform lane = lanes[note.lane];

    // Спавним ноту выше (например, на Y + 5)
    Vector3 spawnPos = new Vector3(lane.position.x, lane.position.y + 7f, lane.position.z);

    GameObject newNote = Instantiate(notePrefab, spawnPos, Quaternion.identity);
    newNote.GetComponent<NoteObject>().Init(note, noteSpeed);
}

}
