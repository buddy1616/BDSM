namespace BDSM
{
    public static class MathInstruction
    {
        public static void Handle(List<BdsmArgument> arguments, InstructionResult result)
        {
            if (arguments.Count != 2)
            {
                result.Success = false;
                result.Message = "MATH requires 2 operands.";
                return;
            }
            if (arguments[0].Type == ArgumentType.LITERAL
                && arguments[1].Type == ArgumentType.LITERAL)
            {
                result.InstructionByte = (byte)OpCodes.MATH_LIT;
                return;
            }
            else if (arguments[0].Type == ArgumentType.ADDRESS
                && arguments[1].Type == ArgumentType.ADDRESS)
            {
                result.InstructionByte = (byte)OpCodes.MATH_ADD;
                return;
            }
            else if (arguments[0].Type == ArgumentType.LITERAL
                && arguments[1].Type == ArgumentType.ADDRESS)
            {
                result.InstructionByte = (byte)OpCodes.MATH_LA;
                return;
            }
            else if (arguments[0].Type == ArgumentType.ADDRESS
                && arguments[1].Type == ArgumentType.LITERAL)
            {
                result.InstructionByte = (byte)OpCodes.MATH_AL;
                return;
            }
            else if (arguments[0].Type == ArgumentType.VARIABLE
                && arguments[1].Type == ArgumentType.LITERAL)
            {
                if (!arguments[0].Instruction.CodeFile.Variables.ContainsKey(arguments[0].ArgumentString))
                {
                    result.Success = false;
                    result.Message = "Variable: " + arguments[0].ArgumentString + " is not defined.";
                    return;
                }
                arguments[0].Value = arguments[0].Instruction.CodeFile.Variables[arguments[0].ArgumentString].Address;
                result.InstructionByte = (byte)OpCodes.MATH_AL;
                return;
            }
            else if (arguments[0].Type == ArgumentType.LITERAL
                && arguments[1].Type == ArgumentType.VARIABLE)
            {
                if (!arguments[1].Instruction.CodeFile.Variables.ContainsKey(arguments[1].ArgumentString))
                {
                    result.Success = false;
                    result.Message = "Variable: " + arguments[1].ArgumentString + " is not defined.";
                    return;
                }
                arguments[1].Value = arguments[1].Instruction.CodeFile.Variables[arguments[1].ArgumentString].Address;
                result.InstructionByte = (byte)OpCodes.MATH_LA;
                return;
            }
            else if (arguments[0].Type == ArgumentType.VARIABLE
                && arguments[1].Type == ArgumentType.VARIABLE)
            {
                if (!arguments[0].Instruction.CodeFile.Variables.ContainsKey(arguments[0].ArgumentString))
                {
                    result.Success = false;
                    result.Message = "Variable: " + arguments[0].ArgumentString + " is not defined.";
                    return;
                }
                arguments[0].Value = arguments[0].Instruction.CodeFile.Variables[arguments[0].ArgumentString].Address;
                if (!arguments[1].Instruction.CodeFile.Variables.ContainsKey(arguments[1].ArgumentString))
                {
                    result.Success = false;
                    result.Message = "Variable: " + arguments[1].ArgumentString + " is not defined.";
                    return;
                }
                arguments[1].Value = arguments[1].Instruction.CodeFile.Variables[arguments[1].ArgumentString].Address;
                result.InstructionByte = (byte)OpCodes.MATH_ADD;
                return;
            }

            result.Success = false;
            result.Message = "Invalid arguements for MATH instruction.";
            return;
        }
    }
}
