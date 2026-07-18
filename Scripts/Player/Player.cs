using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export]
    public float Speed = 200.0f;

    // [Export] ตัวนี้จะทำให้เราสามารถเอาไฟล์ Bullet.tscn มาใส่ผ่านหน้าต่าง Inspector ได้
    [Export]
    public PackedScene BulletScene; 

    // --- ระบบ Skill Combiner ---
    // สร้างช่องใส่หิน Modifier 3 ช่อง (ใส่ [Export] เพื่อให้เลือกปรับจากหน้าต่าง Inspector ได้เลย)
    [Export] public ModifierType SkillSlot1 = ModifierType.None;
    [Export] public ModifierType SkillSlot2 = ModifierType.None;
    [Export] public ModifierType SkillSlot3 = ModifierType.None;
    // -------------------------

    // ระบบจับเวลา (Cooldown) สำหรับการยิง
    public float FireCooldown = 0.5f; // ยิงทุกๆ 0.5 วินาที
    private float currentCooldown = 0.0f;

    // --- ระบบเลือดและการโดนโจมตี ---
    public int Hp = 5;
    private float invincibilityTimer = 0.0f; // เวลานับถอยหลังการเป็นอมตะชั่วคราว (I-frames)
    // ----------------------------

    public override void _Ready()
    {
        GD.Print("Player Script ทำงานแล้ว! พร้อมรับคำสั่ง");
    }

    public override void _PhysicsProcess(double delta)
    {
        // 1. ระบบเดิน (โค้ดเดิมของเรา)
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Velocity = direction * Speed;
        MoveAndSlide();

        // 2. ระบบนับเวลายิง (Auto-cast)
        currentCooldown -= (float)delta;
        if (currentCooldown <= 0)
        {
            ShootNearestEnemy();
            currentCooldown = FireCooldown;
        }

        // 3. ระบบนับเวลาอมตะ (I-frames)
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= (float)delta;
            if (invincibilityTimer <= 0)
            {
                Modulate = new Color(1.0f, 1.0f, 1.0f); // กลับมาเป็นสีปกติ
            }
        }
    }

    // ฟังก์ชันรับดาเมจจากศัตรู
    public void TakeDamage(int damage)
    {
        if (invincibilityTimer > 0) return; // ถ้าเป็นอมตะอยู่ ให้ข้ามการรับดาเมจไปเลย

        Hp -= damage;
        GD.Print("ผู้เล่นโดนโจมตี! เลือดเหลือ: ", Hp);
        
        invincibilityTimer = 1.0f; // เป็นอมตะ 1 วินาทีหลังโดนตี
        Modulate = new Color(1.0f, 0.0f, 0.0f); // เปลี่ยนตัวละครเป็นสีแดงเพื่อบอกว่าเจ็บ!

        if (Hp <= 0)
        {
            GD.Print("GAME OVER!");
            GetTree().ReloadCurrentScene(); // สั่งรีสตาร์ทฉากนี้ใหม่ทั้งหมด
        }
    }

    // ฟังก์ชันสำหรับสวมใส่สกิล (เมื่อวิ่งไปเก็บหินที่ตกพื้น)
    public void EquipModifier(ModifierType newModifier)
    {
        // หาช่องที่ยังว่าง (None) แล้วยัดหินก้อนใหม่ใส่เข้าไป
        if (SkillSlot1 == ModifierType.None) SkillSlot1 = newModifier;
        else if (SkillSlot2 == ModifierType.None) SkillSlot2 = newModifier;
        else if (SkillSlot3 == ModifierType.None) SkillSlot3 = newModifier;
        else 
        {
            GD.Print("ช่องใส่สกิลเต็มแล้ว 3 ช่อง!");
            return;
        }
        
        GD.Print($"เก็บไอเทม: สวมใส่สกิล {newModifier} สำเร็จ!");
    }

    // ฟังก์ชันเรดาร์: หาศัตรูที่อยู่ใกล้ที่สุด
    private Node2D FindNearestEnemy()
    {
        // ดึงรายชื่อตัวละครทั้งหมดที่มีป้าย "enemies" 
        var enemies = GetTree().GetNodesInGroup("enemies");
        Node2D nearest = null;
        float minDistance = float.MaxValue;

        // วนลูปเช็คศัตรูทีละตัว เพื่อหาตัวที่ใกล้ที่สุด
        foreach (Node2D enemy in enemies)
        {
            float distance = GlobalPosition.DistanceTo(enemy.GlobalPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = enemy;
            }
        }
        return nearest;
    }

    // ฟังก์ชันยิงกระสุน
    private void ShootNearestEnemy()
    {
        Node2D target = FindNearestEnemy();
        
        if (target != null && BulletScene != null)
        {
            // คำนวณทิศทางหลักไปหาเป้าหมาย
            Vector2 baseDirection = (target.GlobalPosition - GlobalPosition).Normalized();
            
            // เช็คว่าผู้เล่นใส่หิน MultiShot ในช่องไหนสักช่องหรือไม่?
            bool hasMultiShot = (SkillSlot1 == ModifierType.MultiShot || 
                                 SkillSlot2 == ModifierType.MultiShot || 
                                 SkillSlot3 == ModifierType.MultiShot);

            if (hasMultiShot)
            {
                // ถ้ามี MultiShot ให้ยิง 3 นัด (บิดองศาไปซ้ายและขวานิดหน่อย)
                SpawnBullet(baseDirection.Rotated(-0.3f)); // ซ้าย
                SpawnBullet(baseDirection);                // กลาง
                SpawnBullet(baseDirection.Rotated(0.3f));  // ขวา
            }
            else
            {
                // ถ้าไม่มี ก็ยิง 1 นัดปกติ
                SpawnBullet(baseDirection);
            }
        }
        else
        {
            if (target == null) GD.Print("ยิงไม่ได้: หาศัตรูไม่เจอ!");
            if (BulletScene == null) GD.Print("ยิงไม่ได้: ยังไม่ได้ลากไฟล์ Bullet.tscn มาใส่!");
        }
    }

    // ฟังก์ชันช่วยเสกกระสุน (แยกออกมาเพื่อให้เรียกใช้ซ้ำได้ง่ายๆ)
    private void SpawnBullet(Vector2 direction)
    {
        Bullet bullet = BulletScene.Instantiate<Bullet>();
        bullet.GlobalPosition = GlobalPosition;
        bullet.Direction = direction;
        
        // ส่งต่อพลังหิน (Modifiers) จากตัวผู้เล่น ไปฝังไว้ในกระสุนด้วย
        bullet.Modifiers = new ModifierType[] { SkillSlot1, SkillSlot2, SkillSlot3 };
        
        GetTree().CurrentScene.AddChild(bullet);
    }
}
