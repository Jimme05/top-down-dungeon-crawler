using Godot;
using System;

public partial class Bullet : Area2D
{
    public float Speed = 400.0f; // ความเร็วกระสุน
    public Vector2 Direction = Vector2.Right; // ทิศทางเริ่มต้น
    public int Damage = 1; // ความแรงของกระสุน

    // ตัวแปรสำหรับรับของขลัง (Modifiers) มาจากผู้เล่น
    public ModifierType[] Modifiers;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        Position += Direction * Speed * (float)delta;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Enemy enemy)
        {
            enemy.TakeDamage(Damage); // ดาเมจพื้นฐาน

            // เช็คว่ากระสุนนี้มีพลังแห่งไฟหรือไม่?
            if (Modifiers != null)
            {
                foreach (var mod in Modifiers)
                {
                    if (mod == ModifierType.Fire)
                    {
                        enemy.ApplyFire(); // สั่งให้ศัตรูติดไฟ!
                        break;
                    }
                }
            }

            QueueFree(); 
        }
    }
}
