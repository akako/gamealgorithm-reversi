using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// ゲームシーン制御クラス
/// </summary>
public class Game_SceneController : MonoBehaviour
{
    public static Game_SceneController Instance
    {
        get { return instance; }
    }

    static Game_SceneController instance;

    [SerializeField]
    Game_Field field;
    [SerializeField]
    Game_Message message;

    int turnNumber;

    Game_Field.StoneColor CurrentPlayerStoneColor
    {
        get { return (Game_Field.StoneColor)((turnNumber + 1) % 2 + 1); }
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        GameStart();
    }

    public void GameStart()
    {
        turnNumber = 0;
        field.Initialize();
        IncrementTurnNumber();
    }

    /// <summary>
    /// マスがクリックされた時の処理です
    /// </summary>
    /// <param name="cell">Cell.</param>
    public void OnCellClick(Game_Cell cell)
    {
        field.Lock();
        cell.StoneColor = Instance.CurrentPlayerStoneColor;
        Game_SoundManager.Instance.put.Play();
        field.TurnOverStoneIfPossible(cell);
    }

    public void OnTurnStoneFinished()
    {
        StartCoroutine(NextTurnCoroutine());
    }

    IEnumerator NextTurnCoroutine()
    {
        if (field.CountStone(Game_Field.StoneColor.None) == 0)
        {
            yield return message.Show("GAME FINISHED");
            StartCoroutine(GameFinishedCoroutine());
        }
        else
        {
            IncrementTurnNumber();
            if (field.CountClickableCells() == 0)
            {
                yield return message.Show(string.Format("{0} cannot put stone. TURN SKIPPED", CurrentPlayerStoneColor.ToString()));
                IncrementTurnNumber();
                if (field.CountClickableCells() == 0)
                {
                    yield return message.Show(string.Format("{0} cannot put stone too. GAME FINISHED", CurrentPlayerStoneColor.ToString()));
                    StartCoroutine(GameFinishedCoroutine());
                }
            }
        }
    }

    void IncrementTurnNumber()
    {
        turnNumber++;
        field.UpdateCellsClickable(CurrentPlayerStoneColor);
    }

    IEnumerator GameFinishedCoroutine()
    {
        var blackCount = field.CountStone(Game_Field.StoneColor.Black);
        var whiteCount = field.CountStone(Game_Field.StoneColor.White);

        yield return message.Show(string.Format("{0}\nBlack[{1}] : White[{2}]", 
                blackCount > whiteCount ? "Black WIN!!" : (blackCount < whiteCount ? "White WIN!!" : "DRAW"),
                blackCount,
                whiteCount));

        GameStart();
    }
}
