using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// ゲームシーン制御クラス
/// </summary>
public class Game_SceneController : MonoBehaviour
{
    public static Game_SceneController Instance
    {
        get { return instance; }
    }

    public bool IsAITurn
    {
        get
        {
            return ais.ContainsKey(CurrentPlayerStoneColor);
        }
    }

    /// <summary>
    /// 今の手番プレイヤーの石の色
    /// </summary>
    /// <value>The color of the current player stone.</value>
    Game_Field.StoneColor CurrentPlayerStoneColor
    {
        get { return (Game_Field.StoneColor)((turnNumber + 1) % 2 + 1); }
    }

    static Game_SceneController instance;

    [SerializeField]
    Game_Field field;
    [SerializeField]
    Game_Message message;

    int turnNumber;
    Dictionary<Game_Field.StoneColor, Game_AI_Base> ais = new Dictionary<Game_Field.StoneColor, Game_AI_Base>();

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

        // AI設定
        ais.Clear();
        ais.Add(Game_Field.StoneColor.Black, new Game_AI_Random(Game_Field.StoneColor.Black));
        //ais.Add(Game_Field.StoneColor.White, new Game_AI_Theory(Game_Field.StoneColor.White));

        field.Initialize();
        StartCoroutine(NextTurnCoroutine());
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
            yield break;
        }
        else
        {
            IncrementTurnNumber();
            if (field.CountPuttableCells() == 0)
            {
                // 石を置ける場所が無いならパス
                yield return message.Show(string.Format("{0} cannot put stone. TURN SKIPPED", CurrentPlayerStoneColor.ToString()));
                IncrementTurnNumber();
                if (field.CountPuttableCells() == 0)
                {
                    // もう一方も石を置ける場所が無いならゲーム終了
                    yield return message.Show(string.Format("{0} cannot put stone too. GAME FINISHED", CurrentPlayerStoneColor.ToString()));
                    StartCoroutine(GameFinishedCoroutine());
                    yield break;
                }
            }
        }
        // AIの手番の場合は、1秒待ってから処理を実行
        if (IsAITurn)
        {
            yield return new WaitForSeconds(1f);
            var resultCell = ais[CurrentPlayerStoneColor].GetNextMove(field);
            OnCellClick(field.cells.First(x => x.X == resultCell.x && x.Y == resultCell.y));
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
