using System.Collections.Generic;

namespace BDSM
{
    public static class PushInstruction
    {
        public static void Handle(List<BdsmArgument> arguments, InstructionResult result)
        {
            if (arguments.Count != 1)
            {
                result.Success = false;
                result.Message = "PUSH requires a source output argument.";
                return;
            }
            if (arguments[0].Type == ArgumentType.OUTPUT)
            {
                result.InstructionByte = (byte)OpCodes.PUSH_OUT;
                return;
            }
            else if (arguments[0].Type == ArgumentType.LITERAL)
            {
                result.InstructionByte = (byte)OpCodes.PUSH_LIT;
                return;
            }
            else if (arguments[0].Type == ArgumentType.VARIABLE)
            {
                if (!arguments[0].Instruction.CodeFile.Variables.ContainsKey(arguments[0].ArgumentString))
                {
                    result.Success = false;
                    result.Message = "Variable: " + arguments[1].ArgumentString + " is not defined.";
                    return;
                }
                arguments[0].Value = arguments[0].Instruction.CodeFile.Variables[arguments[0].ArgumentString].Address;
                return;
            }

            result.Success = false;
            result.Message = "Invalid arguements for PUSH instruction.";
            return;
        }
    }
}
