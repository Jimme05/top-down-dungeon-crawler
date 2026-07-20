using Godot;
using System;

public enum EnemyType { Normal, Runner, Tank, Boss }
public enum EnemyState { Idle, Chase, Attack, Skill }

public partial class Enemy : CharacterBody2D
{
    [Export] public EnemyType Type = EnemyType.Normal;
    [Export] public float Speed = 100.0f;
    
    [Export] public PackedScene ModifierItemScene;
    [Export] public PackedScene CoinScene;
    [Export] public PackedScene SoulScene;

    public int Hp = 3;
    private Node2D player;
    private AnimatedSprite2D animSprite;
    private Sprite2D fallbackSprite;
    private ProgressBar hpBar;

    // State Machine Variables
    private EnemyState state = EnemyState.Chase;
    private float stateTimer = 0.0f;
    private float bossSkillCooldown = 3.0f;
    private Random random = new Random();

    // Fire status
    private bool isOnFire = false;
    private float fireTimer = 0.0f;
    
    private bool isCold = false;
    private float coldTimer = 0.0f;

    public override void _Ready()
    {
        player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        
        // ค้นหาโหนดแอนิเมชันที่ตั้งชื่อแยกไว้
        AnimatedSprite2D goblinAnim = GetNodeOrNull<AnimatedSprite2D>("GoblinAnim");
        AnimatedSprite2D bossAnim = GetNodeOrNull<AnimatedSprite2D>("BossAnim");
        
        // ค้นหาโหนด Collision ที่ตั้งชื่อแยกไว้
        CollisionShape2D goblinCol = GetNodeOrNull<CollisionShape2D>("GoblinCollision");
        CollisionShape2D bossCol = GetNodeOrNull<CollisionShape2D>("BossCollision");

        // ถ้าผู้เล่นยังใช้ชื่อเดิมอยู่ ให้ดึงมาใช้ก่อนกันเกมพัง
        if (goblinAnim == null && bossAnim == null)
        {
            animSprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        }

        fallbackSprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        hpBar = GetNodeOrNull<ProgressBar>("HPBar");

        Texture2D goblinTex = GD.Load<Texture2D>("res://Assets/Graphics/goblin.png");
        Texture2D bossTex = GD.Load<Texture2D>("res://Assets/Graphics/boss.png");

        if (Type == EnemyType.Boss)
        {
            Hp = 1000; Speed = 60.0f;
            if (fallbackSprite != null) fallbackSprite.Texture = bossTex;
            Scale = new Vector2(0.35f, 0.35f);
            bossSkillCooldown = 2.0f; // Boss starts attacking soon

            
            // เลือกใช้แอนิเมชันของบอส และซ่อนของก็อบลิน
            if (bossAnim != null) { animSprite = bossAnim; animSprite.Visible = true; }
            if (goblinAnim != null) goblinAnim.Visible = false;

            // เปิด Collision บอส ปิดของก็อบลิน
            if (bossCol != null) bossCol.Disabled = false;
            if (goblinCol != null) goblinCol.Disabled = true;
        }
        else // Normal, Runner, Tank
        {
            // เลือกใช้แอนิเมชันของก็อบลิน และซ่อนของบอส
            if (goblinAnim != null) { animSprite = goblinAnim; animSprite.Visible = true; }
            if (bossAnim != null) bossAnim.Visible = false;

            // เปิด Collision ก็อบลิน ปิดของบอส
            if (goblinCol != null) goblinCol.Disabled = false;
            if (bossCol != null) bossCol.Disabled = true;

            if (Type == EnemyType.Runner)
            {
                Speed = 200.0f; Hp = 1;
                if (fallbackSprite != null) fallbackSprite.Texture = goblinTex;
                Modulate = new Color(1.0f, 1.0f, 0.5f);
                Scale = new Vector2(0.08f, 0.08f);
            }
            else if (Type == EnemyType.Tank)
            {
                Speed = 50.0f; Hp = 10;
                if (fallbackSprite != null) fallbackSprite.Texture = goblinTex;
                Modulate = new Color(0.8f, 0.3f, 1.0f);
                Scale = new Vector2(0.2f, 0.2f);
            }
            else // Normal
            {
                if (fallbackSprite != null) fallbackSprite.Texture = goblinTex;
                Scale = new Vector2(0.12f, 0.12f);
            }
        }
        
        if (hpBar != null)
        {
            hpBar.MaxValue = Hp;
            hpBar.Value = Hp;
            
            // ตั้งค่าสีหลอดเลือดให้เป็นสีแดง
            StyleBoxFlat bg = new StyleBoxFlat();
            bg.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            StyleBoxFlat fill = new StyleBoxFlat();
            fill.BgColor = new Color(1.0f, 0.2f, 0.2f, 1.0f);
            
            hpBar.AddThemeStyleboxOverride("background", bg);
            hpBar.AddThemeStyleboxOverride("fill", fill);
            
            // ขยายขนาดหลอดเลือดให้สวนทางกับ Scale ของตัวละคร เพื่อให้มองเห็นได้
            if (Type == EnemyType.Boss)
            {
                hpBar.Scale = new Vector2(4.0f, 3.0f);
                hpBar.Position = new Vector2(-200f, -200f);
            }
            else if (Type == EnemyType.Runner)
            {
                hpBar.Scale = new Vector2(10.0f, 6.0f);
                hpBar.Position = new Vector2(-500f, -300f);
            }
            else if (Type == EnemyType.Tank)
            {
                hpBar.Scale = new Vector2(5.0f, 3.0f);
                hpBar.Position = new Vector2(-250f, -150f);
            }
            else // Normal
            {
                hpBar.Scale = new Vector2(8.0f, 5.0f);
                hpBar.Position = new Vector2(-400f, -250f);
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (player == null) return;

        // Fire logic
        if (isOnFire)
        {
            fireTimer -= (float)delta;
            if (fireTimer <= 0)
            {
                TakeDamage(1);
                fireTimer = 1.0f;
            }
        }

        // Cold logic
        if (isCold)
        {
            coldTimer -= (float)delta;
            if (coldTimer <= 0)
            {
                isCold = false;
                Speed *= 2.0f; // กลับเป็นความเร็วเดิม
                if (!isOnFire) Modulate = new Color(1.0f, 1.0f, 1.0f);
                else Modulate = new Color(1.0f, 0.4f, 0.0f);
            }
        }

        float distToPlayer = GlobalPosition.DistanceTo(player.GlobalPosition);
        Vector2 direction = (player.GlobalPosition - GlobalPosition).Normalized();

        // Flip Sprite
        if (direction.X != 0)
        {
            if (animSprite != null) animSprite.FlipH = direction.X < 0;
            if (fallbackSprite != null) fallbackSprite.FlipH = direction.X < 0;
        }

        // State Machine
        switch (state)
        {
            case EnemyState.Chase:
                if (animSprite != null) animSprite.Play("walk");
                
                Velocity = direction * Speed;
                bool collided = MoveAndSlide();
                
                if (collided)
                {
                    for (int i = 0; i < GetSlideCollisionCount(); i++)
                    {
                        var collision = GetSlideCollision(i);
                        if (collision.GetCollider() is Player p)
                        {
                            p.TakeDamage(1);
                            Velocity = -direction * 300; // Knockback
                            MoveAndSlide();
                            
                            // เปลี่ยนเป็นท่าโจมตี
                            ChangeState(EnemyState.Attack, 0.5f);
                        }
                    }
                }

                // Boss AI: สุ่มใช้สกิลเมื่อใกล้ถึงเวลา
                if (Type == EnemyType.Boss)
                {
                    bossSkillCooldown -= (float)delta;
                    if (bossSkillCooldown <= 0 && distToPlayer < 600) // ถ้าผู้เล่นอยู่ในระยะ
                    {
                        UseRandomBossSkill();
                    }
                }
                break;

            case EnemyState.Attack:
            case EnemyState.Skill:
                if (animSprite != null && state == EnemyState.Attack) animSprite.Play("attack");
                if (animSprite != null && state == EnemyState.Skill) animSprite.Play("skill");
                
                // หยุดเดินตอนโจมตีหรือใช้สกิล
                Velocity = Vector2.Zero; 
                stateTimer -= (float)delta;
                if (stateTimer <= 0)
                {
                    ChangeState(EnemyState.Chase, 0); // กลับไปเดินตาม
                }
                break;
        }
    }

    private void ChangeState(EnemyState newState, float duration)
    {
        state = newState;
        stateTimer = duration;
    }

    private void UseRandomBossSkill()
    {
        int skillChoice = random.Next(1, 4); // สุ่ม 1, 2, 3
        ChangeState(EnemyState.Skill, 1.5f); // ท่าไม้ตายใช้เวลาชาร์จ/ร่าย 1.5 วินาที
        bossSkillCooldown = 4.0f + (float)random.NextDouble() * 3.0f; // คูลดาวน์รอบต่อไป 4-7 วินาที
        
        PackedScene bulletScene = GD.Load<PackedScene>("res://Scenes/Enemies/enemy_bullet.tscn");
        
        if (skillChoice == 1) // Skill 1: Bullet Hell (ยิง 8 ทิศ)
        {
            GD.Print("Boss uses Skill 1: Bullet Hell!");
            for (int i = 0; i < 8; i++)
            {
                float angle = i * Mathf.Pi / 4.0f; // 45 องศา
                ShootBossBullet(bulletScene, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
            }
        }
        else if (skillChoice == 2) // Skill 2: Dash Attack
        {
            GD.Print("Boss uses Skill 2: Dash!");
            Vector2 dashDir = (player.GlobalPosition - GlobalPosition).Normalized();
            Velocity = dashDir * (Speed * 10); // พุ่งด้วยความเร็ว 10 เท่า!
            MoveAndSlide(); 
            // ลดเวลาชาร์จท่าลงเพราะพุ่งไปแล้ว
            stateTimer = 0.5f; 
        }
        else if (skillChoice == 3) // Skill 3: Straight Slash (คลื่นดาบ)
        {
            GD.Print("Boss uses Skill 3: Straight Slash!");
            Vector2 dir = (player.GlobalPosition - GlobalPosition).Normalized();
            animSprite.Play("attack");
            // ยิงคลื่นดาบรัวๆ 3 ลูกติดกันในทิศทางเดิม
            for(int i = 0; i < 3; i++)
            {
                // ใช้ CallDeferred ร่วมกับ Timer เพื่อหน่วงเวลายิง
                GetTree().CreateTimer(i * 0.2f).Timeout += () => 
                {
                    if (this != null && IsInsideTree())
                    {
                        ShootBossBullet(bulletScene, dir, true); // true = ยิงแบบคลื่นดาบ
                    }
                };
            }
        }
    }

    private void ShootBossBullet(PackedScene bulletScene, Vector2 dir, bool isSlash = false)
    {
        if (bulletScene == null) return;
        Node2D bulletNode = bulletScene.Instantiate<Node2D>();
        
        if (bulletNode is EnemyBullet bullet)
        {
            bullet.GlobalPosition = GlobalPosition;
            bullet.Direction = dir;
            
            if (isSlash)
            {
                bullet.Speed = 600.0f;
                // ปรับสเกลยืดออกให้ดูเหมือนเส้นคลื่นดาบ
                bullet.Scale = new Vector2(2.0f, 0.5f);
                bullet.Rotation = dir.Angle();
            }
            
            GetTree().CurrentScene.CallDeferred("add_child", bullet);
        }
    }

    public void ApplyFire()
    {
        if (!isOnFire)
        {
            isOnFire = true;
            fireTimer = 1.0f;
            Modulate = new Color(1.0f, 0.4f, 0.0f);
        }
    }

    public void ApplyCold()
    {
        if (!isCold)
        {
            isCold = true;
            coldTimer = 2.0f;
            Speed *= 0.5f; // ลดความเร็ว 50%
            Modulate = new Color(0.5f, 0.8f, 1.0f); // เปลี่ยนสีฟ้า
        }
        else
        {
            coldTimer = 2.0f; // รีเฟรชระยะเวลา
        }
    }

    public void TakeDamage(int damage)
    {
        Hp -= damage;
        if (hpBar != null) hpBar.Value = Hp;

        if (Hp <= 0)
        {
            Random rand = new Random();
            if (ModifierItemScene != null && rand.NextDouble() < 0.3f) 
            {
                ModifierItem item = ModifierItemScene.Instantiate<ModifierItem>();
                item.Type = (ModifierType)rand.Next(1, 9); // สุ่มหินสกิลทั้ง 8 แบบ
                item.GlobalPosition = GlobalPosition; 
                GetTree().CurrentScene.CallDeferred("add_child", item); 
            }
            
            if (CoinScene != null)
            {
                CoinItem coin = CoinScene.Instantiate<CoinItem>();
                coin.GlobalPosition = GlobalPosition + new Vector2(20, 0); 
                if (Type == EnemyType.Boss) coin.Value = 20;
                else if (Type == EnemyType.Tank) coin.Value = 5;
                else coin.Value = 1;
                GetTree().CurrentScene.CallDeferred("add_child", coin);
            }
            
            if (SoulScene != null)
            {
                Node2D soul = SoulScene.Instantiate<Node2D>();
                soul.GlobalPosition = GlobalPosition + new Vector2(-20, 0);
                GetTree().CurrentScene.CallDeferred("add_child", soul);
            }

            QueueFree(); 
        }
    }
}
