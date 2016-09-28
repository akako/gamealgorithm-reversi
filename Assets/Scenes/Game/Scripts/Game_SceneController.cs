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

    /// <summary>
    /// 今の手番プレイヤーの石の色
    /// </summary>
    /// <value>The color of the current player stone.</value>
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

    /// <summary>
    /// ゲームを開始します
    /// </summary>
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

    /// <summary>
    /// 石をひっくり返し終わった後の処理です
    /// </summary>
    public void OnTurnStoneFinished()
    {
        StartCoroutine(NextTurnCoroutine());
    }

    /// <summary>
    /// 次の手番に移るコルーチンです
    /// </summary>
    /// <returns>The turn coroutine.</returns>
    IEnumerator NextTurnCoroutine()
    {
        if (field.CountStone(Game_Field.StoneColor.None) == 0)
        {
            // マスが全て埋まったならゲーム終了
            yield return message.Show("GAME FINISHED");
            StartCoroutine(GameFinishedCoroutine());
        }
        else
        {
            IncrementTurnNumber();
            if (field.CountClickableCells() == 0)
            {
                // 石を置ける場所が無いならパス
                yield return message.Show(string.Format("{0} cannot put stone. TURN SKIPPED", CurrentPlayerStoneColor.ToString()));
                IncrementTurnNumber();
                if (field.CountClickableCells() == 0)
                {
                    // もう一方も石を置ける場所が無いならゲーム終了
                    yield return message.Show(string.Format("{0} cannot put stone too. GAME FINISHED", CurrentPlayerStoneColor.ToString()));
                    StartCoroutine(GameFinishedCoroutine());
                }
            }
        }
    }

    /// <summary>
    /// 手番番号をインクリメントし、盤面を更新します
    /// </summary>
    void IncrementTurnNumber()
    {
        turnNumber++;
        field.UpdateCellsClickable(CurrentPlayerStoneColor);
    }

    /// <summary>
    /// ゲーム終了時のコルーチンです
    /// </summary>
    /// <returns>The finished coroutine.</returns>
    IEnumerator GameFinishedCoroutine()
    {
        var blackCount = field.CountStone(Game_Field.StoneColor.Black);
        var whiteCount = field.CountStone(Game_Field.StoneColor.White);

        // 結果表示
        yield return message.Show(string.Format("{0}\nBlack[{1}] : White[{2}]", 
                blackCount > whiteCount ? "Black WIN!!" : (blackCount < whiteCount ? "White WIN!!" : "DRAW"),
                blackCount,
                whiteCount));

        // 表示終了後、次のゲーム開始
        GameStart();
    }
}
