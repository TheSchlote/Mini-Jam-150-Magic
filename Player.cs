using Godot;
using System;

public partial class Player : Node2D
{
    TileMap tileMap;
    AStarGrid2D aStarGrid;
    Godot.Collections.Array<Vector2I> currentIdPath;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        tileMap = GetTree().Root.GetNode<TileMap>("Node2D/TileMap");

        aStarGrid = new AStarGrid2D();
		aStarGrid.Region = tileMap.GetUsedRect();
        aStarGrid.CellSize = new Vector2(16, 16);
        aStarGrid.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;
		aStarGrid.Update();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("move") == false)
        {
            return;
        }

        Godot.Collections.Array<Vector2I> idPath = aStarGrid.GetIdPath(tileMap.LocalToMap(GlobalPosition), tileMap.LocalToMap(GetGlobalMousePosition())).Slice(1);

		if (idPath != null)
        {
            currentIdPath = idPath;

        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (currentIdPath == null || currentIdPath.Count == 0)
        {
            return;
        }

        Vector2 targetPosition = tileMap.MapToLocal(currentIdPath[0]);

        GlobalPosition = GlobalPosition.MoveToward(targetPosition, 1); 

        if (GlobalPosition.DistanceTo(targetPosition) < 1) 
        {
            currentIdPath.RemoveAt(0);
        }
    }

}
