using UnityEngine;

[System.Serializable]
public class Dialogue_Piece
{
    //The text itself
    public string Text;
    //The time it takes to write this text
    public float Duration;
    //The delay before writing this dialogue
    public float Delay;
    //The index in the dialogue this text is in
    public int Index;

    public Dialogue_Piece(string text, float duration, float delay, int index) 
    {
        Text = text;
        Duration = duration;
        Delay = delay;
        Index = index;
    }
}
