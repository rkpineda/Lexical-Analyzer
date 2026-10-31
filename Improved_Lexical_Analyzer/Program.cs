using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.IO;

namespace Improved_Lexical_Analyzer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string filePath = "test.txt";

            if (File.Exists(filePath))
            {
                File.WriteAllText(writePath, $"{"Lexeme", -20}|{"Token", 20}\n");
                File.AppendAllText(writePath, "=========================================\n");

                foreach (string line in File.ReadLines(filePath))
                {
                    string line_code = line;
                    analyzer(line_code);
                    File.AppendAllText(writePath, "\n");
                    Console.WriteLine();
                }
            }
            else Console.WriteLine($"File not found: {Path.GetFullPath(filePath)}"); // FIX: don't exit silently

        }

        public static string writePath = "output.txt";


        public enum state
        {
            start = 0,
            str = 1,
            chr = 2,
            integer = 3,
            flt = 4,
            id = 5,
            punc = 6,
            ops = 7,
            keyword = 8,
            invalid = 10
        }

        public static readonly Dictionary<string, string> keywords = new Dictionary<string, string>
        {
            {"if", "keyword"}, {"else", "keyword"}, {"while", "keyword"}, {"for", "keyword"}, {"return", "keyword"},
            {"int", "datatype"}, {"float", "datatype"}, {"string", "datatype"}, {"bool", "datatype"}, {"void", "keyword"},
            {"true", "bool_value"}, {"false", "bool_value"}, {"print", "keyword"}, {"not", "not_keyword"}, {"or", "or_keyword"},
            {"and", "and_keyword"}
        };

        public static readonly Dictionary<string, string> operators = new Dictionary<string, string>
        {
            {"+", "add_op"}, {"-", "sub_op"}, {"/", "div_op"}, {"*", "mul_op"}, {"%", "mod_op"}, {"=", "asgn_op"}, {">", "grtr_op"},
            {"<", "less_op"}, {"+=", "add_asgn"}, {"-=", "sub_asgn"}, {"*=", "mul_asgn"}, {"/=", "div_asgn"},
            {"%=", "mod_asgn"}, {"**=", "exp_asgn"}, {"//=", "flr_asgn"}, {"**", "exp_op"}, {"//", "flr_op"}, {"++", "inc_op"},
            {"--", "dec_op"}, {"==", "eql_op"}, {"!=", "neql_op"}, {"!", "not_op"}, {">=", "grtr_eql"}, {"<=", "less_eql"},
            {"<=>", "bi_op"}, {"=>", "flw_op"}
        };

        public static readonly Dictionary<char, string> uni_delimiters = new Dictionary<char, string>
        {
            {';', "semi_colon"}, {'[', "op_sqr"}, {']', "cls_sqr"}, {'{', "op_brs"}, {'}', "cls_brs"}, {'(', "op_par"}, {')', "cls_par"}, {',', "comma"}
        };

        public static readonly HashSet<char> id_delimiters = new HashSet<char>
        {
            '+', '-', '*', '/', '=', '!', '<', '>', '%'
         };

        public static bool op_delimiters(char current_ch)
        {
            if (Char.IsLetterOrDigit(current_ch) || current_ch == '_') return true;
            return false;
        }

        public static void token_category(int current_state, ref string specified_type)
        {
            switch (current_state)
            {
                case 1:
                    specified_type = "string_lit";
                    break;
                case 2:
                    specified_type = "char_lit";
                    break;
                case 3:
                    specified_type = "int_lit";
                    break;
                case 4:
                    specified_type = "flt_lit";
                    break;
                case 5:
                    specified_type = "identifier";
                    break;
                case 10:
                    specified_type = "invalid";
                    break;
            }
            return;
        }

        public static void print(string line_code, int current_start_token, int current_index, int current_state)
        {
            string specified_type = string.Empty;
            string token = line_code.Substring(current_start_token, current_index - current_start_token);
            if (keywords.ContainsKey(token))
            {
                specified_type = keywords[token];
                current_state = (int)state.keyword;
            }
            else if (operators.ContainsKey(token))
            {
                specified_type = operators[token];
                current_state = (int)state.ops;
            }
            else if (uni_delimiters.ContainsKey(token[0]))
            {
                specified_type = uni_delimiters[token[0]];
                current_state = (int)state.punc;
            }
            else token_category(current_state, ref specified_type);

            Console.WriteLine($"{token,-20}:{specified_type,20}");
            File.AppendAllText(writePath, $"{token,-20}:{specified_type,20}\n");
        }

        public static void reset(ref int current_index, ref int current_start_token, ref int char_counter,
            ref int current_state, ref bool is_string, ref bool is_char, ref bool is_op, ref bool is_id,
            ref bool is_invalid, ref bool is_float, ref bool is_int, ref bool just_saw_dot)
        {
            current_index++;
            current_start_token = current_index;
            char_counter = current_state = (int)state.start;
            is_id = is_invalid = is_int = is_float = just_saw_dot = is_string = is_char = is_op = false;
            return;
        }

        public static void analyzer(string line_code)
        {
            int len = line_code.Length, current_start_token = 0, ch_counter = 0, current_index = 0, current_state = 0;
            bool is_string = false, is_char = false, is_op = false, is_id = false, is_invalid = false;
            bool is_float = false, is_int = false, just_saw_dot = false;


            while (current_index < len)
            {

                if (char.IsWhiteSpace(line_code[current_index]) && !is_char && !is_string)
                {
                    if (is_float || is_id || is_int || is_invalid || is_op)
                    {
                        print(line_code, current_start_token, current_index, current_state);
                    }

                    reset(ref current_index, ref current_start_token, ref ch_counter, ref current_state, ref is_string, ref is_char, ref is_op, ref is_id, ref is_invalid, ref is_float, ref is_int, ref just_saw_dot);
                    current_state = (int)state.invalid;
                    continue;
                }

                if (operators.ContainsKey(line_code.Substring(current_index, 1)) && !is_string && !is_char)
                {
                    if (is_float || is_id || is_int || is_invalid)
                    {
                        print(line_code, current_start_token, current_index, current_state);
                    }

                    int op_len = 1;
                    for (int l = Math.Min(3, len - current_index); l >= 1; l--)
                    {
                        if (operators.ContainsKey(line_code.Substring(current_index, l)))
                        {
                            op_len = l;
                            break;
                        }
                    }

                    current_start_token = current_index;
                    print(line_code, current_start_token, current_index + op_len, current_state);
                    current_index += op_len - 1;
                    reset(ref current_index, ref current_start_token, ref ch_counter, ref current_state, ref is_string, ref is_char, ref is_op, ref is_id, ref is_invalid, ref is_float, ref is_int, ref just_saw_dot);
                    current_state = (int)state.invalid;
                    continue;
                }

                if (uni_delimiters.ContainsKey(line_code[current_index]) && !is_string && !is_char)
                {
                    if (is_float || is_id || is_int || is_invalid || is_op) print(line_code, current_start_token, current_index, current_state);


                    current_start_token = current_index;
                    print(line_code, current_start_token, current_index + 1, current_state);
                    reset(ref current_index, ref current_start_token, ref ch_counter, ref current_state, ref is_string, ref is_char, ref is_op, ref is_id, ref is_invalid, ref is_float, ref is_int, ref just_saw_dot);
                    current_state = (int)state.invalid;
                    continue;

                }

                if (line_code[current_index] == '"' && !is_char)
                {
                    if (is_float || is_id || is_int || is_invalid || is_op)
                    {
                        print(line_code, current_start_token, current_index, current_state);
                        is_id = is_invalid = is_int = is_float = just_saw_dot = is_string = is_char = is_op = false;
                    }

                    if (!is_string)
                    {
                        current_start_token = current_index;
                        is_string = true;
                        current_state = (int)state.invalid;
                        current_index++;
                        continue;
                    }

                    if (is_string)
                    {
                        is_string = false;
                        current_state = (int)state.str;
                        print(line_code, current_start_token, current_index + 1, current_state);
                        current_index++;
                        current_start_token = current_index;
                        ch_counter = 0;
                        continue;
                    }


                }

                if (line_code[current_index] == '\'' && !is_string)
                {
                    if (is_float || is_int || is_id || is_invalid || is_op)
                    {
                        print(line_code, current_start_token, current_index, current_state);
                        current_start_token = current_index;
                        is_id = is_invalid = is_int = is_float = just_saw_dot = is_string = is_char = is_op = false;
                        current_state = (int)state.start;
                    }

                    if (!is_char)
                    {
                        is_char = true;
                        current_state = (int)state.invalid;
                        current_index++;
                        continue;
                    }

                    if (is_char)
                    {
                        is_char = false;
                        current_state = ch_counter == 1 ? (int)state.chr : (int)state.invalid;
                        print(line_code, current_start_token, current_index + 1, current_state);
                        current_index++;
                        current_start_token = current_index;
                        ch_counter = 0;
                        continue;
                    }
                }

                if (is_char || is_string)
                {
                    if (line_code[current_index] == '\\' && current_index + 1 < len) current_index++;

                    ch_counter++;
                    current_index++;
                    continue;
                }

                if (char.IsDigit(line_code[current_index]) && !is_string && !is_char)
                {
 
                    if (!is_float && !is_int && !is_id && !is_invalid)
                    {
                        is_int = true;
                        current_state = (int)state.integer;
                    }
                    else if (just_saw_dot && !is_id && !is_invalid)
                    {
                        is_float = true;
                        current_state = (int)state.flt;
                    }

                    just_saw_dot = false;
                    current_index++;
                    continue;
                }

                if (line_code[current_index] == '.')
                {
                    if (is_float || just_saw_dot || (!is_int && !is_id)) is_invalid = true;
                    current_state = (int)state.invalid;

                    just_saw_dot = true;
                    current_index++;
                    continue;
                }

                if ((Char.IsLetter(line_code[current_index]) || line_code[current_index] == '_') && !is_string && !is_char)
                {

                    if (is_int || is_float)
                    {
                        is_invalid = true;
                        current_state = (int)state.invalid;
                    }
                    if (!is_id && !is_invalid)
                    {
                        is_id = true;
                        current_state = (int)state.id;
                    }

                    current_index++;
                    continue;
                }

                is_invalid = true;
                current_state = (int)state.invalid;
                current_index++;

            }

            
            if (is_int || is_float || is_id || is_invalid || is_string || is_char || is_op)
            {
                print(line_code, current_start_token, current_index, current_state);
            }

        }
    }
}