namespace BDSM
{
    public class BdsmVariable
    {
        public string Name;
        public VariableType Type;
        public byte[] Value;
        public int Address;

        public BdsmVariable(string name)
        {
            Name = name;
        }
    }
}
