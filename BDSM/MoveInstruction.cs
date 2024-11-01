using System.Collections.Generic;

namespace BDSM
{
    public static class MoveInstruction
    {
        public static void Handle(List<BdsmArgument> arguments, InstructionResult result)
        {
            if (arguments.Count < 2)
            {
                result.Success = false;
                result.Message = "MOVE requires a destination input and a source output.";
                return;
            }
            if (arguments[0].Type == ArgumentType.INPUT
                && arguments[1].Type == ArgumentType.OUTPUT)
            {
                result.InstructionByte = (byte)OpCodes.MOVE_IO;
                arguments[0].Value = arguments[0].Value | arguments[1].Value;
                arguments.RemoveAt(1);
                return;
            }
            else if (arguments[0].Type == ArgumentType.INPUT
                && arguments[1].Type == ArgumentType.LITERAL)
            {
                result.InstructionByte = (byte)OpCodes.MOVE_I_LIT;
                return;
            }
            else if (arguments[0].Type == ArgumentType.INPUT
                && arguments[1].Type == ArgumentType.ADDRESS)
            {
                if (arguments[0].Value >> 4 == (int)Inputs.RAM)
                {
                    result.Success = false;
                    result.Message = "MOVE cannot be performed between two RAM addresses, use intermediate stack or register.";
                    return;
                }
                result.InstructionByte = (byte)OpCodes.MOVE_I_VAR;
                return;
            }
            else if (arguments[0].Type == ArgumentType.ADDRESS
                && arguments[1].Type == ArgumentType.OUTPUT)
            {
                if (arguments[1].Value == (int)Outputs.RAM)
                {
                    result.Success = false;
                    result.Message = "MOVE cannot be performed between two RAM addresses, use intermediate stack or register.";
                    return;
                }
                result.InstructionByte = (byte)OpCodes.MOVE_VAR_OUT;
                return;
            }
            else if (arguments[0].Type == ArgumentType.ADDRESS
                && arguments[1].Type == ArgumentType.LITERAL)
            {
                result.InstructionByte = (byte)OpCodes.MOVE_VAR_LIT;
                return;
            }
            else if (arguments[0].Type == ArgumentType.ADDRESS
                && arguments[1].Type == ArgumentType.ADDRESS)
            {
                result.Success = false;
                result.Message = "MOVE cannot be performed between two RAM addresses, use intermediate stack or register.";
                return;
            }
            else if (arguments[0].Type == ArgumentType.VARIABLE
                && arguments[1].Type == ArgumentType.OUTPUT)
            {
                if (arguments[1].Value == (int)Outputs.RAM)
                {
                    result.Success = false;
                    result.Message = "MOVE cannot be performed between a variable and RAM addresses, use intermediate stack or register.";
                    return;
                }
                if (!arguments[0].Instruction.CodeFile.Variables.ContainsKey(arguments[0].ArgumentString))
                {
                    result.Success = false;
                    result.Message = "Variable: " + arguments[0].ArgumentString + " is not defined.";
                    return;
                }
                result.InstructionByte = (byte)OpCodes.MOVE_VAR_OUT;
                arguments[0].Value = arguments[0].Instruction.CodeFile.Variables[arguments[0].ArgumentString].Address;
                return;
            }
            else if (arguments[0].Type == ArgumentType.INPUT
                && arguments[1].Type == ArgumentType.VARIABLE)
            {
                if (arguments[0].Value == (int)Inputs.RAM)
                {
                    result.Success = false;
                    result.Message = "MOVE cannot be performed between a variable and RAM addresses, use intermediate stack or register.";
                    return;
                }
                if (!arguments[1].Instruction.CodeFile.Variables.ContainsKey(arguments[1].ArgumentString))
                {
                    result.Success = false;
                    result.Message = "Variable: " + arguments[1].ArgumentString + " is not defined.";
                    return;
                }
                result.InstructionByte = (byte)OpCodes.MOVE_I_VAR;
                arguments[1].Value = arguments[1].Instruction.CodeFile.Variables[arguments[1].ArgumentString].Address;
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
                result.InstructionByte = (byte)OpCodes.ZPOKE_LIT;
                return;
            }
            else if (arguments[0].Type == ArgumentType.VARIABLE
                && arguments[1].Type == ArgumentType.VARIABLE)
            {
                result.Success = false;
                result.Message = "MOVE cannot be performed between two variables, use intermediate stack or register.";
                return;
            }

            result.Success = false;
            result.Message = "Invalid arguements for MOVE instruction.";
            return;
        }
    }
}
