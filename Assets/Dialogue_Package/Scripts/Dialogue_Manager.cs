using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue_Manager : MonoBehaviour
{
    [SerializeField] private Text _displayText;
    public Dialogue_Object CurrentDialogue;
    private bool _writingText;
    private List<Dialogue_Piece> _dialogueList = new List<Dialogue_Piece>();

    private void Awake()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) 
        {
            TalkToDialogue();
        }
    }

    void SetCurrentDialogue(Dialogue_Object dialogue) 
    {
        CurrentDialogue = dialogue;
        FillDialogueList();
    }

    void FillDialogueList() 
    {
        _dialogueList.Clear();
        int index = CurrentDialogue.CurrentIndex;
        foreach (Dialogue_Piece d in CurrentDialogue.PiecesOfDialogue)
        {
            if (d.Index == index)
            {
                _dialogueList.Add(d);
            }
        }
    }

    void TalkToDialogue() 
    {
        if (!_writingText) 
        {
            StartCoroutine(WriteText());
        }
    }

    IEnumerator WriteText() 
    {
        _writingText = true;
        _displayText.text = "";
        foreach (Dialogue_Piece d in _dialogueList)
        {
            string currentText = d.Text;
            float duration = (d.Duration / currentText.Length);
            yield return new WaitForSeconds(d.Delay);
            for (int i = 0; i < currentText.Length - 1; i++)
            {
                yield return new WaitForSeconds(duration);
                _displayText.text = _displayText.text + d.Text[i];
            }
        }
        _writingText = false;
        CurrentDialogue.CurrentIndex++;
        FillDialogueList();
    }
}
