using Godot;
using System;

public partial class Enemy : AnimatedSprite2D
{
    [Export]
    public int MoveSpeed = 100; // The speed at which the enemy moves
    [Export]
    public int AttackRange = 1; // The attack range of the enemy
    public Vector2 TargetPosition { get; set; }
    private Player player;
    private int PlayerPathTarget = 1;
    private AstarPath AstarPathFind;
    private TileMap WorldTileMap;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        WorldTileMap = GetParent().GetNode<TileMap>("TileMap");
        AstarPathFind = GetNode<AstarPath>("AstarPath");
        AstarPathFind.SetTileMap(WorldTileMap);

    }
    // Setter method to set the player reference from the Main scene
    public void SetPlayer(Player player)
    {
        this.player = player;
    }
    public void PerformTurn(Main gameManager)
    {
        GD.Print(Name + " is performing a turn.");
        TargetPosition = player.GlobalPosition;
        GD.Print(Name + " target position: " + TargetPosition);

        MoveTowardsTarget(gameManager);

        if (IsPlayerInRange())
        {
            AttackPlayer();
        }

        gameManager.OnEnemyTurnFinished();
    }
    private bool IsPlayerInRange()
    {
        float distanceToPlayer = Position.DistanceTo(player.GlobalPosition);
        return distanceToPlayer <= AttackRange * WorldTileMap.TileSet.TileSize.X; // Assuming square grid
    }
    private void MoveTowardsTarget(Main gameManager)
    {
        // First, calculate the path towards the target position
        if (AstarPathFind.SetAstarPath(GlobalPosition, TargetPosition))
        {
            GD.Print(Name + " calculated path with " + AstarPathFind.PathNodeList.Count + " points.");

            // If there are points in the path and the enemy can still move
            if (PlayerPathTarget < AstarPathFind.PathNodeList.Count)
            {
                Vector2 targetNode = WorldTileMap.MapToLocal((Vector2I)AstarPathFind.PathNodeList[PlayerPathTarget]);
                Position = Position.MoveToward(targetNode, (float)(MoveSpeed * gameManager.GetProcessDeltaTime()));

                // Check if the enemy has reached the current target node
                if (Position == targetNode)
                {
                    PlayerPathTarget++;
                    GD.Print(Name + " moved to node: " + targetNode);
                }
            }
            else
            {
                // The enemy has reached the end of the path
                GD.Print(Name + " reached the end of the path.");
                gameManager.OnEnemyTurnFinished();
            }
        }
        else
        {
            // Path calculation failed
            GD.Print(Name + " failed to calculate a path.");
            gameManager.OnEnemyTurnFinished();
        }
    }


    private void AttackPlayer()
    {
        // Perform attack logic here
        GD.Print("Enemy attacks the player!");
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
	}
}
