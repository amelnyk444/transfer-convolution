using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerenosZhortka
{
    class PushDownAutomaton
    {
        private string _stack;//Stack
        private string str;//inputed string
        private string RightOutput;//Right output
        private List<List<string>> LeftRightTable;// table of left- and rightoutputed symbols
        private List<List<string>> PrecedencyMatrix;//Precedency Matrix
        private List<string>[] Rules;//Rules List
        private List<string> Terminals;//Terminals List
        private List<string> Nonterminals;//Nonterminals List
        List<string> Union ;//Terminals and Nonterminals union
        public PushDownAutomaton()
        {
            _stack = "$";
            str = "";
            RightOutput = "";
            Terminals = new List<string>();
            Nonterminals = new List<string>();
            Union = new List<string>();
            Rules = new List<string>[2];
            List<string> rules = new List<string>();
            List<string> vyv = new List<string>();
            Rules[0] = rules;
            Rules[1] = vyv;
            LeftRightTable = new List<List<string>>();
            PrecedencyMatrix = new List<List<string>>();
        }
        public void InputGrammar()
        {
            Console.WriteLine("Enter rules.The first inputed row will be considered as a start rule" +
            " .If you want to finish the input, input '.' instead of left part of rule");
            Union.Add("$");
            for (int i = 0;; i++)
            {
                string inp = Console.ReadLine();
                Console.Write("Left part: ");
                inp = Console.ReadLine();
                if(inp == ".") { break; }
                if (inp.Length > 1) { Console.WriteLine("Left part length must be 1, input again");continue; }
                if (!Nonterminals.Contains(inp)) { Nonterminals.Add(inp); }
                Rules[0].Add(inp);
                Console.Write("Right part: ");
                inp = Console.ReadLine();
                if (Rules[1].Contains(inp))
                { Console.WriteLine("Simple precedence grammar can`t have two equal rules, input again");
                    Rules[0].RemoveAt(Rules[0].Count - 1);
                    i -= 1;
                    continue;
                }
                if(inp.Contains('$'))//$ - the last stack symbol
                {
                    Console.WriteLine("$ is the pushdown automatons reserved symbol, you can`t use it, try again");
                    Rules[0].RemoveAt(Rules[0].Count - 1);
                    i -= 1;
                    continue;
                }
                Rules[1].Add(inp);
                Console.WriteLine(" ({2}) {0}->{1}", Rules[0][i], Rules[1][i],i+1);
            }
            //We have already formed Nonterminals list, so other symbols are terminals
            for(int i = 0;i<Rules[1].Count;i++)
                for(int j = 0;j<Rules[1][i].Length;j++)
                {
                    if(Nonterminals.Contains(Rules[1][i][j].ToString())||Terminals.Contains(Rules[1][i][j].ToString()))
                    {
                        continue;
                    }
                    else
                    {
                        Terminals.Add(Rules[1][i][j].ToString());
                    }
                }
            CreateLR();
            if(CreatePrMatrix())//Simmple-precedence grammar check
            {
                Show();
                StartAlgorithm();
            }
            else { Console.WriteLine("This grammar is not a precedence grammar"); }
        }        
        private string FindLeft(string elem)//Function, which is looking for leftmost symbols
        {   string result = "";
            for(int i=0;i<Rules[0].Count;i++)
                if (Rules[0][i] == elem && !result.Contains((Rules[1][i])[0])) { result += (Rules[1][i])[0]; }

            for (int i = 0;i<result.Length;i++)           
                if (result[i].ToString()!=elem && Nonterminals.Contains(result[i].ToString()))
                {
                    string s = FindLeft(result[i].ToString());
                    for(int j=0;j<s.Length;j++)
                        if (s == ""||result.Contains(s[j])) { continue; }
                        else { result += s[j]; }
                    
                }                                      
            return result;
        }      
        
        private string FindRight(string elem)//Function, which is looking for rightmost symbols
        {
            string result = "";
            for (int i = 0; i < Rules[0].Count; i++)
                if (Rules[0][i] == elem && !result.Contains((Rules[1][i])[Rules[1][i].Length - 1])) { result += (Rules[1][i])[Rules[1][i].Length - 1]; }

            for (int i = 0; i < result.Length; i++)
                if (result[i].ToString() != elem && Nonterminals.Contains(result[i].ToString()))
                {
                    string s = FindRight(result[i].ToString());
                    for (int j = 0; j < s.Length; j++)
                        if (s == "" || result.Contains(s[j])) { continue; }
                        else { result += s[j]; }
                }
            return result;
        }       
        private void CreateLR() //Function,which creates table of left- and rightoutputed symbols
        {   List<string> row = new List<string>();
            row.Add("N");
            row.Add("L(G)");
            row.Add("R(G)");
            LeftRightTable.Add(row);
            for (int i = 0; i < Nonterminals.Count; i++)
            {   List<string> row1 = new List<string>();
                row1.Add(Nonterminals[i]);
                row1.Add(FindLeft(Nonterminals[i]));
                row1.Add(FindRight(Nonterminals[i]));
                LeftRightTable.Add(row1);
            }            
        }
        
        private bool CreatePrMatrix()//Function,which creates precedency matrix
        {
            FormPrMatrix();
            for (int i = 0;i<Rules[0].Count;i++)
            {
                string s = Rules[1][i];
                //Check for "=" relation
                for (int j = 0;j<Rules[1][i].Length-1;j++)
                {
                    string elem=PrecedencyMatrix[Union.IndexOf(s[j].ToString())+1][Union.IndexOf(s[j + 1].ToString())+1];
                    if(" "==elem || "=" == elem)
                    { PrecedencyMatrix[Union.IndexOf(s[j].ToString()) + 1][ Union.IndexOf(s[j + 1].ToString()) + 1] = "="; }
                    else { return false; }                   
                }
                //Check for "<" relation
                for (int j=s.Length-1;j>0;j--)
                {
                    string ls = FindLeft(s[j].ToString());
                    if (ls == "") { continue; }
                    for (int k = 0; k < ls.Length; k++)
                    {
                        string elem = PrecedencyMatrix[Union.IndexOf(s[j - 1].ToString()) + 1][ Union.IndexOf(ls[k].ToString()) + 1];
                        if (" " == elem||"<"==elem)
                        { PrecedencyMatrix[Union.IndexOf(s[j - 1].ToString()) + 1][ Union.IndexOf(ls[k].ToString()) + 1] = "<"; }
                        else { return false; }
                    }                   
                }
                //Check for ">" relation
                for (int j = 0;j< Rules[1][i].Length - 1; j++)
                {   string rs = FindRight(s[j].ToString());
                    string ls = FindLeft(s[j + 1].ToString());
                    if (rs == "") {continue;}
                    for (int k = 0; k < rs.Length; k++)
                    {  for (int h = 0; h < ls.Length; h++)
                        {   string elem = PrecedencyMatrix[Union.IndexOf(rs[k].ToString()) + 1][Union.IndexOf(ls[h].ToString()) + 1];
                            if (" " == elem || ">" == elem) { PrecedencyMatrix[Union.IndexOf(rs[k].ToString()) + 1][Union.IndexOf(ls[h].ToString()) + 1] = ">";}
                            else { return false; }                           
                        }
                        string elem1 = PrecedencyMatrix[Union.IndexOf(rs[k].ToString()) + 1][Union.IndexOf(s[j + 1].ToString()) + 1];
                        if (" " == elem1 || ">" == elem1) { PrecedencyMatrix[Union.IndexOf(rs[k].ToString()) + 1][Union.IndexOf(s[j + 1].ToString()) + 1] = ">";}
                        else { return false; }
                    }                 
                }                
            }
            //Forming ">" and "<" relations for $-symbol
            for(int i = 1; i < Union.Count; i++) { if(FindLeft(Rules[0][0]).Contains(PrecedencyMatrix[0][i])) PrecedencyMatrix[Union.Count][i] = "<"; }
            for(int i = 1; i < Union.Count; i++) { if (FindRight(Rules[0][0]).Contains(PrecedencyMatrix[i][0])) PrecedencyMatrix[i][Union.Count] = ">"; }
            return true;
        }
        private void FormPrMatrix()//Precedency matrix forming
        { List<string> row = new List<string>();
            row.Add(" ");
            for (int i = 0; i < Union.Count; i++)
                row.Add(Union[i]);
            PrecedencyMatrix.Add(row);
            for (int i = 0; i < Union.Count; i++)
            {
                List<string> row1 = new List<string>();
                row1.Add(Union[i]);
                for (int j = 0; j < Union.Count; j++)
                    row1.Add(" ");
                PrecedencyMatrix.Add(row1);
            }
        }       
        private void Perenos() //Transfer
        {
            _stack += str[0];
            str = str.Remove(0,1);
        }        
        private int Zhortka()//Convolution
        {
            string zhStr = _stack[_stack.Length - 1].ToString();
            for(int i = _stack.Length -1;i>0;i--)
            {
                switch(PrecedencyMatrix[Union.IndexOf(_stack[i - 1].ToString()) + 1][Union.IndexOf(_stack[i].ToString()) + 1])
                {   case "<":
                        for (int j = 0; j < Rules[0].Count; j++)
                            if (zhStr == Rules[1][j])
                            {   _stack=_stack.Remove(i,zhStr.Length);
                                _stack += Rules[0][j];
                                return j+1;
                            }
                        break;
                    case "=":
                        zhStr = _stack[i - 1] + zhStr;
                        break;
                    default:
                        return -1;
                }               
            }
            return -1;
        }        
        public void Show()//Tables demonstration
        {
            Console.WriteLine("Table of the left and right outputed symbols:");
            for(int i = 0;i<Nonterminals.Count+1;i++)
            {
                for (int j = 0; j < 3; j++)
                    Console.Write(LeftRightTable[i][j]+"         ");
                Console.WriteLine();
            }
            Console.WriteLine("Precedency matrix:");
            for(int i = 0;i<Union.Count+1;i++)
            {
                for (int j = 0; j < Union.Count+1; j++)
                    Console.Write(PrecedencyMatrix[i][j]+" ");
                Console.WriteLine();
            }
        }
        public void StartAlgorithm()//transfer-convolution algorithm
        {
            Console.Write("Input the string: ");
            str = Console.ReadLine()+"$";
            while (!("$" == str && "$" + Rules[0][0] == _stack)) 
            {
                if ("$" == str && "$" + Rules[0][0] == _stack) { Console.WriteLine("The string is determinated by this grammar"); return; }
                int i = Union.IndexOf(_stack[_stack.Length - 1].ToString()) + 1;
                int j = Union.IndexOf(str[0].ToString()) + 1;
                switch (PrecedencyMatrix[i][j])
                {
                    case "<":
                        Perenos();
                        Console.WriteLine("Perenos ({0},{1},{2})", _stack, str, RightOutput);
                        break;
                    case "=":
                        Perenos();
                        Console.WriteLine("Perenos ({0},{1},{2})", _stack, str, RightOutput);
                        break;
                    case ">":
                        int zh = Zhortka();
                        if (-1 != zh) { RightOutput += zh; Console.WriteLine("Zhortka ({0},{1},{2})", _stack, str, RightOutput); break; }
                        else { Console.WriteLine("The string is not determinated by this grammar"); return; }
                    default:
                        Console.WriteLine("The string is not determinated by this grammar");
                        return;
                }
            }
            Console.WriteLine("The string is determinated by this grammar");
        }
    }
}
