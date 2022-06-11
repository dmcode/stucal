using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stucal
{
    public partial class Form1 : Form
    {
        const string eq = "=";
        const int MAXLEN = 15;
        static int CURRENT_BASE = 10;

        private Dictionary<string, ICalculate> calcs = new Dictionary<string, ICalculate>()
        {
            { Add.op, new Add() }, { Sub.op, new Sub() }, { Mul.op, new Mul() }, { Div.op, new Div() }
        };
        private List<string> expression;

        public Form1()
        {
            InitializeComponent();
            reset();
            changeBase(10);
            button22.BackColor = Color.Azure;
        }

        private void reset()
        {
            expression = new List<string>();
            history.Text = "";
            display.Text = "0";
            hex.Text = "0";
            dec.Text = "0";
            oct.Text = "0";
            bin.Text = "0";
        }

        private void changeBase(int currentBase)
        {
            CURRENT_BASE = currentBase;
            foreach(Button item in panel2.Controls)
            {
                if (CURRENT_BASE == ConvertibleNumber.BASE_HEX)
                {
                    item.Enabled = true;
                    continue;
                }

                int n;
                var isNumeric = int.TryParse(item.Text, out n);

                if (CURRENT_BASE == ConvertibleNumber.BASE_DEC)
                {
                    item.Enabled = isNumeric;
                    continue;
                }

                if (CURRENT_BASE == ConvertibleNumber.BASE_OCT)
                {
                    item.Enabled = isNumeric && n < 8;
                    continue;
                }

                if (CURRENT_BASE == ConvertibleNumber.BASE_BIN)
                    item.Enabled = isNumeric && n < 2;
            }
            reset();
        }

        private bool equalIsLast()
        {
            return expression.Count > 0 && expression.Last() == eq;
        }

        private bool operatorIsLast()
        {
            return expression.Count > 0 && Array.Exists(calcs.Keys.ToArray(), el => el.Equals(expression.Last()));
        }

        private bool isOperator(string item)
        {
            return Array.Exists(calcs.Keys.ToArray(), el => el.Equals(item));
        }

        private void displayWithConversion(string number)
        {
            IConvertibleNumber formatted = new FormattedNumber(new ConvertibleNumber(number, CURRENT_BASE));
            display.Text = formatted.ToString();
            hex.Text = formatted.ToHex();
            dec.Text = formatted.ToDec();
            oct.Text = formatted.ToOct();
            bin.Text = formatted.ToBin();
        }

        private void registerDigit(string digit)
        {
            if (equalIsLast())
                reset();
            if (display.Text == "0")
                display.Text = "";
            if (display.Text.Length < MAXLEN)
                displayWithConversion(display.Text + digit);
        }

        private void evaluateExpression(string op)
        {
            if (isOperator(op))
            {
                // przypadek gdy zamiast znaku `=` dostajemy kolejny operator,
                // liczymy wyrazenie i rezultat odkladamy jako nowe wyrazenie
                if (operatorIsLast())
                {
                    // dostalismy operator bez wprowadzonej liczby
                    if (this.display.Text.Length < 1)
                    {
                        // to zmieniamy operator
                        expression.RemoveAt(expression.Count - 1);
                    }
                    else
                    {
                        expression.Add(this.display.Text);
                        string result = computeExpression(expression);
                        expression.Clear();
                        expression.Add(result);
                    }
                }
                else if (equalIsLast())
                {
                    expression.Clear();
                    expression.Add(this.display.Text);
                }
                else
                {
                    expression.Add(this.display.Text);
                }
                
                expression.Add(op);
                history.Text = String.Join(" ", expression);
                display.Text = "";
            }
            if (op == eq)
            {
                if (equalIsLast())
                {
                    // przypadek gdy uzytkownik ciagle klika znak =
                    expression[0] = this.display.Text;
                }
                else
                {
                    expression.Add(this.display.Text);
                    expression.Add(op);
                }
                history.Text = String.Join(" ", expression);
                displayWithConversion(computeExpression(expression));
            }
        }

        private string computeExpression(List<string> expression)
        {
            Int64 result = 0;
            string op = Add.op;
            foreach(var item in expression)
            {
                if (isOperator(item))
                {
                    op = item;
                    continue;
                }

                if (item == eq)
                    break;

                ICalculate cal;
                if (calcs.TryGetValue(op, out cal) && op.Length > 0)                
                    result = cal.result(result, new ConvertibleNumber(item, CURRENT_BASE).ToInt64());
            }
            return Convert.ToString(result, CURRENT_BASE);
        }

        private void buttonDigit_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            registerDigit(btn.Text);
        }

        private void buttonOperator_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            evaluateExpression(btn.Text);
        }

        private void buttonResult_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            evaluateExpression(btn.Text);
        }

        private void button21_Click(object sender, EventArgs e)
        {
            reset();
        }

        private void button22_Click(object sender, EventArgs e)
        {
            changeBase(10);
            Button b = sender as Button;
            b.BackColor = Color.Azure;
            button23.BackColor = Color.Gainsboro;
            button24.BackColor = Color.Gainsboro;
            button25.BackColor = Color.Gainsboro;
        }

        private void button23_Click(object sender, EventArgs e)
        {
            changeBase(16);
            Button b = sender as Button;
            b.BackColor = Color.Azure;
            button22.BackColor = Color.Gainsboro;
            button24.BackColor = Color.Gainsboro;
            button25.BackColor = Color.Gainsboro;
        }

        private void button24_Click(object sender, EventArgs e)
        {
            changeBase(8);
            Button b = sender as Button;
            b.BackColor = Color.Azure;
            button22.BackColor = Color.Gainsboro;
            button23.BackColor = Color.Gainsboro;
            button25.BackColor = Color.Gainsboro;
        }

        private void button25_Click(object sender, EventArgs e)
        {
            changeBase(2);
            Button b = sender as Button;
            b.BackColor = Color.Azure;
            button22.BackColor = Color.Gainsboro;
            button23.BackColor = Color.Gainsboro;
            button24.BackColor = Color.Gainsboro;
        }
    }

    interface ICalculate
    {
        Int64 result(Int64 A, Int64 B);
    }

    class Add : ICalculate
    {
        public static string op = "+";
        public Int64 result(Int64 A, Int64 B)
        {
            return A + B;
        }
    }

    class Sub : ICalculate
    {
        public static string op = "-";
        public Int64 result(Int64 A, Int64 B)
        {
            return A - B;
        }
    }

    class Mul : ICalculate
    {
        public static string op = "*";
        public Int64 result(Int64 A, Int64 B)
        {
            return A * B;
        }
    }

    class Div : ICalculate
    {
        public static string op = "/";
        public Int64 result(Int64 A, Int64 B)
        {
            if (B == 0)
                throw new InvalidOperationException();
            return A / B;
        }
    }

    interface IConvertibleNumber
    {
        string Number { get; }
        int FromBase { get; }

        string ToHex();

        string ToDec();

        string ToOct();

        string ToBin();

        Int64 ToInt64();
    }

    class ConvertibleNumber: IConvertibleNumber
    {
        public string Number { get; }
        public int FromBase { get; }
        public const int BASE_HEX = 16;
        public const int BASE_DEC = 10;
        public const int BASE_OCT = 8;
        public const int BASE_BIN = 2;

        public ConvertibleNumber(string number, int fromBase=10)
        {
            this.Number = number.Replace(" ", "");
            this.FromBase = fromBase;
        }

        public override string ToString()
        {
            return Number;
        }

        public string ToHex()
        {
            return Convert.ToString(Convert.ToInt64(Number, FromBase), BASE_HEX);
        }

        public string ToDec()
        {
            return Convert.ToString(Convert.ToInt64(Number, FromBase), BASE_DEC);
        }

        public string ToOct()
        {
            return Convert.ToString(Convert.ToInt64(Number, FromBase), BASE_OCT);
        }

        public string ToBin()
        {
            return Convert.ToString(Convert.ToInt64(Number, FromBase), BASE_BIN);
        }

        public Int64 ToInt64()
        {
            return Convert.ToInt64(Number, FromBase);
        }
    }

    class FormattedNumber: IConvertibleNumber
    {
        private IConvertibleNumber number;

        public string Number {
            get
            {
                return number.ToString();
            }
        }

        public int FromBase
        {
            get
            {
                return number.FromBase;
            }
        }

        public FormattedNumber(IConvertibleNumber number)
        {
            this.number = number;
        }

        public override string ToString()
        {
            switch(number.FromBase)
            {
                case ConvertibleNumber.BASE_HEX: return ToHex();
                case ConvertibleNumber.BASE_DEC: return ToDec();
                case ConvertibleNumber.BASE_OCT: return ToOct();
                case ConvertibleNumber.BASE_BIN: return ToBin();
            }
            return number.ToString();
        }

        public Int64 ToInt64()
        {
            return number.ToInt64();
        }

        public string ToHex()
        {
            var reversed = reverse(number.ToHex());
            return reverse(Regex.Replace(reversed, ".{4}", "$0 "));
        }

        public string ToDec()
        {
            var reversed = reverse(number.ToDec());
            return reverse(Regex.Replace(reversed, ".{3}", "$0 "));
        }

        public string ToOct()
        {
            var reversed = reverse(number.ToOct());
            return reverse(Regex.Replace(reversed, ".{3}", "$0 "));
        }

        public string ToBin()
        {
            var reversed = reverse(number.ToBin());
            return reverse(Regex.Replace(reversed, ".{4}", "$0 "));
        }

        private string reverse(string number)
        {
            return new string(number.ToCharArray().Reverse().ToArray()).Trim();
        }
    }
}
