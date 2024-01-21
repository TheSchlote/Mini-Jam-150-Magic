using Godot;
using System;

public partial class Player : AnimatedSprite2D
{
	private int PlayerPathTarget = 1;
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
            if (mbEvent.ButtonIndex == MouseButton.Left)
            {
                Vector2I clickedTile = WorldTileMap.LocalToMap(GetGlobalMousePosition());
                // Check if the clicked tile is non-walkable
                if (!AstarPathFind.NonWalkableTiles.Contains(clickedTile))
                {
                    if (AstarPathFind.SetAstarPath(GlobalPosition, GetGlobalMousePosition()))
                    {
                        PlayerPathTarget = 1;
                    }
                }
                else
                {
                    GD.Print("Clicked on a non-walkable tile.");
                }
            }
        }
    }


    private void WalkPath(double delta)
	{
		if(PlayerPathTarget < AstarPathFind.PathNodeList.Count)
		{
            Vector2 targetNode = WorldTileMap.MapToLocal((Vector2I)AstarPathFind.PathNodeList[PlayerPathTarget]);
			Position = Position.MoveToward(targetNode, (float)(150 * delta));
			if(Position == targetNode)
			{
				PlayerPathTarget++;
			}
			//Play("Walk");
		}
		else
		{
			//Play("Idle");
		}
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
		WalkPath(delta);
	}
}
