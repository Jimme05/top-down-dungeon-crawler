using Godot;

// ประกาศ Enum (ประเภทข้อมูลแบบตัวเลือก) สำหรับชนิดของ Modifier
public enum ModifierType
{
    None,       // ช่องว่าง (ไม่ได้ใส่หิน)
    Fire,       // หินไฟ (ยิงแล้วศัตรูเลือดลดต่อเนื่อง)
    MultiShot   // หินกระจาย (ยิงออกไป 3 แฉก)
}
