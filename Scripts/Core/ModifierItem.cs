using Godot;
using System;

public partial class ModifierItem : Area2D
{
    public ModifierType Type = ModifierType.Fire; 

    private Node2D player;
    private bool isCollected = false;
    private bool playerInRange = false;
    private Label promptLabel;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        
        // กำหนดสีตามประเภทหิน
        if (Type == ModifierType.Fire) Modulate = new Color(1.0f, 0.5f, 0.0f); // ส้มไฟ
        else if (Type == ModifierType.MultiShot) Modulate = new Color(0.0f, 0.5f, 1.0f); // ฟ้าคราม
        else if (Type == ModifierType.Pierce) Modulate = new Color(1.0f, 0.8f, 0.0f); // เหลืองทอง
        else if (Type == ModifierType.Chain) Modulate = new Color(0.6f, 0.2f, 1.0f); // ม่วง
        else if (Type == ModifierType.Cold) Modulate = new Color(0.6f, 0.9f, 1.0f); // ฟ้าอ่อน
        else if (Type == ModifierType.FasterAttacks) Modulate = new Color(1.0f, 0.2f, 0.4f); // ชมพูแดง
        else if (Type == ModifierType.Fork) Modulate = new Color(0.3f, 1.0f, 0.6f); // เขียวอ่อน
        else if (Type == ModifierType.Homing) Modulate = new Color(1.0f, 1.0f, 1.0f); // ขาวสว่าง

        // สร้างข้อความบอกชื่อไอเทม
        promptLabel = new Label();
        promptLabel.Text = GetItemName();
        promptLabel.Position = new Vector2(-40, -40); // ให้อยู่เหนือไอเทม
        promptLabel.Visible = true; // เปิดโชว์ไว้ตลอดเวลา
        promptLabel.HorizontalAlignment = HorizontalAlignment.Center;
        AddChild(promptLabel);
    }

    private string GetItemName()
    {
        if (Type == ModifierType.Fire) return "Fire Stone";
        if (Type == ModifierType.MultiShot) return "MultiShot Stone";
        if (Type == ModifierType.Pierce) return "Pierce Stone";
        if (Type == ModifierType.Chain) return "Chain Stone";
        if (Type == ModifierType.Cold) return "Cold Stone";
        if (Type == ModifierType.FasterAttacks) return "Fast Attacks";
        if (Type == ModifierType.Fork) return "Fork Stone";
        if (Type == ModifierType.Homing) return "Homing Stone";
        return "Skill Stone";
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            playerInRange = true;
            promptLabel.Text = GetItemName() + "\n[E] เก็บ"; // เพิ่มคำว่า E เข้าไป
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player)
        {
            playerInRange = false;
            promptLabel.Text = GetItemName(); // ซ่อนปุ่ม E
        }
    }

    public override void _Process(double delta)
    {
        // ตรวจสอบการกดปุ่ม E เมื่อผู้เล่นอยู่ในระยะ
        if (playerInRange && !isCollected)
        {
            if (Input.IsPhysicalKeyPressed(Key.E))
            {
                isCollected = true;
                ((Player)player).AddToInventory(Type); 
                QueueFree(); 
            }
        }
    }
}
