using Godot;
using System;

public partial class EnemyBullet : Area2D
{
    public float Speed = 300.0f;
    public Vector2 Direction = Vector2.Right;
    public int Damage = 1;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        
        // กระสุนศัตรูจะหายไปเองใน 3 วินาที (กันรกฉาก)
        GetTree().CreateTimer(3.0f).Timeout += () => QueueFree();
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += Direction * Speed * (float)delta;
    }

    private void OnBodyEntered(Node2D body)
    {
        // ทำดาเมจเฉพาะกับผู้เล่น
        if (body is Player player)
        {
            player.TakeDamage(Damage);
            QueueFree(); 
        }
        else if (body.IsInGroup("walls") || body.Name.ToString().Contains("wall", StringComparison.OrdinalIgnoreCase))
        {
            QueueFree();
        }
    }
}
