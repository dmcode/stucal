using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stucal
{
    public partial class Form1 : Form
    {
        const string eq = "=";

        private Dictionary<string, ICalculate> calcs = new Dictionary<string, ICalculate>()
        {
            { Add.op, new Add() }, { Sub.op, new Sub() }, { Mul.op, new Mul() }, { Div.op, new Div() }
        };
        private List<string> expression = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void registerDigit(string digit)
        {
            display.Text += digit;
        }

        private void evaluateExpression(string op)
        {
            if (Array.Exists(calcs.Keys.ToArray(), el => el.Equals(op)))
            {
                // przypadek gdy zamiast znaku `=` dostajemy kolejny operator,
                // liczymy wyrazenie i rezultat odkladamy jako nowe wyrazenie
                if (expression.Count > 0 && Array.Exists(calcs.Keys.ToArray(), el => el.Equals(expression.Last())))
                {
                    expression.Add(this.display.Text);
                    string result = computeExpression(expression);
                    expression.Clear();
                    expression.Add(result);
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
                // przypadek gdy ciagle dostajemy =
                if (expression.Last() == eq)
                {
                    expression[0] = this.display.Text;
                }
                else
                {
                    expression.Add(this.display.Text);
                    expression.Add(op);
                }
                history.Text = String.Join(" ", expression);
                display.Text = computeExpression(expression);
            }
        }

        private string computeExpression(List<string> expression)
        {
            decimal result = 0;
            string op = Add.op;
            foreach(var item in expression)
            {
                if (Array.Exists(calcs.Keys.ToArray(), el => el.Equals(item)))
                {
                    op = item;
                    continue;
                }

                if (item == eq)
                    break;

                ICalculate cal;
                if (calcs.TryGetValue(op, out cal) && op.Length > 0)                
                    result = cal.result(result, Convert.ToDecimal(item));
            }
            return result.ToString();
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
    }

    interface ICalculate
    {
        decimal result(decimal A, decimal B);
    }

    class Add : ICalculate
    {
        public static string op = "+";
        public decimal result(decimal A, decimal B)
        {
            return A + B;
        }
    }

    class Sub : ICalculate
    {
        public static string op = "-";
        public decimal result(decimal A, decimal B)
        {
            return A - B;
        }
    }

    class Mul : ICalculate
    {
        public static string op = "*";
        public decimal result(decimal A, decimal B)
        {
            return A * B;
        }
    }

    class Div : ICalculate
    {
        public static string op = "/";
        public decimal result(decimal A, decimal B)
        {
            if (B == 0)
                throw new InvalidOperationException();
            return A / B;
        }
    }
}
