using Godot;
using System;

public enum PlayerClass
{
    Archer,
    Mage
}

public partial class Player : CharacterBody2D
{
    [Export]
    public float Speed = 200.0f;

    [Export]
    public PackedScene BulletScene; 

    // --- ระบบอาชีพ (Class System) ---
    [Export] 
    public PlayerClass CurrentClass = PlayerClass.Archer;
    // -------------------------

    // --- ระบบ Skill Combiner ---
    [Export] public ModifierType SkillSlot1 = ModifierType.None;
    [Export] public ModifierType SkillSlot2 = ModifierType.None;
    [Export] public ModifierType SkillSlot3 = ModifierType.None;
    
    // --- ระบบทรัพยากรในด่าน (รีเซ็ตเมื่อตาย) ---
    public int Souls = 0;
    public int BonusDamage = 0; 
    
    // --- ระบบรัศมีการโจมตี ---
    [Export]
    public float AttackRange = 300.0f; // ศัตรูต้องเข้ามาใกล้กว่า 300 ถึงจะยิง

    // ระบบจับเวลา (Cooldown) สำหรับการยิง
    public float FireCooldown = 0.5f; 
    private float currentCooldown = 0.0f;

    // --- ระบบเลือดและการโดนโจมตี ---
    public int Hp = 5;
    private float invincibilityTimer = 0.0f; 

    // --- ระบบ Dash (พุ่งหลบ) ---
    public float DashSpeed = 600.0f; 
    public float DashDuration = 0.2f; 
    public float DashCooldown = 1.0f; 
    
    private bool isDashing = false;
    private float dashTimeLeft = 0.0f;
    private float currentDashCooldown = 0.0f;
    private Vector2 lastDirection = Vector2.Right; 

    // --- ระบบ Ultimate Skill (คลิกขวา) ---
    public float UltimateCooldown = 5.0f; 
    private float currentUltimateCooldown = 0.0f;
    // ----------------------------

    // ตัวแปรกันการกดปุ่ม U รัวๆ
    private bool uKeyPressed = false;

    // --- ระบบ Animation ---
    private AnimatedSprite2D animSprite;
    private float attackAnimTimer = 0.0f; // เอาไว้ค้างท่าโจมตีไว้แป๊บนึง

    public override void _Ready()
    {
        // ค้นหา AnimatedSprite2D
        animSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        GD.Print("Player Script ทำงานแล้ว! อาชีพปัจจุบัน: ", CurrentClass);
        
        // เซ็ตความเร็วในการยิงตามอาชีพ
        if (CurrentClass == PlayerClass.Archer) FireCooldown = 0.5f; 
        else if (CurrentClass == PlayerClass.Mage) FireCooldown = 1.5f; 

        // เพิ่มโบนัสเลือดถาวรจากการซื้ออัปเกรด
        Hp += GlobalData.PermanentBonusHp;
        GD.Print($"เลือดเริ่มต้นรอบนี้: {Hp} (โบนัสถาวร +{GlobalData.PermanentBonusHp})");
    }

