namespace BDSM
{
    public static class PokeInstruction
    {
        public static void Handle(List<BdsmArgument> arguments, InstructionResult result)
        {
            if (arguments.Count < 2)
            {
                result.Success = false;
                result.Message = "POKE requires at least a single address byte (for ZPOKE) and a source output, or 2 address bytes (for full POKE) and a source output.";
                return;
            }
            if (arguments.Count == 3)
            {
                if (arguments[0].Type == ArgumentType.LITERAL
                    && arguments[1].Type == ArgumentType.LITERAL
                    && arguments[2].Type == ArgumentType.LITERAL)
                {
                    result.InstructionByte = (byte)OpCodes.POKE_FULL_LIT;
                    return;
                }
                if (arguments[0].Type == ArgumentType.LITERAL
                    && arguments[1].Type == ArgumentType.LITERAL
                    && arguments[2].Type == ArgumentType.OUTPUT)
                {
                    result.InstructionByte = (byte)OpCodes.POKE_FULL_OUT;
                    return;
                }
            }
            else if (arguments.Count == 2)
            {
                if (arguments[0].Type == ArgumentType.LITERAL
                    && arguments[1].Type == ArgumentType.LITERAL)
                {
                    result.InstructionByte = (byte)OpCodes.ZPOKE_LIT;
                    return;
                }
                if (arguments[0].Type == ArgumentType.LITERAL
                    && arguments[1].Type == ArgumentType.OUTPUT)
                {
                    result.InstructionByte = (byte)OpCodes.ZPOKE_OUT;
                    return;
                }
                if (arguments[0].Type == ArgumentType.VARIABLE
                    && arguments[1].Type == ArgumentType.LITERAL)
                {
                    if (!arguments[0].Instruction.CodeFile.Variables.ContainsKey(arguments[0].ArgumentString))
                    {
                        result.Success = false;
                        result.Message = "Variable: " + arguments[0].ArgumentString + " is not defined.";
                        return;
                    }
                    arguments[0].Value = arguments[0].Instruction.CodeFile.Variables[arguments[0].ArgumentString].Address;
                    result.InstructionByte = (byte)OpCodes.ZPOKE_LIT;
                    return;
                }
                if (arguments[0].Type == ArgumentType.VARIABLE
                    && arguments[1].Type == ArgumentType.OUTPUT)
                {
                    if (!arguments[0].Instruction.CodeFile.Variables.ContainsKey(arguments[0].ArgumentString))
                    {
                        result.Success = false;
                        result.Message = "Variable: " + arguments[0].ArgumentString + " is not defined.";
                        return;
                    }
                    arguments[0].Value = arguments[0].Instruction.CodeFile.Variables[arguments[0].ArgumentString].Address;
                    result.InstructionByte = (byte)OpCodes.ZPOKE_OUT;
                    return;
                }
            }

            result.Success = false;
            result.Message = "Invalid arguements for POKE instruction.";
            return;
        }
    }
}
