namespace Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Model
{
    public sealed class Action
    {
        public double target_velocity_x;
        public double target_velocity_y;
        public double target_velocity_z;
        public double jump_speed;
        public bool use_nitro;

        public override string ToString()
        {
            return target_velocity_x+","+target_velocity_y+","+target_velocity_z+" j "+jump_speed;
        }
    }
}