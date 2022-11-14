using System;

namespace Models
{
    public class Agent
    {
        public int Number = 0;
        
        public int Food = 0;

        public Tuple<int, int> StartingCords = new(0,0);

        public Tuple<int, int> Position = new(0,0);
    }
}