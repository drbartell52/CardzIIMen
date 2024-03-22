using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Gameboard.Persistance;

namespace Gameboard.Examples
{
    public class RedoController : MonoBehaviour
    {
        public GameObject UndoButton;
        public GameObject RedoButton;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            UndoButton.SetActive(GameCaretaker.GetInstance().CanUndo());
            RedoButton.SetActive(GameCaretaker.GetInstance().CanRedo());
        }

        public void Undo()
        {
            GameCaretaker.GetInstance().Undo();
        }

        public void Redo()
        {
            GameCaretaker.GetInstance().Redo();
        }

        public void NewGame()
        {
            GameCaretaker.GetInstance().Clear();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}