using Godot;
using System;
using System.Collections.Generic;

public partial class Enemy : AnimatedSprite2D
{
    private int EnemyPathTarget = 1;
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

    public void EnemyMove(Vector2I playerPosition)
    {
        isMoving = true;

        // Update non-walkable tiles with the positions of player and other enemies
        var occupiedPositions = FindOccupiedPositions();
        AstarPathFind.UpdateNonWalkableTiles(occupiedPositions);

        if (AstarPathFind.SetAstarPath(GlobalPosition, playerPosition))
        {
            // Remove the last node to stop one tile before the player
            if (AstarPathFind.PathNodeList.Count > 1)
            {
                AstarPathFind.PathNodeList.RemoveAt(AstarPathFind.PathNodeList.Count - 1);
            }
            EnemyPathTarget = 1;
        }
    }


    private void WalkPath(double delta)
    {
        if (EnemyPathTarget < AstarPathFind.PathNodeList.Count)
        {
            Vector2 targetNode = WorldTileMap.MapToLocal((Vector2I)AstarPathFind.PathNodeList[EnemyPathTarget]);
            Position = Position.MoveToward(targetNode, (float)(150 * delta));
            if (Position == targetNode)
            {
                EnemyPathTarget++;
            }
            //Play("Walk");
        }
        else
        {
            isMoving = false;
            if (GetParent() is Main main)
            {
                main.NextEnemyTurn(); // Notify Main that this enemy finished moving
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
    private IEnumerable<Vector2> FindOccupiedPositions()
    {
        List<Vector2> occupiedPositions = new List<Vector2>();
        if (GetParent() is Main main)
        {
            occupiedPositions.Add(main.player.GlobalPosition);
        }
        return occupiedPositions;
    }
}
