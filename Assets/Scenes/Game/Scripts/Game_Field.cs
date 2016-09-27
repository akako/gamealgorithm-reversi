using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ゲームフィールド制御クラス
/// </summary>
[RequireComponent(typeof(GridLayoutGroup))]
public class Game_Field : UIBehaviour
{
    /// <summary>
    /// 石の色
    /// </summary>
    public enum StoneColor
    {
        None,
        Black,
        White
    }

    [SerializeField]
    Game_Cell cellPrefab;

    List<Game_Cell> cells = new List<Game_Cell>();
    int turnStoneForDirectionIfPossibleCoroutineCount;

    protected override void Awake()
    {
        base.Awake();
        for (var y = 0; y < 8; y++)
        {
            for (var x = 0; x < 8; x++)
            {
                var cell = Instantiate(cellPrefab);
                cell.transform.SetParent(transform);
                cell.Initialize(x, y);
                cells.Add(cell);
            }
        }
        cellPrefab.gameObject.SetActive(false);
    }

    /// <summary>
    /// フィールド初期化処理
    /// </summary>
    public void Initialize()
    {
        // 全てのマスを空に
        foreach (var cell in cells)
        {
            cell.StoneColor = StoneColor.None;
        }

        // 中央の4マスに初期石を配置
        cells.First(c => c.X == 3 && c.Y == 3).StoneColor = StoneColor.White;
        cells.First(c => c.X == 4 && c.Y == 4).StoneColor = StoneColor.White;
        cells.First(c => c.X == 3 && c.Y == 4).StoneColor = StoneColor.Black;
        cells.First(c => c.X == 4 && c.Y == 3).StoneColor = StoneColor.Black;
    }

    /// <summary>
    /// 全てのマスをクリック不可にします
    /// </summary>
    public void Lock()
    {
        foreach (var cell in cells)
        {
            cell.IsClickable = false;
        }
    }

    /// <summary>
    /// 指定したマスを起点に、可能であれば石をひっくり返します
    /// </summary>
    /// <param name="cell">Cell.</param>
    public void TurnOverStoneIfPossible(Game_Cell cell)
    {
        StartCoroutine(TurnOverStoneIfPossibleCoroutine(cell));
    }

    /// <summary>
    /// 指定色の石の数を数えます
    /// </summary>
    /// <returns>The stone.</returns>
    /// <param name="stoneColor">Stone color.</param>
    public int CountStone(StoneColor stoneColor)
    {
        return cells.Count(x => x.StoneColor == stoneColor);
    }

    /// <summary>
    /// クリック可能なマスの数を数えます
    /// </summary>
    /// <returns>The clickable cells.</returns>
    public int CountClickableCells()
    {
        return cells.Count(x => x.IsClickable);
    }

    /// <summary>
    /// 各マスのクリック可否状態を更新します
    /// </summary>
    public void UpdateCellsClickable(StoneColor stoneColor)
    {
        foreach (var cell in cells)
        {
            cell.IsClickable = IsStonePuttableCell(cell, stoneColor);
        }
    }

