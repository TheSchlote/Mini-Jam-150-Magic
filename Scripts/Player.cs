using Godot;
using System;

public partial class Player : AnimatedSprite2D
{
	private int PlayerPathTarget = 1;
	private int MovesThisTurn = 0;
	private int MaxMovesPerTurn = 3;
	private int LongRange = 5;
	private int MidRange = 3;
	private int ShortRange = 1;
    private bool isTurnComplete = false;
	private TileMap WorldTileMap;
	private AstarPath AstarPathFind;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		WorldTileMap = GetParent().GetNode<TileMap>("TileMap");
		AstarPathFind = GetNode<AstarPath>("AstarPath");
		AstarPathFind.SetTileMap(WorldTileMap);
	}

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mbEvent && mbEvent.IsPressed())
        {
            if (mbEvent.ButtonIndex == MouseButton.Left && isTurnComplete)
            {
                StartNewTurn();
                Vector2I clickedTile = WorldTileMap.LocalToMap(GetGlobalMousePosition());
                // Check if the clicked tile is non-walkable
                if (!AstarPathFind.NonWalkableTiles.Contains(clickedTile))
                {
                    if (AstarPathFind.SetAstarPath(GlobalPosition, GetGlobalMousePosition()))
                    {
                        PlayerPathTarget = 1;
                    }
                }
            }
        }
    }
    private void WalkPath(double delta)
    {
        if(isTurnComplete)
        {
            return;
        }

        if (PlayerPathTarget < AstarPathFind.PathNodeList.Count && CanMove())
        {
            Vector2 targetNode = WorldTileMap.MapToLocal((Vector2I)AstarPathFind.PathNodeList[PlayerPathTarget]);
            Position = Position.MoveToward(targetNode, (float)(150 * delta));
            if (Position == targetNode)
            {
                PlayerPathTarget++;
                MovesThisTurn++;
                if (!CanMove())
                {
                    EndPlayerTurn();
                    return;
                }
            }
            //Play("Walk");
        }
        else if (CanMove())
        {
            EndPlayerTurn();
        }
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		WalkPath(delta);
	}
    private bool CanMove()
    {
        bool canMove = MovesThisTurn < MaxMovesPerTurn;
        return canMove;
    }
    private void PerformAttack(Vector2 targetPosition, int attackRange)
    {
        // Determine the type of attack based on the range and then execute it
        // You'll need to calculate the distance to the target and compare with attackRange
    }
    private void EndPlayerTurn()
    {
        isTurnComplete = true; // Set the flag to true as the turn is now complete
        PlayerPathTarget = 1; // Reset the path target for the next turn.
        MovesThisTurn = 0; // Reset move counter for the next turn.
        Main gameManager = GetParent() as Main; // Adjust the cast if your Main node is a different type
        gameManager.EndPlayerTurn();
    }
    public void StartNewTurn()
    {
        isTurnComplete = false; // Reset the turn completion flag
        // Reset any other necessary states for the new turn
    }
}
