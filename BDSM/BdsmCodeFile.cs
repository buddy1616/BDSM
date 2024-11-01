using System.Text;
using System.Text.RegularExpressions;


namespace BDSM
{
    public class BdsmCodeFile
    {
        public List<byte> ProgramBytes;

        public List<string> ErrorMessages = new List<string>();
        public List<string> WarningMessages = new List<string>();
        public List<string> Messages = new List<string>();
        public bool Success;
        public Dictionary<string, BdsmVariable> Variables = new Dictionary<string, BdsmVariable>();
        public Dictionary<string, int> LabelAddresses = new Dictionary<string, int>();

        private string[] instructionStringLines;
        private int currentVariableAddress = 0;

        public BdsmCodeFile(string[] lines)
        {
            List<string> lineList = new List<string>();

            // replace alias instructions
            for (int i=0;i<lines.Length;i++)
            {
                string line = lines[i].Trim();

                int commentIndex = line.IndexOf(';');
                if (commentIndex >= 0)
                {
                    line = line.Substring(0, commentIndex).Trim();
                }

                string[] instructionParts = Regex.Split(line.Replace(',', ' '), "\\s+",
                                  RegexOptions.IgnoreCase,
                                  TimeSpan.FromMilliseconds(500));
                string instruction = instructionParts[0].Trim().ToLower();
                switch (instruction)
                {
                    case "mova":
                    case "lda":
                        if (instructionParts.Length == 2)
                        {
                            lineList.Add("MOVE I1, " + instructionParts[1].Trim());
                        }
                        else
                        {
                            Error(i, "MOVA/LDA macro takes a single argument.");
                        }
                        break;
                    case "movb":
                    case "ldb":
                        if (instructionParts.Length == 2)
                        {
                            lineList.Add("MOVE I2, " + instructionParts[1].Trim());
                        }
                        else
                        {
                            Error(i, "MOVB/LDB macro takes a single argument.");
                        }
                        break;
                    case "show":
                        if (instructionParts.Length == 2)
                        {
                            lineList.Add("MOVE I3, " + instructionParts[1].Trim());
                        }
                        else
                        {
                            Error(i, "SHOW macro takes a single argument.");
                        }
                        break;
                    default: lineList.Add(line); break;
                }
            }
            instructionStringLines = lineList.ToArray();
        }

        public bool Compile()
        {
            ProgramBytes = new List<byte>();
            LabelAddresses = new Dictionary<string, int>();
            Success = true;

            for (int i = 0; i < instructionStringLines.Length; i++)
            {
                string instruction = instructionStringLines[i].Trim();
                if (instruction.Length > 0)
                {
                    if (instruction.Substring(instruction.Length - 1) == ":")
                    {
                        HandleLabel(i, instruction);
                        continue;
                    }
                    string[] variableParts = instruction.Replace('\t', ' ').Split(' ');
                    if (variableParts.Length == 3)
                    {
                        if (HandleVariable(i, variableParts[0], variableParts[1], variableParts[2]))
                        {
                            continue;
                        }
                    }

                    BdsmInstruction bdsmi = new BdsmInstruction(instruction, this);
                    if (!bdsmi.Result.Success)
                    {
                        Error(i, bdsmi.Result.Message);
                        continue;
                    }
                    ProgramBytes.AddRange(bdsmi.GetByteStream());
                }
            }

            return Success;
        }

        public void HandleLabel(int line, string instruction)
        {
            if (char.IsNumber(instruction[0]))
            {
                Error(line, "Line Labels must start with a letter.");
            }
            else
            {
                string label = instruction.Replace(':', ' ').Trim().ToLower();
                if (Variables.ContainsKey(label))
                {
                    Error(line, label.ToUpper() + " is already defined as a variable.");
                    return;
                }
                LabelAddresses[label] = ProgramBytes.Count;
            }
        }

        public bool HandleVariable(int line, string name, string type, string value)
        {
            BdsmVariable v = new BdsmVariable(name);
            switch (type)
            {
                case "byte":
                    if (AddNewVariable(line, v, VariableType.BYTE, 1, value)) { return true; }
                    break;
            }
            return false;
        }
        public bool AddNewVariable(int line, BdsmVariable v, VariableType type, int size, string value)
        {
            if (LabelAddresses.ContainsKey(v.Name))
            {
                Error(line, v.Name.ToUpper() + " is already defined as a label.");
                return false;
            }
            v.Type = type;
            v.Address = currentVariableAddress;
            if (value.Substring(0, 1).ToLower() == "b")
            {
                try
                {
                    long binaryValue = Convert.ToInt64(value.Substring(1), 2);
                    byte[] bytes = BitConverter.GetBytes(binaryValue);
                    int byteSize = 0;
                    for (int i = bytes.Length - 1; i >= 0; i--)
                    {
                        if (bytes[i] != 0)
                        {
                            byteSize = i + 1;
                            break;
                        }
                    }
                    if (byteSize > size)
                    {
                        Error(line, "Variable binary declaration too large to fit into data type: " + type.ToString());
                        return false;
                    }
                    v.Value = new byte[byteSize];
                    for (int i = 0; i < byteSize; i++)
                    {
                        v.Value[i] = bytes[i];
                    }
                }
                catch { }
            }
            else if (value.Substring(0, 1) == "\""
                    || value.Substring(0, 1) == "'")
            {
                if (value.Substring(value.Length -1, 1).ToLower() == value.Substring(0, 1))
                {
                    string val = value.Substring(1, value.Length - 2);
                    if (val.Length > size)
                    {
                        Error(line, "Variable string declaration too large to fit into data type: " + type.ToString());
                        return false;
                    }
                    byte[] bytes = Encoding.ASCII.GetBytes(val);
                    v.Value = bytes;
                }
                else
                {
                    Error(line, "No closing quote found.");
                    return false;
                }
            }
            else
            {
                try
                {
                    byte[] bytes = BitConverter.GetBytes(Int64.Parse(value));
                    int byteSize = 0;
                    for (int i = bytes.Length - 1; i >= 0; i--)
                    {
                        if (bytes[i] != 0)
                        {
                            byteSize = i + 1;
                            break;
                        }
                    }
                    if (byteSize > size)
                    {
                        Error(line, "Variable binary declaration too large to fit into data type: " + type.ToString());
                        return false;
                    }
                    v.Value = new byte[byteSize];
                    for (int i = 0; i < byteSize; i++)
                    {
                        v.Value[i] = bytes[i];
                    }
                }
                catch { }
            }

            for (int i = 0; i < size; i++)
            {
                ProgramBytes.Add((byte)OpCodes.ZPOKE_LIT);
                ProgramBytes.Add((byte)(currentVariableAddress+i));
                ProgramBytes.Add(v.Value[i]);
            }
            Variables.Add(v.Name, v);
            currentVariableAddress += size;
            return true;
        }

        private void Error(int number, string mess) { ErrorMessages.Add("Error (line " + (number+1).ToString() + "): " + mess); Success = false; }
        private void Warn(int number, string mess) { ErrorMessages.Add("Warning (line " + (number+1).ToString() + "): " + mess); }
        private void Info(string mess) { ErrorMessages.Add(mess); }
    }
}