    /// <summary>
    /// 石が配置可能なマスかどうか確認します
    /// </summary>
    /// <returns><c>true</c> if this instance is stone puttable cell the specified cell; otherwise, <c>false</c>.</returns>
    /// <param name="cell">Cell.</param>
    bool IsStonePuttableCell(Game_Cell cell, StoneColor stoneColor)
    {
        if (cell.StoneColor == StoneColor.None)
        {
            // マスが空の場合は、該当マスから8方向に対して相手の石を挟める状態かどうかチェック
            return ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(cell, stoneColor, -1, -1) ||
            ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(cell, stoneColor, -1, 0) ||
            ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(cell, stoneColor, -1, 1) ||
            ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(cell, stoneColor, 0, -1) ||
            ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(cell, stoneColor, 0, 1) ||
            ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(cell, stoneColor, 1, -1) ||
            ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(cell, stoneColor, 1, 0) ||
            ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(cell, stoneColor, 1, 1);
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 任意のマスから指定された方向に対し、相手の石を挟める場所に自分の石が配置されているかチェックします
    /// </summary>
    /// <returns><c>true</c>, if own stone at the other side of enemy stone for direction was existsed, <c>false</c> otherwise.</returns>
    /// <param name="cell">Cell.</param>
    /// <param name="xDirection">X direction.</param>
    /// <param name="yDirection">Y direction.</param>
    bool ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(Game_Cell cell, StoneColor stoneColor, int xDirection, int yDirection)
    {
        var x = cell.X;
        var y = cell.Y;

        var existsEnemyStone = false;
        while (true)
        {
            x += xDirection;
            y += yDirection;
            var targetCell = GetCell(x, y);
            if (null == targetCell || targetCell.StoneColor == StoneColor.None)
            {
                return false;
            }
            else if (targetCell.StoneColor == stoneColor)
            {
                return existsEnemyStone;
            }
            else
            {
                existsEnemyStone = true;
            }
        }
    }

    /// <summary>
    /// 石をひっくり返すコルーチンです
    /// </summary>
    /// <returns>The over stone if possible coroutine.</returns>
    /// <param name="cell">Cell.</param>
    IEnumerator TurnOverStoneIfPossibleCoroutine(Game_Cell cell)
    {
        // 誤操作防止のため全マスをクリック不可にする
        foreach (var c in cells)
        {
            c.IsClickable = false;
        }

        // 8方向それぞれに対し、ひっくり返す処理を実行
        turnStoneForDirectionIfPossibleCoroutineCount = 8;
        StartCoroutine(TurnStoneForDirectionIfPossibleCoroutine(cell, -1, -1));
        StartCoroutine(TurnStoneForDirectionIfPossibleCoroutine(cell, -1, 0));
        StartCoroutine(TurnStoneForDirectionIfPossibleCoroutine(cell, -1, 1));
        StartCoroutine(TurnStoneForDirectionIfPossibleCoroutine(cell, 0, -1));
        StartCoroutine(TurnStoneForDirectionIfPossibleCoroutine(cell, 0, 1));
        StartCoroutine(TurnStoneForDirectionIfPossibleCoroutine(cell, 1, -1));
        StartCoroutine(TurnStoneForDirectionIfPossibleCoroutine(cell, 1, 0));
        StartCoroutine(TurnStoneForDirectionIfPossibleCoroutine(cell, 1, 1));
        while (turnStoneForDirectionIfPossibleCoroutineCount > 0)
        {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.5f);

        Game_SceneController.Instance.OnTurnStoneFinished();
    }

    /// <summary>
    /// 石をひっくり返すコルーチンです（指定した1方向に対して処理をかける実働メソッド）
    /// </summary>
    /// <returns>The stone for direction if possible coroutine.</returns>
    /// <param name="cell">Cell.</param>
    /// <param name="xDirection">X direction.</param>
    /// <param name="yDirection">Y direction.</param>
    IEnumerator TurnStoneForDirectionIfPossibleCoroutine(Game_Cell cell, int xDirection, int yDirection)
    {
        if (!ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(cell, cell.StoneColor, xDirection, yDirection))
        {
            turnStoneForDirectionIfPossibleCoroutineCount--;
            yield break;
        }

        var x = cell.X;
        var y = cell.Y;
        while (true)
        {
            x += xDirection;
            y += yDirection;
            var targetCell = GetCell(x, y);
            if (null == targetCell || targetCell.StoneColor == cell.StoneColor)
            {
                turnStoneForDirectionIfPossibleCoroutineCount--;
                yield break;
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                targetCell.StoneColor = cell.StoneColor;
                Game_SoundManager.Instance.turn.Play();
            }
        }
    }

    /// <summary>
    /// 指定場所のマスを取得します
    /// </summary>
    /// <returns>The cell.</returns>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    Game_Cell GetCell(int x, int y)
    {
        return cells.FirstOrDefault(c => c.X == x && c.Y == y);
    }
}
