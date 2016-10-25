using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Game_AI_Random : Game_AI_Base
{
    public Game_AI_Random(Game_Field.StoneColor stoneColor)
    {
        this.stoneColor = stoneColor;
    }

    public override CellInfo GetNextMove(Game_Field gameField)
    {
        var simulateField = GenerateSimulateFieldWithGameField(gameField);
        var puttableCellInfos = simulateField.GetPuttableCellInfos(stoneColor);
        if (puttableCellInfos.Count == 0)
        {
            return null;
        }
        return puttableCellInfos[UnityEngine.Random.Range(0, puttableCellInfos.Count)];
    }
}
