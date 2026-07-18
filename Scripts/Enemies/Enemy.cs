using Godot;
using System;

public partial class Enemy : CharacterBody2D
{
    [Export]
    public float Speed = 100.0f; // ศัตรูควรจะเดินช้ากว่าผู้เล่นนิดนึง

    public int Hp = 3; // เลือดของศัตรู

    private Node2D player; // ตัวแปรสำหรับเก็บเป้าหมาย (ผู้เล่น)

    public override void _Ready()
    {
        // ใช้ Group ที่เราเพิ่งสร้าง เพื่อหาว่าผู้เล่นอยู่ไหน!
        player = GetTree().GetFirstNodeInGroup("player") as Node2D;
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
                GetTree().CurrentScene.CallDeferred("add_child", item); // ใช้ CallDeferred เพื่อความปลอดภัยตอนศัตรูตาย
                GD.Print("เย้! ศัตรูดรอปหินพลัง!");
            }

            QueueFree(); // ลบโหนดศัตรูทิ้ง
        }
    }
}
