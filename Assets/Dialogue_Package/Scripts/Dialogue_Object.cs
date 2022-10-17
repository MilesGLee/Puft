using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue_Object : MonoBehaviour
{
    [SerializeField] private List<Dialogue_Piece> m_piecesOfDialogue;
    public List<Dialogue_Piece> PiecesOfDialogue { get { return m_piecesOfDialogue; } }

    public string CurrentDialogue;
    public int CurrentIndex;

    private void Awake()
    {
        
    }
}