    public override void _PhysicsProcess(double delta)
    {
        // 1. ระบบจัดการคูลดาวน์พุ่งหลบ
        if (currentDashCooldown > 0) currentDashCooldown -= (float)delta;
        
        // รับปุ่ม Spacebar ("ui_accept") เพื่อสั่ง Dash
        if (Input.IsActionJustPressed("ui_accept") && currentDashCooldown <= 0 && !isDashing)
        {
            isDashing = true;
            dashTimeLeft = DashDuration;
            currentDashCooldown = DashCooldown;
            invincibilityTimer = DashDuration; // เป็นอมตะระหว่างพุ่ง
            Modulate = new Color(0.5f, 0.5f, 1.0f); // เปลี่ยนตัวเป็นสีฟ้าตอนพุ่ง
            GD.Print("DASH!");
        }

        // 2. ระบบเดินและพุ่ง (ฟิสิกส์)
        if (isDashing)
        {
            // ถ้าอยู่ในสถานะพุ่ง ให้ใช้ความเร็ว DashSpeed
            Velocity = lastDirection * DashSpeed;
            dashTimeLeft -= (float)delta;
            if (dashTimeLeft <= 0)
            {
                isDashing = false;
                Modulate = new Color(1.0f, 1.0f, 1.0f); // กลับเป็นสีเดิมเมื่อพุ่งเสร็จ
            }
        }
        else
        {
            // ระบบเดินปกติ
            Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
            if (direction != Vector2.Zero)
            {
                lastDirection = direction; // จำทิศล่าสุดเอาไว้เสมอ
            }
            Velocity = direction * Speed;
        }
        
        MoveAndSlide();

        // --- ระบบอัปเดตแอนิเมชัน (แยกตามคลาส) ---
        if (animSprite != null)
        {
            string prefix = (CurrentClass == PlayerClass.Archer) ? "archer_" : "mage_";

            // หันหน้าซ้ายขวา
            if (lastDirection.X < 0) animSprite.FlipH = true;
            else if (lastDirection.X > 0) animSprite.FlipH = false;

            // เช็คว่ากำลังเล่นท่าโจมตีอยู่หรือเปล่า
            if (attackAnimTimer > 0)
            {
                attackAnimTimer -= (float)delta;
                animSprite.Play(prefix + "attack");
            }
            else
            {
                // ถ้าไม่ได้โจมตี ให้เล่นท่ายืน หรือ เดิน
                if (Velocity.Length() > 0 || isDashing)
                    animSprite.Play(prefix + "walk");
                else
                    animSprite.Play(prefix + "idle");
            }
        }
        // ------------------------------------

        // 2. ระบบนับเวลายิง (Auto-cast)
        currentCooldown -= (float)delta;
        if (currentCooldown <= 0)
        {
            ShootNearestEnemy();
            currentCooldown = FireCooldown;
        }

        // 4. ระบบนับเวลาอมตะ (I-frames)
        if (invincibilityTimer > 0)
        {
            invincibilityTimer -= (float)delta;
            // กลับเป็นสีปกติเมื่อหมดอมตะ (แต่ถ้ากำลังพุ่งอยู่ให้ข้ามไปก่อน)
            if (invincibilityTimer <= 0 && !isDashing)
            {
                Modulate = new Color(1.0f, 1.0f, 1.0f); 
            }
        }

        // 5. ระบบท่าไม้ตาย (Ultimate Skill)
        if (currentUltimateCooldown > 0)
        {
            currentUltimateCooldown -= (float)delta;
        }
        else if (Input.IsMouseButtonPressed(MouseButton.Right)) 
        {
            CastUltimate(); 
            currentUltimateCooldown = UltimateCooldown; 
        }

        // 6. ระบบซื้ออัปเกรดถาวร (กดปุ่ม U)
        if (Input.IsPhysicalKeyPressed(Key.U))
        {
            if (!uKeyPressed)
            {
                uKeyPressed = true;
                if (GlobalData.Coins >= 10)
                {
                    GlobalData.Coins -= 10;
                    GlobalData.PermanentBonusHp += 1;
                    Hp += 1; // อัปเดตเลือด ณ ปัจจุบันด้วย
                    GD.Print($"ซื้ออัปเกรดสำเร็จ! (เหลือ {GlobalData.Coins} เหรียญ) -> เลือดตั้งต้นเพิ่มเป็น +{GlobalData.PermanentBonusHp} ตลอดกาล!");
                }
                else
                {
                    GD.Print($"เงินไม่พอซื้ออัปเกรด! ต้องการ 10 เหรียญ (คุณมี {GlobalData.Coins} เหรียญ)");
                }
            }
        }
        else
        {
            uKeyPressed = false; // รีเซ็ตการกดปุ่ม
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

    // ฟังก์ชันใช้งานท่าไม้ตายตามอาชีพ (Ultimate Skill)
    private void CastUltimate()
    {
        GD.Print($"ใช้งานท่าไม้ตายของ {CurrentClass}!!");
        
        // สั่งเล่นแอนิเมชันโจมตีค้างไว้ 0.5 วินาทีตอนใช้ท่าไม้ตาย
        attackAnimTimer = 0.5f;

        if (CurrentClass == PlayerClass.Archer)
        {
            // ท่าไม้ตาย Archer: สาดกระสุน 8 ทิศทางรอบตัว (Nova Burst)
            for (int i = 0; i < 8; i++)
            {
                float angle = i * (MathF.PI / 4.0f); // หมุนทีละ 45 องศา
                Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                SpawnBullet(dir);
            }
        }
        else if (CurrentClass == PlayerClass.Mage)
        {
            // ท่าไม้ตาย Mage: ยิงกระสุนเวทมนตร์ลูกยักษ์สาดออกไป 3 แฉกข้างหน้าอย่างรุนแรง
            Vector2 baseDir = lastDirection;
            SpawnBullet(baseDir.Rotated(-0.5f)); // ซ้าย
            SpawnBullet(baseDir);                // กลาง
            SpawnBullet(baseDir.Rotated(0.5f));  // ขวา
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

    // ฟังก์ชันเรดาร์: หาศัตรูที่อยู่ใกล้ที่สุดภายในระยะ (AttackRange)
    private Node2D FindNearestEnemy()
    {
        var enemies = GetTree().GetNodesInGroup("enemies");
        Node2D nearest = null;
        float minDistance = float.MaxValue;

        foreach (Node2D enemy in enemies)
        {
            float distance = GlobalPosition.DistanceTo(enemy.GlobalPosition);
            // เช็คว่าระยะใกล้กว่าตัวก่อนหน้า และ อยู่ในรัศมีการโจมตี
            if (distance < minDistance && distance <= AttackRange)
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
            // สั่งเล่นแอนิเมชันโจมตี
            attackAnimTimer = 0.2f; 
            
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
        
        // --- ปรับแต่งกระสุนตามอาชีพ ---
        if (CurrentClass == PlayerClass.Mage)
        {
            bullet.Scale = new Vector2(2.5f, 2.5f);
            bullet.Damage = 3 + BonusDamage; // รวมพลังจากแท่นบูชา
            bullet.Speed = 200.0f;
            bullet.Modulate = new Color(0.0f, 1.0f, 1.0f); 
        }
        else if (CurrentClass == PlayerClass.Archer)
        {
            bullet.Damage = 1 + BonusDamage; // รวมพลังจากแท่นบูชา
        }
        // ------------------------------
        
        GetTree().CurrentScene.AddChild(bullet);
    }
}
