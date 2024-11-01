using System.Collections.Generic;

namespace BDSM
{
    public static class PopInstruction
    {
        public static void Handle(List<BdsmArgument> arguments, InstructionResult result)
        {
            if (arguments.Count != 1)
            {
                result.Success = false;
                result.Message = "POP requires a destination input argument.";
                return;
            }
            if (arguments[0].Type == ArgumentType.INPUT)
            {
                result.InstructionByte = (byte)OpCodes.POP_INP;
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
            result.Message = "Invalid arguements for POP instruction.";
            return;
        }
    }
}
