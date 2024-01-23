using Godot;
using System;
using System.Collections.Generic;

public partial class Player : AnimatedSprite2D
{
    private int PlayerPathTarget = 1;
    private TileMap WorldTileMap;
    private AstarPath AstarPathFind;
    public bool isMoving = false;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        WorldTileMap = GetParent().GetNode<TileMap>("TileMap");
        AstarPathFind = GetNode<AstarPath>("AstarPath");
        AstarPathFind.SetTileMap(WorldTileMap);
    }

    public void PlayerMove()
    {
        isMoving = true;
        Vector2I clickedTile = WorldTileMap.LocalToMap(GetGlobalMousePosition());

        // Update non-walkable tiles with the positions of enemies
        var occupiedPositions = FindOccupiedPositions();
        AstarPathFind.UpdateNonWalkableTiles(occupiedPositions);

        if (!AstarPathFind.NonWalkableTiles.Contains(clickedTile))
        {
            if (AstarPathFind.SetAstarPath(GlobalPosition, GetGlobalMousePosition()))
            {
                PlayerPathTarget = 1;
            }
        }
        else
        {
            clickedTile = FindNearestUnoccupiedTile(clickedTile); // Adjust the target tile
            if (AstarPathFind.SetAstarPath(GlobalPosition, WorldTileMap.MapToLocal(clickedTile)))
            {
                PlayerPathTarget = 1;
            }
        }
    }

    private void WalkPath(double delta)
    {
        if (PlayerPathTarget < AstarPathFind.PathNodeList.Count)
        {
            Vector2 targetNode = WorldTileMap.MapToLocal((Vector2I)AstarPathFind.PathNodeList[PlayerPathTarget]);
            Position = Position.MoveToward(targetNode, (float)(150 * delta));
            if (Position == targetNode)
            {
                PlayerPathTarget++;
            }
            //Play("Walk");
        }
        else
        {
            isMoving = false;
            if (GetParent() is Main main)
            {
                main.EnemyTurnStart(); // Call this only when the player has reached the target
            }
            //Play("Idle");
        }
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (isMoving)
        {
            WalkPath(delta);
        }
    }
    private Vector2I FindNearestUnoccupiedTile(Vector2I targetTile)
    {
        Vector2I mapSize = WorldTileMap.GetUsedRect().Size;
        int maxRange = Math.Max(mapSize.X, mapSize.Y);

        // Check in a spiral or concentric squares around the targetTile
        for (int range = 1; range <= maxRange; range++)
        {
            foreach (var tile in GetTilesInRange(targetTile, range))
            {
                if (!AstarPathFind.NonWalkableTiles.Contains(tile) && !IsTileOutsideTheMap(tile))
                {
                    return tile; // Return the first unoccupied tile found
                }
            }
        }

        return targetTile; // Return the original target if no unoccupied tile is found
    }
    private IEnumerable<Vector2I> GetTilesInRange(Vector2I center, int range)
    {
        List<Vector2I> tiles = new List<Vector2I>();

        for (int y = -range; y <= range; y++)
        {
            for (int x = -range; x <= range; x++)
            {
                var tile = new Vector2I(center.X + x, center.Y + y);
                if ((x == -range || x == range || y == -range || y == range) && !IsTileOutsideTheMap(tile))
                {
                    tiles.Add(tile);
                }
            }
        }

        return tiles;
    }
    private bool IsTileOutsideTheMap(Vector2I tile)
    {
        Rect2I usedRect = WorldTileMap.GetUsedRect();
        return tile.X < usedRect.Position.X || tile.Y < usedRect.Position.Y
            || tile.X >= usedRect.Position.X + usedRect.Size.X
            || tile.Y >= usedRect.Position.Y + usedRect.Size.Y;
    }
    private IEnumerable<Vector2> FindOccupiedPositions()
    {
        List<Vector2> occupiedPositions = new List<Vector2>();
        if (GetParent() is Main main)
        {
            foreach (var enemy in main.enemies)
            {
                occupiedPositions.Add(enemy.GlobalPosition);
            }
        }
        return occupiedPositions;
    }
}
