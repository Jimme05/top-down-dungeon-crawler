using Godot;
using System;

public partial class EnemySpawner : Node2D
{
    // ช่องสำหรับลากไฟล์ Enemy.tscn มาใส่
    [Export]
    public PackedScene EnemyScene;
    
    // ตั้งเวลาให้ศัตรูเกิดทุกๆ 3 วินาที
    [Export]
    public float SpawnInterval = 3.0f; 
    
    private float timer = 0.0f;
    private Random random = new Random(); // ตัวช่วยสุ่มตัวเลข

    public override void _PhysicsProcess(double delta)
    {
        if (EnemyScene == null) return; // ถ้ายังไม่ได้ใส่ไฟล์ศัตรู ให้หยุดทำงาน
        
        timer -= (float)delta;
        if (timer <= 0)
        {
            SpawnEnemy();
            timer = SpawnInterval; // รีเซ็ตเวลา
        }
    }

    private void SpawnEnemy()
    {
        // 1. เสกศัตรูขึ้นมา
        Node2D enemy = EnemyScene.Instantiate<Node2D>();
        
        // 2. สุ่มตำแหน่งเกิด (ให้อยู่ภายในขอบเขตหน้าจอ)
        // หน้าจอพื้นฐานของ Godot คือ กว้าง 1152, สูง 648
        float randomX = (float)random.NextDouble() * 1152;
        float randomY = (float)random.NextDouble() * 648;
        
        enemy.GlobalPosition = new Vector2(randomX, randomY);
        
        // 3. เอาศัตรูไปปล่อยในฉาก
        AddChild(enemy);
    }
}
