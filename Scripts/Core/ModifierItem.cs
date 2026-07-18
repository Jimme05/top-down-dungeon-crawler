using Godot;
using System;

public partial class ModifierItem : Area2D
{
    // ชนิดของหินที่ดรอป (เดี๋ยวเราจะสุ่มตอนที่ศัตรูตาย)
    public ModifierType Type = ModifierType.Fire; 

    public override void _Ready()
    {
        // เมื่อมีใครมาเดินชนไอเทม ให้เรียกฟังก์ชัน OnBodyEntered
        BodyEntered += OnBodyEntered;
        
        // เปลี่ยนสีไอเทมตามประเภทของหิน
        if (Type == ModifierType.Fire)
            Modulate = new Color(1.0f, 0.5f, 0.0f); // หินไฟสีส้ม
        else if (Type == ModifierType.MultiShot)
            Modulate = new Color(0.0f, 0.5f, 1.0f); // หินกระจายสีฟ้า
    }

    private void OnBodyEntered(Node2D body)
    {
        // ถ้าคนที่มาเก็บคือ Player
        if (body is Player player)
        {
            player.EquipModifier(Type); // สั่งให้ผู้เล่นสวมใส่สกิล
            QueueFree(); // ไอเทมหายไปจากพื้น
        }
    }
}
