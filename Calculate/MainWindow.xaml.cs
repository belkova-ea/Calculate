using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Calculate
{
    public struct Token
    {
        public string type;
        public char op;
        public double num;

        public string Dump()
        {
            if (type == "op")
            {
                return type + op.ToString();
            }
            if (type == "number")
            {
                return type + num.ToString();
            }
            return "broken";
        }
    }

    class RPN
    {
        //Метод Calculate принимает выражение в виде строки и возвращает результат, в своей работе использует другие методы класса
        //"Входной" метод класса
        static public double Calculate(List<Token> tokens)
        {
            var tokens_rpn = GetExpression(tokens);   //Преобразовываем выражение в постфиксную запись

            double result = Counting(tokens_rpn); //Решаем полученное выражение
            return result; //Возвращаем результат
        }

        static public double CalculateString(string input)
        {
            var tokens = new Parser().ParseString(input + "=");
            return Calculate(tokens);
        }

        static private byte GetPriority(Token token)
        {
            switch (token.op)
            {
                case '(': return 0;
                case ')': return 1;
                case '+': return 2;
                case '-': return 3;
                case '*': return 4;
                case '/': return 4;
                case '^': return 5;
                default: return 6;
            }
        }

        //Метод, преобразующий входную строку с выражением в постфиксную запись
        static private List<Token> GetExpression(List<Token> input)
        {
            List<Token> output = new(); //Строка для хранения выражения
            Stack<Token> operStack = new(); //Стек для хранения операторов

            foreach (var token in input)
            {
                //Если символ - цифра, то считываем все число
                if (token.type == "number") //Если цифра
                {
                    output.Add(token);
                }

                //Если символ - оператор
                if (token.type == "op") //Если оператор
                {
                    if (token.op == '(')
                    {
                        operStack.Push(token);
                    }
                    else if (token.op == ')')
                    {
                        var s = operStack.Pop();
                        while (s.type != "op" && s.op != '(')
                        {
                            output.Add(s);
                            s = operStack.Pop();
                        }
                    }
                    else //Если любой другой оператор
                    {
                        if (operStack.Count > 0) //Если в стеке есть элементы
                            if (GetPriority(token) <= GetPriority(operStack.Peek())) //И если приоритет нашего оператора меньше или равен приоритету оператора на вершине стека
                                output.Add(operStack.Pop()); //То добавляем последний оператор из стека в строку с выражением

                        operStack.Push(token); //Если стек пуст, или же приоритет оператора выше - добавляем операторов на вершину стека

                    }
                }
            }

            //Когда прошли по всем символам, выкидываем из стека все оставшиеся там операторы в строку
            while (operStack.Count > 0)
                output.Add(operStack.Pop());

            return output; //Возвращаем выражение в постфиксной записи
        }

        //Метод, вычисляющий значение выражения, уже преобразованного в постфиксную запись
        static private double Counting(List<Token> input)
        {
            double result = 0; //Результат
            Stack<double> temp = new Stack<double>(); //стек для решения

            foreach (var token in input)
            {
                //Если символ - цифра, то читаем все число и записываем на вершину стека
                if (token.type == "number")
                {
                    temp.Push(token.num);
                }
                else if (token.type == "op") //Если символ - оператор
                {
                    //Берем два последних значения из стека
                    double a = temp.Pop();

                    double b = a;
                    if (temp.Count != 0)
                    {
                        b = temp.Pop();
                    }

                    switch (token.op) //И производим над ними действие, согласно оператору
                    {
                        case '+': result = b + a; break;
                        case '-': result = b - a; break;
                        case '*': result = b * a; break;
                        case '/': result = b / a; break;
                        case '^': result = double.Parse(Math.Pow(double.Parse(b.ToString()), double.Parse(a.ToString())).ToString()); break;
                    }
                    temp.Push(result); //Результат вычисления записываем обратно в стек
                }
            }
            return temp.Peek(); //Забираем результат всех вычислений из стека и возвращаем его
        }
    }

    public class Parser
    {
        public string state = "null";
        int open_brackets = 0;

        public string entered_num = "0";
        public List<Token> tokens = new();

        public string Parse(char c)
        {
            string ret = "0";
            try
            {
                ret = RPN.Calculate(tokens).ToString();
            } catch { }

            switch (state)
            {
                case "null":
                    switch (c)
                    {
                        case >= '0' and <= '9':
                        case ',':
                            state = "s_num";
                            input_digit(c);
                            ret = entered_num;
                            break;
                        case '-':
                            state = "unar";
                            ret = "0";
                            break;
                        case '(':
                            state = "obr";
                            ret = "0";
                            break;
                    }
                    break;
                case "s_num":
                    switch (c)
                    {
                        case >= '0' and <= '9':
                        case ',':
                            state = "s_num";
                            input_digit(c);
                            ret = entered_num;
                            break;
                        case '+':
                            state = "op";
                            end_input_digit(c);
                            ret = RPN.Calculate(tokens).ToString();
                            input_operator(c);
                            break;
                        case '-':
                            state = "op";
                            end_input_digit(c);
                            ret = RPN.Calculate(tokens).ToString();
                            input_operator(c);
                            break;
                        case '*':
                            state = "op";
                            end_input_digit(c);
                            ret = RPN.Calculate(tokens).ToString();
                            input_operator(c);
                            break;
                        case '/':
                            state = "op";
                            end_input_digit(c);
                            ret = RPN.Calculate(tokens).ToString();
                            input_operator(c);
                            break;
                        case '=':
                            end_input_digit(c);
                            ret = RPN.Calculate(tokens).ToString();
                            break;
                        case '%':
                            if (tokens.Count() >= 2)
                            {
                                state = "cbr";
                                var tns = tokens.ToList();
                                tns.RemoveAt(tokens.Count() - 1);
                                var n = RPN.Calculate(tns);
                                //System.Diagnostics.Debug.WriteLine(n);
                                entered_num = (n * (double.Parse(entered_num) / 100)).ToString();
                                end_input_digit(c);
                                ret = RPN.Calculate(tokens).ToString();
                            } else
                            {
                                ret = entered_num;
                            }
                            break;
                    }
                    break;
                case "s_num_unar":
                    switch (c)
                    {
                        case >= '0' and <= '9':
                        case ',':
                            state = "s_num_unar";
                            input_digit(c);
                            ret = "-" + entered_num;
                            break;
                        case '+':
                            state = "op";
                            end_input_digit_unar(c);
                            ret = RPN.Calculate(tokens).ToString();
                            input_operator(c);
                            break;
                        case '-':
                            state = "op";
                            end_input_digit_unar(c);
                            ret = RPN.Calculate(tokens).ToString();
                            input_operator(c);
                            break;
                        case '*':
                            state = "op";
                            end_input_digit_unar(c);
                            ret = RPN.Calculate(tokens).ToString();
                            input_operator(c);
                            break;
                        case '/':
                            state = "op";
                            end_input_digit_unar(c);
                            ret = RPN.Calculate(tokens).ToString();
                            input_operator(c);
                            break;
                        case '=':
                            end_input_digit_unar(c);
                            ret = RPN.Calculate(tokens).ToString();
                            break;
                    }
                    break;
                case "unar":
                    switch (c)
                    {
                        case >= '0' and <= '9':
                        case ',':
                            state = "s_num_unar";
                            input_digit(c);
                            break;
                        case '(':
                            state = "obr";
                            break;
                        case '=':
                            end_input_digit_unar(c);
                            ret = RPN.Calculate(tokens).ToString();
                            break;
                    }
                    break;
                case "op":
                    switch (c)
                    {
                        case >= '0' and <= '9':
                        case ',':
                            state = "s_num";
                            input_digit(c);
                            ret = entered_num;
                            break;
                        case '(':
                            state = "obr";
                            break;
                    }
                    break;
                case "obr":
                    switch (c)
                    {
                        case >= '0' and <= '9':
                        case ',':
                            state = "s_num";
                            input_digit(c);
                            break;
                        case '-':
                            state = "unar";
                            input_digit(c);
                            break;
                    }
                    break;
                case "cbr":
                    switch (c)
                    {
                        case '+':
                            state = "op";
                            input_operator(c);
                            break;
                        case '-':
                            state = "op";
                            input_operator(c);
                            break;
                        case '*':
                            state = "op";
                            input_operator(c);
                            break;
                        case '/':
                            state = "op";
                            input_operator(c);
                            break;
                        case >= '0' and <= '9':
                        case ',':
                            state = "s_num";
                            input_digit(c);
                            ret = entered_num;
                            break;
                    }
                    break;

            }
            return ret;
        }

        public List<Token> ParseString(string s)
        {
            foreach (char c in s)
            {
                Parse(c);
            }
            return tokens;
        }

        void input_digit(char c)
        {
            if (c == ',')
            {
                if (entered_num.Contains(','))
                {
                    return;
                }
            }
            else
            {
                if (entered_num == "0")
                {
                    entered_num = "";
                }
            }
            entered_num += c;
        }

        void end_input_digit(char c)
        {
            Token t = new()
            {
                type = "number",
                num = double.Parse(entered_num)
            };
            tokens.Add(t);
            entered_num = "";
        }

        void end_input_digit_unar(char c)
        {
            Token t = new()
            {
                type = "number",
                num = -double.Parse(entered_num)
            };
            tokens.Add(t);
            entered_num = "";
        }

        void input_operator(char c)
        {
            Token t = new()
            {
                type = "op",
                op = c
            };
            tokens.Add(t);
        }
    }

    

    public partial class MainWindow : Window
    {

        double MEMORY_CELL = 0;
        bool FLAG_LOG_VISIBLE = false;
        int SELECTED_ROW = 0;

        Parser parser = new();

        ///////////

        //Метод возвращает true, если проверяемый символ - разделитель ("пробел" или "равно")
        static private bool IsDelimeter(char c)
        {
            if ((" =".IndexOf(c) != -1))
                return true;
            return false;
        }
        //Метод возвращает true, если проверяемый символ - оператор
        static private bool IsOperator(char с)
        {
            if ("+-/*^()".IndexOf(с) != -1)
                return true;
            return false;
        }

        public MainWindow()
        {
            InitializeComponent();
            Application.Current.MainWindow.Width = 250;
        }


        private string MboxText()
        {
            Stack<string> ns = new();
            string mbox = "";
            foreach (var t in parser.tokens)
            {
                if (t.type == "op")
                {
                    if (ns.Count != 0)
                    {
                        mbox = mbox + ns.Pop().ToString();
                        ns.Clear();
                    }
                    mbox = mbox + t.op;
                }
                else if (t.type == "number")
                {
                    ns.Push(t.num.ToString());
                }
            }
            if (ns.Count != 0)
            {
                mbox = mbox + ns.Pop().ToString();
            }
            return mbox;
        }


        private void InputChar(char c)
        {
            if (c == '=')
            {
                var final_value = parser.Parse('=');

                History.Items.Add(MboxText() + " = " + final_value.ToString());

                parser = new();
                parser.state = "cbr";

                Token t = new();
                t.type = "number";
                t.num = double.Parse(final_value);
                parser.tokens.Add(t);
            }
            
            var textbox_value = parser.Parse(c);
            TextBox.Text = textbox_value.ToString();
            MemoryBox.Text = MboxText();
        }


        private void bt1_Click(object sender, RoutedEventArgs e)
        {
            InputChar('1');
        }
        private void bt2_Click(object sender, RoutedEventArgs e)
        {
            InputChar('2');
        }
        private void bt3_Click(object sender, RoutedEventArgs e)
        {
            InputChar('3');
        }
        private void bt4_Click(object sender, RoutedEventArgs e)
        {
            InputChar('4');
        }
        private void bt5_Click(object sender, RoutedEventArgs e)
        {
            InputChar('5');
        }
        private void bt6_Click(object sender, RoutedEventArgs e)
        {
            InputChar('6');
        }
        private void bt7_Click(object sender, RoutedEventArgs e)
        {
            InputChar('7');
        }
        private void bt8_Click(object sender, RoutedEventArgs e)
        {
            InputChar('8');
        }
        private void bt9_Click(object sender, RoutedEventArgs e)
        {
            InputChar('9');
        }
        private void bt0_Click(object sender, RoutedEventArgs e)
        {
            InputChar('0');

        }
        private void znak_Click(object sender, RoutedEventArgs e)
        {
            InputChar(',');
        }


        private void Plus_Click(object sender, RoutedEventArgs e)
        {
            InputChar('+');
        }
        private void Minus_Click(object sender, RoutedEventArgs e)
        {

            InputChar('-');
        }
        private void mult_Click(object sender, RoutedEventArgs e)
        {
            InputChar('*');
        }
        private void del_Click(object sender, RoutedEventArgs e)
        {
            InputChar('/');
        }

        private void OneDelX_Click(object sender, RoutedEventArgs e)
        {
            var final_value = double.Parse(parser.Parse('='));

            if (final_value == 0)
            {
                TextBox.Text = "Error";
                return;
            }

            parser = new();
            parser.state = "cbr";

            Token t = new();
            t.type = "number";
            t.num = 1/final_value;
            parser.tokens.Add(t);

            TextBox.Text = t.num.ToString();
            MemoryBox.Text = "1/" + final_value.ToString();
        }


        private void Root_Click(object sender, RoutedEventArgs e)
        {
            var final_value = double.Parse(parser.Parse('='));

            if (final_value < 0)
            {
                TextBox.Text = "err";
                return;
            }

            parser = new();
            parser.state = "cbr";

            Token t = new();
            t.type = "number";
            t.num = Math.Sqrt(final_value);
            parser.tokens.Add(t);

            TextBox.Text = t.num.ToString();
            MemoryBox.Text = "sqrt(" + final_value.ToString() + ")";
        }


        private void proc_b_Click(object sender, RoutedEventArgs e)
        {
            InputChar('%');
        }


        private void PlusMinus_Click(object sender, RoutedEventArgs e)
        {
            var final_value = double.Parse(parser.Parse('='));
            parser = new();
            parser.state = "cbr";

            Token t = new();
            t.type = "number";
            t.num = -final_value;
            parser.tokens.Add(t);

            TextBox.Text = t.num.ToString();
            MemoryBox.Text = MboxText();
        }


        private void Equal_Click(object sender, RoutedEventArgs e)
        {
            InputChar('=');
            MemoryBox.Clear();
        }


        private void CE_Click(object sender, RoutedEventArgs e)
        {
            TextBox.Clear();
            TextBox.Text = "0";
            parser.entered_num = "0";
        }


        private void C_Click(object sender, RoutedEventArgs e)
        {
            MemoryBox.Clear();
            TextBox.Text = "0";
            parser = new();
        }


        private void str_Click(object sender, RoutedEventArgs e)
        {
            string n = parser.entered_num;
            if (n.Length > 0)
            {
                parser.entered_num = n.Remove(n.Length - 1);
            }

            TextBox.Text = parser.entered_num;
        }


        private void MC_Click(object sender, RoutedEventArgs e)
        {
            MEMORY_CELL = 0;
            Brush temp = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF283E4C"));
            MS.Foreground = temp;
        }


        private void MR_Click(object sender, RoutedEventArgs e)
        {
            parser.entered_num = "";
            TextBox.Text = parser.entered_num;

            MemoryBox.Clear();
            foreach (var c in MEMORY_CELL.ToString())
            {
                InputChar(c);
            }
        }


        private void MS_Click(object sender, RoutedEventArgs e)
        {
            MEMORY_CELL = double.Parse(parser.entered_num);
            Brush temp = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#46A05A"));
            MS.Foreground = temp;
        }


        private void Mplus_Click(object sender, RoutedEventArgs e)
        {
            MemoryBox.Clear();
            MEMORY_CELL += double.Parse(TextBox.Text);
        }


        private void Mminus_Click(object sender, RoutedEventArgs e)
        {
            MemoryBox.Clear();
            MEMORY_CELL -= double.Parse(TextBox.Text);
        }


        private void Log_Click(object sender, RoutedEventArgs e)
        {
            if (FLAG_LOG_VISIBLE == false)
            {
                Application.Current.MainWindow.Width = 500;
                History.Visibility = Visibility.Visible;
                FLAG_LOG_VISIBLE = true;
            }
            else
            {
                History.Visibility = Visibility.Collapsed;
                Application.Current.MainWindow.Width = 250;
                FLAG_LOG_VISIBLE = false;
            }
        }


        private void Copy_Log_Click(object sender, RoutedEventArgs e)
        {
            string temp = "";

            if (History.Items.IsEmpty != true)
            {
                foreach (object item in History.Items) temp += item.ToString() + "\r" + "\n";
            }
            else
            {
                temp = "0";
            }

            Clipboard.SetText(temp);
        }


        private void MenuItem_Click(object sender, RoutedEventArgs e) //Правка - Копировать
        {
            if (TextBox.Text.Length > 0)
            {
                Clipboard.SetText(TextBox.Text);
            }
        }


        private void MenuItem_Click_1(object sender, RoutedEventArgs e) //Правка - Вставить
        {
            parser.entered_num = "";
            TextBox.Text = parser.entered_num;

            MemoryBox.Clear();
            foreach (var c in Clipboard.GetText())
            {
                InputChar(c);
            }
        }


        private void Clear_Log_Click(object sender, RoutedEventArgs e)
        {
            History.Items.Clear();
        }


        private void Change_Log_Click(object sender, RoutedEventArgs e)
        {
            if (History.SelectedItem != null)
            {
                Change_Item_Button.Visibility = Visibility.Visible;
                Change_Item_TextBox.Visibility = Visibility.Visible;

                Change_Item_TextBox.Text = History.SelectedItem.ToString();
                SELECTED_ROW = History.SelectedIndex;
            }
        }


        private void Change_Item_Button_Click(object sender, RoutedEventArgs e)
        {
            string temp = Change_Item_TextBox.Text.Substring(0, Change_Item_TextBox.Text.IndexOf('='));

            History.Items[SELECTED_ROW] = temp + "= " + RPN.CalculateString(temp);

            Change_Item_TextBox.Visibility = Visibility.Collapsed;
            Change_Item_Button.Visibility = Visibility.Collapsed;
        }

        
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D0:
                    bt0_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.D1:
                    bt1_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.D2:
                    bt2_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.D3:
                    bt3_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.D4:
                    bt4_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.D5:
                    bt5_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.D6:
                    bt6_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.D7:
                    bt7_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.D8:
                    bt8_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.D9:
                    bt9_Click(new object(), new RoutedEventArgs());
                    break;

                case Key.NumPad0:
                    bt0_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.NumPad1:
                    bt1_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.NumPad2:
                    bt2_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.NumPad3:
                    bt3_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.NumPad4:
                    bt4_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.NumPad5:
                    bt5_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.NumPad6:
                    bt6_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.NumPad7:
                    bt7_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.NumPad8:
                    bt8_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.NumPad9:
                    bt9_Click(new object(), new RoutedEventArgs());
                    break;

                case Key.Decimal:
                    znak_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.OemPeriod:
                    znak_Click(new object(), new RoutedEventArgs());
                    break;

                case Key.Divide:
                    del_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.Multiply:
                    mult_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.OemPlus:
                    Plus_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.OemMinus:
                    Minus_Click(new object(), new RoutedEventArgs());
                    break;

                case Key.Delete:
                    CE_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.Back:
                    str_Click(new object(), new RoutedEventArgs());
                    break;
                case Key.Enter:
                    InputChar('=');
                    MemoryBox.Clear();
                    break;
            }

        }
    }
}