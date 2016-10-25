using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// セオリーで攻めるAI
/// </summary>
public class Game_AI_Theory : Game_AI_Base
{
    public Game_AI_Theory(Game_Field.StoneColor stoneColor)
    {
        this.stoneColor = stoneColor;
    }

    /// <summary>
    /// 次の手を取得します
    /// </summary>
    /// <returns>The next move.</returns>
    /// <param name="gameField">Game field.</param>
    public override CellInfo GetNextMove(Game_Field gameField)
    {
        var simulateField = GenerateSimulateFieldWithGameField(gameField);
        var cellInfoPoints = new Dictionary<CellInfo, int>();
        foreach (var cellInfo in simulateField.GetPuttableCellInfos(stoneColor))
        {
            var point = 0;

            // 角を取るための重み付け
            if (cellInfo.IsCorner)
            {
                point += 50;
            }

            // 角の斜め隣には出来るだけ打たないための重み付け
            if (cellInfo.IsCornerInside && simulateField.Stage != SimulateField.StageEnum.End)
            {
                point -= 30;
            }

            // 序盤は少なく取るための重み付け
            var tmpField = GenerateSimulateFieldWithGameField(gameField);
            var turnedCellInfos = tmpField.PutStone(cellInfo.x, cellInfo.y, stoneColor);
            point -= turnedCellInfos.Count * 10;

            // 返す石が相手の石に囲まれている場所を狙う重み付け
            foreach (var cell in turnedCellInfos)
            {
                point += tmpField.CountStoneAroundCell(cell.x, cell.y, stoneColor == Game_Field.StoneColor.Black ? Game_Field.StoneColor.White : Game_Field.StoneColor.Black) * 2;
            }

            // 終盤はたくさん石を取ることを狙う重み付け
            if (simulateField.Stage == SimulateField.StageEnum.End)
            {
                point += turnedCellInfos.Count * 5;
            }

            cellInfoPoints.Add(cellInfo, point);
        }

        // ポイントが一番高いマスを返す
        return cellInfoPoints.OrderByDescending(x => x.Value).First().Key;
    }
}
