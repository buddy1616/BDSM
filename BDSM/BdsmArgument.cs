namespace BDSM
{
    public class BdsmArgument
    {
        public string ArgumentString;
        public ArgumentType Type;
        public int Value;
        public bool Success = true;
        public string Message = "";
        public BdsmInstruction Instruction;
        public BdsmArgument(string argument, BdsmInstruction instruction)
        {
            Instruction = instruction;
            ArgumentString = argument.Trim().ToLower();
            if (Instruction.CodeFile.LabelAddresses.ContainsKey(ArgumentString))
            {
                Type = ArgumentType.LABEL;
            }
            else if (Instruction.CodeFile.Variables.ContainsKey(ArgumentString))
            {
                Type = ArgumentType.VARIABLE;
                Value = Instruction.CodeFile.Variables[ArgumentString].Address;
            }
            else if (ArgumentString.Substring(0, 1) == "i")
            {
                try
                {
                    if (ArgumentString.Length == 5)
                    {
                        Value = Convert.ToInt32(ArgumentString.Substring(1, 4), 2);
                    }
                    else
                    {
                        Value = Convert.ToInt32(ArgumentString.Substring(1));
                    }
                    Type = ArgumentType.INPUT;
                    Value = Value << 4;
                }
                catch (Exception e)
                {
                    Message = "Could not interpret Input argument value.";
                    Success = false;
                }
            }
            else if (ArgumentString.Substring(0, 1) == "o")
            {
                try
                {
                    if (ArgumentString.Length == 5)
                    {
                        Value = Convert.ToInt32(ArgumentString.Substring(1, 4), 2);
                    }
                    else
                    {
                        Value = Convert.ToInt32(ArgumentString.Substring(1));
                    }
                    Type = ArgumentType.OUTPUT;
                }
                catch (Exception e)
                {
                    Message = "Could not interpret output argument value.";
                    Success = false;
                }
            }
            else if (ArgumentString.Substring(0, 1) == "b")
            {
                try
                {
                    Value = Convert.ToInt32(ArgumentString.Substring(1), 2);
                    Type = ArgumentType.LITERAL;
                }
                catch (Exception e)
                {
                    Message = "Could not interpret binary literal argument value.";
                    Success = false;
                }
            }
            else if (ArgumentString.Substring(0, 1) == "[")
            {
                string arg = ArgumentString.Replace("[", "").Replace("]", "").Replace(" ", "").Trim();
                if (arg.Substring(0, 1) == "b")
                {
                    try
                    {
                        Value = Convert.ToInt32(arg.Substring(1), 2);
                    }
                    catch (Exception e)
                    {
                        Message = "Could not interpret binary address argument value.";
                        Success = false;
                    }
                }
                else
                {
                    try
                    {
                        Value = Convert.ToInt32(arg);
                    }
                    catch (Exception e)
                    {
                        Message = "Could not interpret address argument value.";
                        Success = false;
                    }
                }
                Type = ArgumentType.ADDRESS;
            }
            else
            {
                try
                {
                    Value = Convert.ToInt32(ArgumentString);
                    Type = ArgumentType.LITERAL;
                }
                catch (Exception e)
                {
                    Message = "Could not interpret literal argument value, or label/variable has not been defined.";
                    Success = false;
                }
            }

            if (Value > 255)
            {
                Message = "Argument out of range for data type.";
                Success = false;
            }
        }
    }
}
