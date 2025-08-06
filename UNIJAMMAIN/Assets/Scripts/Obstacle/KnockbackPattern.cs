public class KnockbackPattern
{
    bool isEnabled = false;
    int hp = 2;
    public void OnKnockback(bool isOn)
    {
        isEnabled = isOn;
    }

    public bool CheckKnockback()
    {
        if (!isEnabled) return false;
        if(--hp == 0) return false;

        return true;
    }
}