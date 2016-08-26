namespace CipherStone
{
    public static class consistantHash
    {
        public static ulong ConsistantHash(string read)
        {
            ulong hashedValue = 3074457345618258791ul;
            foreach (char t in read)
            {
                hashedValue += t;
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }
    }
}