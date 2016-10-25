using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

abstract public class Game_AI_Base
{
    protected Game_Field.StoneColor stoneColor;

    abstract public Game_AI_Base.CellInfo GetNextMove(Game_Field gameField);

    protected SimulateField GenerateSimulateFieldWithGameField(Game_Field gameField)
    {
        var cells = new Game_Field.StoneColor[Game_Field.SIZE_X, Game_Field.SIZE_Y];
        foreach (var cell in gameField.cells)
        {
            cells[cell.X, cell.Y] = cell.StoneColor;
        }
        return new SimulateField(cells);
    }

    public class CellInfo
    {
        public bool IsCorner
        { 
            get
            {
                return x == 0 && y == 0 ||
                x == 0 && y == (Game_Field.SIZE_Y - 1) ||
                x == (Game_Field.SIZE_X - 1) && y == 0 ||
                x == (Game_Field.SIZE_X - 1) && y == (Game_Field.SIZE_Y - 1);
            }
        }

        public bool IsCornerInside
        { 
            get
            {
                return x == 1 && y == 1 ||
                x == 1 && y == (Game_Field.SIZE_Y - 2) ||
                x == (Game_Field.SIZE_X - 2) && y == 1 ||
                x == (Game_Field.SIZE_X - 2) && y == (Game_Field.SIZE_Y - 2);
            }
        }

        public readonly int x;
        public readonly int y;
        public readonly Game_Field.StoneColor stoneColor;

        public CellInfo(int x, int y, Game_Field.StoneColor stoneColor)
        {
            this.x = x;
            this.y = y;
            this.stoneColor = stoneColor;
        }
    }

    protected class SimulateField
    {
        public enum StageEnum
        {
            Opening,
            Middle,
            End
        }

        public StageEnum Stage
        {
            get
            {
                var emptyCellCount = CountStone(Game_Field.StoneColor.None);
                var stagePunctuation = (float)cells.Length / 3f;
                if (emptyCellCount < stagePunctuation)
                {
                    return StageEnum.End;
                }
                else if (emptyCellCount < stagePunctuation * 2)
                {
                    return StageEnum.Middle;
                }
                else
                {
                    return StageEnum.Opening;
                }
            }
        }

        Game_Field.StoneColor[,] cells;

        public SimulateField(Game_Field.StoneColor[,] cells)
        {
            this.cells = cells;
        }

        public List<CellInfo> GetPuttableCellInfos(Game_Field.StoneColor stoneColor)
        {
            var infos = new List<CellInfo>();
            for (var x = 0; x < Game_Field.SIZE_X; x++)
            {
                for (var y = 0; y < Game_Field.SIZE_Y; y++)
                {
                    if (IsStonePuttable(x, y, stoneColor))
                    {
                        infos.Add(new CellInfo(x, y, stoneColor));
                    }
                }
            }
            return infos;
        }

        public int CountStone(Game_Field.StoneColor stoneColor)
        {
            return cells.Cast<Game_Field.StoneColor>().Count(x => x == stoneColor);
        }

        public int CountStoneAroundCell(int x, int y, Game_Field.StoneColor stoneColor)
        {
            var count = 0;
            for (var xDiff = -1; xDiff <= 1; xDiff++)
            {
                for (var yDiff = -1; yDiff <= 1; yDiff++)
                {
                    var nextX = x + xDiff;
                    var nextY = y + yDiff;
                    if (nextX < 0 || Game_Field.SIZE_X <= nextX || nextY < 0 || Game_Field.SIZE_Y <= nextY)
                    {
                        continue;
                    }
                    if ((xDiff != 0 || yDiff != 0) && cells[nextX, nextY] == stoneColor)
                    {
                        count++;
                    } 
                }
            }
            return count;
        }

        public List<CellInfo> PutStone(int x, int y, Game_Field.StoneColor stoneColor)
        {
            if (!IsStonePuttable(x, y, stoneColor))
            {
                throw new Exception(string.Format("Cannot put {0} stone to [{1},{2}].", stoneColor.ToString(), x, y));
            }
            cells[x, y] = stoneColor;
            return TurnOverStoneIfPossible(x, y);
        }

        bool IsStonePuttable(int x, int y, Game_Field.StoneColor stoneColor)
        {
            if (IsCorrectCoordinate(x, y) && cells[x, y] == Game_Field.StoneColor.None)
            {
                return ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(x, y, stoneColor, -1, -1) ||
                ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(x, y, stoneColor, -1, 0) ||
                ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(x, y, stoneColor, -1, 1) ||
                ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(x, y, stoneColor, 0, -1) ||
                ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(x, y, stoneColor, 0, 1) ||
                ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(x, y, stoneColor, 1, -1) ||
                ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(x, y, stoneColor, 1, 0) ||
                ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(x, y, stoneColor, 1, 1);
            }
            else
            {
                return false;
            }
        }

        bool ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(int x, int y, Game_Field.StoneColor stoneColor, int xDirection, int yDirection)
        {
            var existsEnemyStone = false;
            while (true)
            {
                x += xDirection;
                y += yDirection;
                if (!IsCorrectCoordinate(x, y) || cells[x, y] == Game_Field.StoneColor.None)
                {
                    return false;
                }
                else if (cells[x, y] == stoneColor)
                {
                    return existsEnemyStone;
                }
                else
                {
                    existsEnemyStone = true;
                }
            }
        }

        List<CellInfo> TurnOverStoneIfPossible(int x, int y)
        {
            var infos = new List<CellInfo>();
            // 8方向それぞれに対し、ひっくり返す処理を実行
            infos.AddRange(TurnStoneForDirectionIfPossible(x, y, -1, -1));
            infos.AddRange(TurnStoneForDirectionIfPossible(x, y, -1, 0));
            infos.AddRange(TurnStoneForDirectionIfPossible(x, y, -1, 1));
            infos.AddRange(TurnStoneForDirectionIfPossible(x, y, 0, -1));
            infos.AddRange(TurnStoneForDirectionIfPossible(x, y, 0, 1));
            infos.AddRange(TurnStoneForDirectionIfPossible(x, y, 1, -1));
            infos.AddRange(TurnStoneForDirectionIfPossible(x, y, 1, 0));
            infos.AddRange(TurnStoneForDirectionIfPossible(x, y, 1, 1));
            return infos;
        }

        List<CellInfo> TurnStoneForDirectionIfPossible(int x, int y, int xDirection, int yDirection)
        {
            var infos = new List<CellInfo>();
            if (!ExistsOwnStoneAtTheOtherSideOfEnemyStoneForDirection(x, y, cells[x, y], xDirection, yDirection))
            {
                return infos;
            }

            var targetStoneColor = cells[x, y];

            while (true)
            {
                x += xDirection;
                y += yDirection;
                if (!IsCorrectCoordinate(x, y) || cells[x, y] == targetStoneColor)
                {
                    return infos;
                }
                else
                {
                    cells[x, y] = targetStoneColor;
                    infos.Add(new CellInfo(x, y, targetStoneColor));
                }
            }
        }

        bool IsCorrectCoordinate(int x, int y)
        {
            return 0 <= x && x < Game_Field.SIZE_X && 0 <= y && y < Game_Field.SIZE_Y;
        }
    }
}
