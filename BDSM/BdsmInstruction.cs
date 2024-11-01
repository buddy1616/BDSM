using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BDSM
{
    public class InstructionResult
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "";
        public byte InstructionByte { get; set; } = 0;
        public bool PrependZPeek { get; set; } = false;
        public int ZPeekAddress { get; set; } = 0;
    }

    public class BdsmInstruction
    {
        public string InstructionString;
        public List<BdsmArgument> Arguments;
        public InstructionResult Result = new InstructionResult();
        public BdsmCodeFile CodeFile;

        public BdsmInstruction(string instructionString, BdsmCodeFile codeFile)
        {
            CodeFile = codeFile;
            string[] result = Regex.Split(instructionString.Replace(',', ' '), "\\s+",
                                          RegexOptions.IgnoreCase,
                                          TimeSpan.FromMilliseconds(500));
            if (result.Length > 0)
            {
                InstructionString = result[0].Trim().ToLower();
                Arguments = new List<BdsmArgument>();
                for (int i = 0; i < result.Length - 1; i++)
                {
                    Arguments.Add(new BdsmArgument(result[i + 1].Trim(), this));
                }
            }

            if (!CheckArguments())
            {
                Result.Success = false;
                return;
            }

            switch (InstructionString)
            {
                case "noop": case "nop": Result.InstructionByte = (byte)OpCodes.NOOP; break;
                case "halt": case "hlt": Result.InstructionByte = (byte)OpCodes.HALT; break;
                case "rset": case "rst": Result.InstructionByte = (byte)OpCodes.RSET; break;

                case "move": case "mov": MoveInstruction.Handle(Arguments, Result);  break;
                case "poke": case "pok": PokeInstruction.Handle(Arguments, Result); break;
                case "peek": case "pek": PeekInstruction.Handle(Arguments, Result); break;
                case "math": case "mat": MathInstruction.Handle(Arguments, Result); break;

                case "add":              HandleArithInstruction((byte)OpCodes.ADD); break;
                case "addc": case "adc": HandleArithInstruction((byte)OpCodes.ADDC); break;
                case "inc":              HandleArithInstruction((byte)OpCodes.INC); break;
                case "sub":              HandleArithInstruction((byte)OpCodes.SUB); break;
                case "subb": case "sbb": HandleArithInstruction((byte)OpCodes.SUBB); break;
                case "dec":              HandleArithInstruction((byte)OpCodes.DEC); break;
                case "shfl": case "shl": HandleArithInstruction((byte)OpCodes.SHFL); break;
                case "rotl": case "rtl": HandleArithInstruction((byte)OpCodes.ROTL); break;

                case "nota": case "nta": HandleArithInstruction((byte)OpCodes.NOTA); break;
                case "nor":              HandleArithInstruction((byte)OpCodes.NOR); break;
                case "anna": case "ana": HandleArithInstruction((byte)OpCodes.ANNA); break;
                case "fals": case "fal": HandleArithInstruction((byte)OpCodes.FALS); break;
                case "nand": case "nan": HandleArithInstruction((byte)OpCodes.NAND); break;
                case "notb": case "nob": HandleArithInstruction((byte)OpCodes.NOTB); break;
                case "xor":              HandleArithInstruction((byte)OpCodes.XOR); break;
                case "annb": case "anb": HandleArithInstruction((byte)OpCodes.ANNB); break;
                case "orna": case "ona": HandleArithInstruction((byte)OpCodes.ORNA); break;
                case "xnor": case "xno": HandleArithInstruction((byte)OpCodes.XNOR); break;
                case "bout": case "bou": HandleArithInstruction((byte)OpCodes.BOUT); break;
                case "and":              HandleArithInstruction((byte)OpCodes.AND); break;
                case "true": case "tru": HandleArithInstruction((byte)OpCodes.TRUE); break;
                case "ornb": case "onb": HandleArithInstruction((byte)OpCodes.ORNB); break;
                case "or":               HandleArithInstruction((byte)OpCodes.OR); break;
                case "aout": case "aou": HandleArithInstruction((byte)OpCodes.AOUT); break;

                case "push": case "psh": PushInstruction.Handle(Arguments, Result); break;
                case "pop":              PopInstruction.Handle(Arguments, Result); break;
                case "swap": case "swp": HandleGenericInstruction("SWAP", (byte)OpCodes.SWAP, new ArgumentType[0]); break;
                case "stsh": case "sts": HandleGenericInstruction("STSH", (byte)OpCodes.STSH, new ArgumentType[0]); break;
                case "rstr": case "rsr": HandleGenericInstruction("RSTR", (byte)OpCodes.RSTR, new ArgumentType[0]); break;
                case "npsh": case "npu": HandleGenericInstruction("NPSH", (byte)OpCodes.NPSH, new ArgumentType[0]); break;
                case "npop": case "npo": HandleGenericInstruction("NPOP", (byte)OpCodes.NPOP, new ArgumentType[0]); break;
                case "call": case "cal": HandleGenericInstruction("CALL", (byte)OpCodes.CALL, new ArgumentType[2] { ArgumentType.LITERAL, ArgumentType.LITERAL }); break;
                case "rtrn": case "ret": HandleGenericInstruction("RTRN", (byte)OpCodes.RTRN, new ArgumentType[0]); break;

                case "jump": case "jmp": HandleJumpInstruction((byte)OpCodes.JUMP); break;
                case "jmpc": case "jc":  HandleJumpInstruction((byte)OpCodes.JMPC); break;
                case "jpnc": case "jnc": HandleJumpInstruction((byte)OpCodes.JPNC); break;
                case "jmpz": case "jz":  HandleJumpInstruction((byte)OpCodes.JMPZ); break;
                case "jpnz": case "jnz": HandleJumpInstruction((byte)OpCodes.JPNZ); break;
                case "jmpe": case "je":  HandleJumpInstruction((byte)OpCodes.JMPE); break;
                case "jpne": case "jne": HandleJumpInstruction((byte)OpCodes.JPNE); break;
                case "jmpg": case "jg":  HandleJumpInstruction((byte)OpCodes.JMPG); break;
                case "jpng": case "jng": HandleJumpInstruction((byte)OpCodes.JPNG); break;
                case "jmpl": case "jl":  HandleJumpInstruction((byte)OpCodes.JMPL); break;
                case "jpnl": case "jnl": HandleJumpInstruction((byte)OpCodes.JPNL); break;

                case "kaos": case "kao": Result.InstructionByte = (byte)OpCodes.KAOS; break;
            }
        }

        public bool CheckArguments()
        {
            for (int i = 0; i < Arguments.Count; i++)
            {
                if (!Arguments[i].Success)
                {
                    Result.Message = "Argument " + i.ToString() + ": " + Arguments[i].Message;
                    return false;
                }
            }
            return true;
        }

        public byte[] GetByteStream()
        {
            List<byte> bytes = new List<byte>();
            if (Result.PrependZPeek)
            {
                bytes.Add(10);
                bytes.Add((byte)Result.ZPeekAddress);
            }
            bytes.Add(Result.InstructionByte);
            for (int i = 0; i < Arguments.Count; i++)
            {
                bytes.Add((byte)Arguments[i].Value);
            }
            return bytes.ToArray();
        }

        public void HandleGenericInstruction(string instName, byte instructionByte, ArgumentType[] argTypes)
        {
            if (Arguments.Count != argTypes.Length)
            {
                Result.Success = false;
                Result.Message = instName + " must have " + argTypes.Length + " arguments.";
                return;
            }
            for (int i=0;i<argTypes.Length;i++)
            {
                if (Arguments[i].Type != argTypes[i])
                {
                    Result.Success = false;
                    Result.Message = "Argument " + (i+1).ToString() + " must be a(n) " + argTypes[i].ToString() + " type.";
                    return;
                }
            }
            Result.InstructionByte = instructionByte;
            return;
        }

        public void HandleArithInstruction(byte instructionByte)
        {
            if (Arguments.Count != 1)
            {
                Result.Success = false;
                Result.Message = "Arithmetic/Logic instructions must contain exactly 1 destination argument.";
                return;
            }
            Result.InstructionByte = instructionByte;
            return;
        }
        public void HandleJumpInstruction(byte instructionByte)
        {
            if (Arguments.Count < 1 || Arguments.Count > 2)
            {
                Result.Success = false;
                Result.Message = "Jump instructions must have either fully addressed arguments (ie 2 byte addresses), or a single address argument for close jump.";
                return;
            }
            if (Arguments.Count == 2)
            {
                if (Arguments[0].Type != ArgumentType.LITERAL
                    || Arguments[1].Type != ArgumentType.LITERAL)
                {
                    Result.Success = false;
                    Result.Message = "Full Jump instructions arguments must be literal.";
                    return;
                }
                Result.InstructionByte = instructionByte;
                return;
            }
            if (Arguments[0].Type != ArgumentType.LITERAL
                && Arguments[0].Type != ArgumentType.LABEL)
            {
                Result.Success = false;
                Result.Message = "Close jump instructions arguments must be literal or a defined label.";
                return;
            }

            if (Arguments[0].Type == ArgumentType.LITERAL)
            {
                Result.InstructionByte = (byte)(instructionByte + 16);
            }
            else
            {
                if (!CodeFile.LabelAddresses.ContainsKey(Arguments[0].ArgumentString.ToLower()))
                {
                    Result.Success = false;
                    Result.Message = "No label defined for: " + Arguments[0].ArgumentString + ".";
                    return;
                }
                int thisInstructionAddress = CodeFile.ProgramBytes.Count;
                int thisInstructionPage = thisInstructionAddress / 256;
                int targetLabelAddress = CodeFile.LabelAddresses[Arguments[0].ArgumentString.ToLower()];
                int targetAddressPage = targetLabelAddress / 256;

                if (thisInstructionPage == targetAddressPage)
                {
                    Arguments[0].Type = ArgumentType.LITERAL;
                    Arguments[0].Value = targetLabelAddress;
                    Result.InstructionByte = (byte)(instructionByte + 16);
                }
                else
                {
                    Arguments[0].Type = ArgumentType.LITERAL;
                    Arguments[0].Value = targetAddressPage;
                    Arguments.Add(new BdsmArgument(targetLabelAddress.ToString(), this));
                    Result.InstructionByte = instructionByte;
                }
            }
            return;
        }
    }
}
