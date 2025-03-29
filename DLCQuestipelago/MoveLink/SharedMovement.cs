namespace DLCQuestipelago.MoveLink
{
    public class SharedMovement
    {
        public float Time { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

        public SharedMovement(float time, float x, float y)
        {
            Time = time;
            X = x;
            Y = y;
        }
    }
}
