using Godot;
using System;

public partial class CoinItem : Area2D
{
    // มูลค่าของเหรียญ (ตัว Tank จะดรอปเหรียญที่มีมูลค่า 5)
    public int Value = 1;

    private Node2D player;
    public float MagneticRange = 200.0f; // ระยะดูดไอเทม
    public float MoveSpeed = 500.0f; // ความเร็วตอนไอเทมลอยเข้าหาตัว
    private bool isCollected = false; // ป้องกันเก็บซ้ำ

    public override void _Ready()
    {
        // เมื่อมีใครมาเดินชนเหรียญ ให้เรียกฟังก์ชัน OnBodyEntered
        BodyEntered += OnBodyEntered;
        player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        
        // ทำให้เหรียญเป็นสีเหลืองทอง และกระพริบเบาๆ (ถ้าอยากให้สวยขึ้น)
        Modulate = new Color(1.0f, 0.8f, 0.0f); // สีเหลืองทอง
        
        // ถ้ามีมูลค่ามากกว่า 1 ให้ขนาดใหญ่ขึ้น
        if (Value > 1)
        {
            Scale = new Vector2(1.5f, 1.5f);
        }

        Label nameLabel = new Label();
        nameLabel.Text = Value > 1 ? $"Coin ({Value})" : "Coin";
        nameLabel.Position = new Vector2(-40, -40);
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        AddChild(nameLabel);
    }

    private void OnBodyEntered(Node2D body)
    {
        // ถ้าคนที่มาเก็บคือ Player และยังไม่ได้เก็บ
        if (body is Player && !isCollected)
        {
            isCollected = true;
            GlobalData.Coins += Value; // เพิ่มเงิน
            GD.Print($"เก็บเหรียญได้ {Value}! (เหรียญรวม: {GlobalData.Coins})");
            QueueFree(); // เหรียญหายไปจากพื้น
        }
    }

}
