namespace BDSM
{
    public static class PeekInstruction
    {
        public static void Handle(List<BdsmArgument> arguments, InstructionResult result)
        {
            if (arguments.Count < 1)
            {
                result.Success = false;
                result.Message = "PEEK requires at least a single address byte (for ZPEEK), or 2 address bytes (for full PEEK).";
                return;
            }
            if (arguments.Count == 2)
            {
                if (arguments[0].Type == ArgumentType.LITERAL
                    && arguments[1].Type == ArgumentType.LITERAL)
                {
                    result.InstructionByte = (byte)OpCodes.PEEK_FULL;
                    return;
                }
            }
            else if (arguments.Count == 1)
            {
                if (arguments[0].Type == ArgumentType.LITERAL)
                {
                    result.InstructionByte = (byte)OpCodes.ZPEEK;
                    return;
                }
                if (arguments[0].Type == ArgumentType.VARIABLE)
                {
                    if (!arguments[0].Instruction.CodeFile.Variables.ContainsKey(arguments[0].ArgumentString))
                    {
                        result.Success = false;
                        result.Message = "Variable: " + arguments[0].ArgumentString + " is not defined.";
                        return;
                    }
                    arguments[0].Value = arguments[0].Instruction.CodeFile.Variables[arguments[0].ArgumentString].Address;
                    result.InstructionByte = (byte)OpCodes.ZPEEK;
                    return;
                }
            }

            result.Success = false;
            result.Message = "Invalid arguements for PEEK instruction.";
            return;
        }
    }
}
