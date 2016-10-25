using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 完全にランダムなAI
/// </summary>
public class Game_AI_Random : Game_AI_Base
{
    public Game_AI_Random(Game_Field.StoneColor stoneColor)
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
        // 盤面シミュレータを作成
        var simulateField = GenerateSimulateFieldWithGameField(gameField);
        // 石を置けるマスを取得
        var puttableCellInfos = simulateField.GetPuttableCellInfos(stoneColor);
        // ランダムで返すのみ
        return puttableCellInfos[UnityEngine.Random.Range(0, puttableCellInfos.Count)];
    }
}
