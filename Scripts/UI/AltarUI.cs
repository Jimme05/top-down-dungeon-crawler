using Godot;
using System;

public partial class AltarUI : Control
{
    private Player player;
    private Altar currentAltar; // เพื่อเก็บใช้อ้างอิงว่าเปิดจากเสาต้นไหน

    private Button btn1;
    private Button btn2;
    private Button btn3;
    private Control panel;

    private string[] currentBuffs = new string[3];

    // --- รายชื่อบัฟ ---
    // Common (โอกาสออกบ่อย)
    private string[] commonBuffs = { "Heal 30%", "MaxHP +50", "Damage +1", "Speed +10%" };
    
    // Rare (โอกาสออกยากกว่า)
    private string[] rareBuffs = { "Full Heal", "MaxHP +150", "Damage +3", "Fire Rate +20%", "Random Skill" };

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore; // ป้องกัน Control บังเมาส์
        player = GetTree().GetFirstNodeInGroup("player") as Player;
        
        panel = GetNode<Control>("Panel");
        btn1 = GetNode<Button>("Panel/VBoxContainer/Button1");
        btn2 = GetNode<Button>("Panel/VBoxContainer/Button2");
        btn3 = GetNode<Button>("Panel/VBoxContainer/Button3");

        btn1.Pressed += () => OnBuffSelected(0);
        btn2.Pressed += () => OnBuffSelected(1);
        btn3.Pressed += () => OnBuffSelected(2);
        
        HideUI();
    }

    public void ShowUI(Altar altar)
    {
        currentAltar = altar;
        GenerateRandomBuffs();
        
        panel.Visible = true;
        GetTree().Paused = true; // หยุดเกมชั่วคราวขณะเลือกบัฟ
    }

    public void HideUI()
    {
        panel.Visible = false;
        GetTree().Paused = false;
    }

    private void GenerateRandomBuffs()
    {
        Random rand = new Random();
        
        for (int i = 0; i < 3; i++)
        {
            // สุ่มความหายาก (80% Common, 20% Rare)
            bool isRare = rand.NextDouble() < 0.2f;
            string buffName = "";
            Color rarityColor;

            if (isRare)
            {
                buffName = rareBuffs[rand.Next(rareBuffs.Length)];
                rarityColor = new Color(0.2f, 0.6f, 1.0f); // สีฟ้าสำหรับ Rare
            }
            else
            {
                buffName = commonBuffs[rand.Next(commonBuffs.Length)];
                rarityColor = new Color(1.0f, 1.0f, 1.0f); // สีขาวสำหรับ Common
            }

            currentBuffs[i] = buffName;
            
            Button btn = i == 0 ? btn1 : (i == 1 ? btn2 : btn3);
            btn.Text = (isRare ? "[RARE] " : "[COMMON] ") + buffName;
            btn.Modulate = rarityColor;
        }
    }

    private void OnBuffSelected(int index)
    {
        string buff = currentBuffs[index];
        ApplyBuff(buff);
        
        // ทำให้เสาโทเทมต้นนี้ใช้งานไม่ได้อีก
        if (currentAltar != null)
        {
            currentAltar.Deactivate();
        }

        HideUI();
    }

    private void ApplyBuff(string buffName)
    {
        if (player == null) return;

        switch (buffName)
        {
            // Common
            case "Heal 30%":
                player.Hp += (int)(player.MaxHp * 0.3f);
                if (player.Hp > player.MaxHp) player.Hp = player.MaxHp;
                break;
            case "MaxHP +50":
                player.MaxHp += 50;
                player.Hp += 50;
                break;
            case "Damage +1":
                player.BonusDamage += 1;
                break;
            case "Speed +10%":
                player.SpeedModifier += 0.1f;
                break;

            // Rare
            case "Full Heal":
                player.Hp = player.MaxHp;
                break;
            case "MaxHP +150":
                player.MaxHp += 150;
                player.Hp += 150;
                break;
            case "Damage +3":
                player.BonusDamage += 3;
                break;
            case "Fire Rate +20%":
                player.FireRateModifier -= 0.2f; // ลดคูลดาวน์ลง 20%
                if (player.FireRateModifier < 0.2f) player.FireRateModifier = 0.2f; // แคปไว้ไม่ให้ยิงรัวเกินไป
                break;
            case "Random Skill":
                Random rand = new Random();
                ModifierType[] skills = { ModifierType.Fire, ModifierType.MultiShot }; // หินสุ่ม
                ModifierType randomSkill = skills[rand.Next(skills.Length)];
                player.AddToInventory(randomSkill);
                break;
        }
        
        GD.Print($"ได้รับบัฟ: {buffName}");
    }
}
