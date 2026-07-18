using Godot;
using System;

public enum EnemyType
{
    Normal,
    Runner,
    Tank,
    Boss
}

public partial class Enemy : CharacterBody2D
{
    [Export]
    public EnemyType Type = EnemyType.Normal;

    [Export]
    public float Speed = 100.0f; // ศัตรูควรจะเดินช้ากว่าผู้เล่นนิดนึง

    public int Hp = 3; // เลือดของศัตรู

    private Node2D player; // ตัวแปรสำหรับเก็บเป้าหมาย (ผู้เล่น)

    public override void _Ready()
    {
        // ใช้ Group ที่เราเพิ่งสร้าง เพื่อหาว่าผู้เล่นอยู่ไหน!
        player = GetTree().GetFirstNodeInGroup("player") as Node2D;

        // ตั้งค่าสเตตัสตามประเภทของศัตรู
        if (Type == EnemyType.Runner)
        {
            Speed = 200.0f;
            Hp = 1;
            Modulate = new Color(1.0f, 1.0f, 0.0f); // สีเหลือง
            Scale = new Vector2(0.7f, 0.7f); // ตัวเล็กลง
        }
        else if (Type == EnemyType.Tank)
        {
            Speed = 50.0f;
            Hp = 10;
            Modulate = new Color(0.8f, 0.0f, 1.0f); // สีม่วง
            Scale = new Vector2(1.5f, 1.5f); // ตัวใหญ่ขึ้น
        }
        else if (Type == EnemyType.Boss)
        {
            Hp = 50; // เลือดบอสมหาศาล
            Speed = 60.0f; // เดินไวกว่า Tank นิดหน่อย
            Modulate = new Color(0.8f, 0.1f, 0.1f); // สีแดงเข้ม น่ากลัว
            Scale = new Vector2(3.0f, 3.0f); // ตัวใหญ่ยักษ์
        }
    }

    private bool isOnFire = false;
    private float fireTimer = 0.0f;

    public override void _PhysicsProcess(double delta)
    {
        // 1. วิ่งไล่ล่าผู้เล่น
        if (player != null)
        {
            Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();
            Velocity = direction * Speed;
            
            // MoveAndSlide จะคืนค่าเป็น true ถ้ามีการชนเกิดขึ้น
            bool collided = MoveAndSlide(); 
            
            if (collided)
            {
                // วนลูปเช็คว่าชนกับอะไรบ้าง
                for (int i = 0; i < GetSlideCollisionCount(); i++)
                {
                    KinematicCollision2D collision = GetSlideCollision(i);
                    Node collider = collision.GetCollider() as Node;
                    
                    // ถ้าสิ่งที่ชนคือ Player ให้สั่งทำดาเมจ
                    if (collider is Player p)
                    {
                        p.TakeDamage(1);
                        
                        // เด้งศัตรูถอยหลังนิดหน่อยเพื่อไม่ให้เดินทะลุผู้เล่น
                        Velocity = -direction * 300;
                        MoveAndSlide(); 
                    }
                }
            }
        }

        // 2. ถ้าตัวติดไฟอยู่ ให้ลดเลือดเรื่อยๆ
        if (isOnFire)
        {
            fireTimer -= (float)delta;
            if (fireTimer <= 0)
            {
                TakeDamage(1); // โดนเผา 1 ดาเมจ
                fireTimer = 1.0f; // เผาทุกๆ 1 วินาที
            }
        }
    }

    // ฟังก์ชันสั่งให้ศัตรูติดไฟ
    public void ApplyFire()
    {
        if (!isOnFire)
        {
            isOnFire = true;
            fireTimer = 1.0f;
            
            // เปลี่ยนสีตัวละครให้กลายเป็นสีส้มแดง (เป็นการ Feedback ให้ผู้เล่นเห็นภาพ)
            Modulate = new Color(1.0f, 0.4f, 0.0f);
        }
    }

    // ช่องสำหรับใส่ฉากไอเทมหินสกิล
    [Export]
    public PackedScene ModifierItemScene;
    
    // ช่องสำหรับใส่ฉากเหรียญ (เข้าสู่ระบบข้ามฉาก)
    [Export]
    public PackedScene CoinScene;
    
    // ช่องสำหรับใส่ฉากวิญญาณ (เอาไว้บูชาแท่น)
    [Export]
    public PackedScene SoulScene;

    // ฟังก์ชันสำหรับรับดาเมจ (จะถูกเรียกโดยกระสุน)
    public void TakeDamage(int damage)
    {
        Hp -= damage;
        GD.Print("ศัตรูโดนยิง! เลือดเหลือ: ", Hp);
        
        if (Hp <= 0)
        {
            GD.Print("ศัตรูตายแล้ว!");
            
            // สุ่มดรอปหินสกิล (โอกาส 30% ที่จะดรอป)
            Random random = new Random();
            if (ModifierItemScene != null && random.NextDouble() < 0.3f) 
            {
                Node2D item = ModifierItemScene.Instantiate<Node2D>();
                item.GlobalPosition = GlobalPosition; // ดรอปตรงที่ตาย
                GetTree().CurrentScene.CallDeferred("add_child", item); 
                GD.Print("เย้! ศัตรูดรอปหินพลัง!");
            }
            
            // ดรอปเหรียญลงพื้น
            if (CoinScene != null)
            {
                CoinItem coin = CoinScene.Instantiate<CoinItem>();
                coin.GlobalPosition = GlobalPosition + new Vector2(20, 0); 
                
                // ถ้ายิงบอสได้ ให้เหรียญมูลค่า 20! ถ้า Tank ให้ 5
                if (Type == EnemyType.Boss) coin.Value = 20;
                else if (Type == EnemyType.Tank) coin.Value = 5;
                else coin.Value = 1;
                
                GetTree().CurrentScene.CallDeferred("add_child", coin);
            }
            
            // ดรอปวิญญาณลงพื้น (ดรอปแน่นอน 100% ตัวละ 1 ดวง)
            if (SoulScene != null)
            {
                Node2D soul = SoulScene.Instantiate<Node2D>();
                soul.GlobalPosition = GlobalPosition + new Vector2(-20, 0); // ดรอปเยื้องไปอีกฝั่ง
                GetTree().CurrentScene.CallDeferred("add_child", soul);
            }

            QueueFree(); // ลบโหนดศัตรูทิ้ง
        }
    }
}
