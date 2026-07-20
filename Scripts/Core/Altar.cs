using Godot;
using System;

public partial class Altar : Area2D
{
    [Export] public int SoulCost = 3;
    
    private bool isPlayerNear = false;
    private bool isUsed = false;
    private Label interactLabel;
    private Player nearbyPlayer;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;

        interactLabel = new Label();
        interactLabel.Text = $"[E / Space] to Interact\nCost: {SoulCost} Souls";
        interactLabel.Position = new Vector2(-70, -80);
        interactLabel.Visible = false;
        AddChild(interactLabel);
    }

    public override void _Process(double delta)
    {
        if (isUsed) return;

        if (isPlayerNear && Input.IsActionJustPressed("ui_accept"))
        {
            if (nearbyPlayer != null && nearbyPlayer.Souls >= SoulCost)
            {
                nearbyPlayer.Souls -= SoulCost;
                OpenAltarUI();
            }
            else
            {
                GD.Print("วิญญาณไม่พอ!");
                interactLabel.Text = $"Not enough souls!\nNeed: {SoulCost}";
                interactLabel.Modulate = new Color(1, 0, 0); // แดง
            }
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (isUsed) return;

        if (body is Player player)
        {
            nearbyPlayer = player;
            isPlayerNear = true;
            interactLabel.Text = $"[E / Space] to Interact\nCost: {SoulCost} Souls";
            interactLabel.Modulate = new Color(1, 1, 1);
            interactLabel.Visible = true;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body is Player)
        {
            nearbyPlayer = null;
            isPlayerNear = false;
            interactLabel.Visible = false;
        }
    }

    private void OpenAltarUI()
    {
        // ซ่อน Label
        interactLabel.Visible = false;
        
        var altarUI = GetTree().GetFirstNodeInGroup("altar_ui") as AltarUI;
        if (altarUI != null)
        {
            altarUI.ShowUI(this);
        }
        else
        {
            GD.PrintErr("ไม่พบ AltarUI ในฉาก! ลืมเพิ่มหรือเปล่า?");
        }
    }

    public void Deactivate()
    {
        isUsed = true;
        isPlayerNear = false;
        if (interactLabel != null) interactLabel.Visible = false;
        
        Modulate = new Color(0.3f, 0.3f, 0.3f); // กลายเป็นสีเทา
        SetDeferred("monitoring", false); 
    }
}
