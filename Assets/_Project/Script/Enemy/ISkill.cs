public interface ISkill
{
    string Name { get; }
    float Cooldown { get; }
    bool IsReady();
    void Use(Enemy owner);
}