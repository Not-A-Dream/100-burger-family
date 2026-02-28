using System;

[Serializable]
public class RoomState
{
    public bool wateredToday;
    public int burgerCount;

    public bool isCooking;
    public DateTime cookingEndsAtUtc;

    public string lastMessage;

    public void ResetDaily()
    {
        wateredToday = false;
    }
}